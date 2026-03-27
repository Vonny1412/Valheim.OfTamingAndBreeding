using OfTamingAndBreeding.Components.Base;
using OfTamingAndBreeding.Components.Extensions;
using OfTamingAndBreeding.Components.SpecialPrefabs;
using OfTamingAndBreeding.OTABUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OfTamingAndBreeding.Components.Traits
{
    public class TameableTrait : OTABComponent<TameableTrait>
    {

        [NonSerialized] private static readonly List<List<string[]>> _requireGlobalKeys;

        static TameableTrait()
        {
            _requireGlobalKeys = new List<List<string[]>>();

            Net.NetworkSessionManager.Instance.OnSessionClosed += (netsess, dataLoaded) => {
                _requireGlobalKeys.Clear();
            };
        }

        // set by registry processor
        [SerializeField] public bool m_fedTimerDisabled = false;
        [SerializeField] public bool m_tamingDisabled = false;
        [SerializeField] public float m_starvingGraceFactor = -1; // todo: add bool m_useDefaultStarvingGraceFactor (?)
        [SerializeField] public string m_petCommand = null;

        // set in awake
        [NonSerialized] private ZNetView m_nview = null;
        [NonSerialized] private Tameable m_tameable = null;
        [NonSerialized] private BaseAI m_baseAI = null;
        [NonSerialized] private Character m_character = null;
        [NonSerialized] private AnimalAITrait m_animalAITrait = null;
        [NonSerialized] private CharacterTrait m_characterTrait = null;
        [NonSerialized] private BaseAITrait m_baseAITrait = null;
        [NonSerialized] private float m_baseFedDuration = 600;
        [NonSerialized] private float m_baseTamingTime = 1800;

        // helper fields
        [NonSerialized] private bool m_isStarving = false;
        [NonSerialized] private float m_starvingCheckTimer = 0;

        // set in registration
        [SerializeField] private int m_requireGlobalKeysIndex = -1;

        private void Awake()
        {
            m_nview = GetComponent<ZNetView>();
            m_tameable = GetComponent<Tameable>();
            m_baseAI = GetComponent<BaseAI>();
            m_character = GetComponent<Character>();
            m_animalAITrait = GetComponent<AnimalAITrait>();
            m_characterTrait = GetComponent<CharacterTrait>();
            m_baseAITrait = GetComponent<BaseAITrait>();

            if (m_nview.IsValid())
            {
                m_nview.Register<float>("RPC_UpdateFedDuration", RPC_UpdateFedDuration);

                if (m_nview.IsOwner())
                {
                    var zdo = m_nview.GetZDO();

                    // set missing starvation time point
                    if (CanBecomeStarving())
                    {
                        var z_starvingAfter = zdo.GetLong(Plugin.ZDOVars.z_starvingAfter, -1);
                        if (z_starvingAfter == -1)
                        {
                            var s_tameLastFeeding = zdo.GetLong(ZDOVars.s_tameLastFeeding, -1);
                            if (s_tameLastFeeding != -1)
                            {
                                // how valheim stores time: m_nview.GetZDO().Set(ZDOVars.s_tameLastFeeding, ZNet.instance.GetTime().Ticks);
                                z_starvingAfter = UpdateStarvingTimePoint(new DateTime(s_tameLastFeeding));
                            }
                            else
                            {
                                z_starvingAfter = UpdateStarvingTimePoint(ZNet.instance.GetTime() - m_baseAI.GetTimeSinceSpawned());
                            }
                        }
                    }

                    // update invalid remaining taming time
                    if (!IsTamingDisabled())
                    {
                        var tamingTime = m_tameable.m_tamingTime;
                        var remainingTime = zdo.GetFloat(ZDOVars.s_tameTimeLeft, tamingTime);
                        if (remainingTime > tamingTime)
                        {
                            zdo.Set(ZDOVars.s_tameTimeLeft, tamingTime);
                        }
                    }
                }
            }

            if (m_animalAITrait)
            {
                m_animalAITrait.m_onConsumedItem = (Action<ItemDrop>)Delegate.Combine(m_animalAITrait.m_onConsumedItem, new Action<ItemDrop>(m_tameable.OnConsumedItem));
            }

            m_baseFedDuration = m_tameable.m_fedDuration;
            m_baseTamingTime = m_tameable.m_tamingTime;

            UpdateFedDuration();
            UpdateTamingTime();

            Register(this);
        }

        private void OnDestroy()
        {
            Unregister(this);
        }

        public bool IsTamingStarted()
        {
            return m_tameable.GetRemainingTime() < m_tameable.m_tamingTime;
        }

        public bool IsCommandable()
        {
            return m_tameable.m_commandable;
        }

        internal void SetRequiredGlobalKeys(List<string[]> orKeysList)
        {
            m_requireGlobalKeysIndex = _requireGlobalKeys.Count;
            _requireGlobalKeys.Add(orKeysList);
        }

        public bool HasRequiredGlobalKeys(out List<string[]> orKeysList)
        {
            if (m_requireGlobalKeysIndex != -1)
            {
                orKeysList = _requireGlobalKeys[m_requireGlobalKeysIndex];
                return true;
            }
            orKeysList = null;
            return false;
        }

        public bool SolvesRequiredGlobalKeys()
        {
            if (HasRequiredGlobalKeys(out List<string[]> orKeysList))
            {
                foreach (var andKeys in orKeysList)
                {
                    if (andKeys.All((key) => ZoneSystem.instance.GetGlobalKey(key)))
                    {
                        return true;
                    }
                }
                return false;
            }
            return true;
        }

        public bool CanBeTamed()
        {
            if (SolvesRequiredGlobalKeys() == false)
            {
                return false;
            }
            return true;
        }

        public string GetNotTameableReason()
        {
            if (SolvesRequiredGlobalKeys() == false)
            {
                if (HasRequiredGlobalKeys(out List<string[]> orKeysList))
                {
                    // just take first AND-list for now
                    // todo: maybe display full list?
                    var andList = orKeysList[0];
                    var outList = String.Join(", ", andList.Select((k) => Localization.instance.Localize($"$OTAB_require_key_{k}")));
                    return Localization.instance.Localize("$otab_taming_requires_key", outList);
                }
            }
            return "";
        }

        public float GetBaseFedDuration()
        {
            return m_baseFedDuration;
        }

        public float GetBaseTamingTime()
        {
            return m_baseTamingTime;
        }

        public bool IsFedTimerDisabled()
        {
            return m_fedTimerDisabled == true;
        }

        public bool IsTamingDisabled()
        {
            return m_tamingDisabled == true;
        }

        private void RPC_UpdateFedDuration(long sender, float totalFactor)
        {
            if (!m_nview || !m_nview.IsValid()) return;

            if (!m_nview.IsOwner()) // because already updated
            {
                UpdateFedDuration(totalFactor);
            }
        }

        public void UpdateFedDuration()
        {
            if (!m_nview || !m_nview.IsValid()) return;

            var globalFactor = Plugin.Configs.GlobalFedDurationFactor.Value;
            if (globalFactor < 0f)
            {
                return;
            }
            var customFactor = m_nview.GetZDO().GetFloat(Plugin.ZDOVars.z_fedDurationFactor, 1f);
            var totalFactor = globalFactor * customFactor;
            UpdateFedDuration(totalFactor);
        }

        private void UpdateFedDuration(float totalFactor)
        {
            if (totalFactor >= 0) // yes, we do allow 0, too
            {
                m_tameable.m_fedDuration = GetBaseFedDuration() * totalFactor;
            }
        }

        public void UpdateTamingTime()
        {
            if (!m_nview || !m_nview.IsValid()) return;

            var globalFactor = Plugin.Configs.GlobalTamingTimeFactor.Value;
            if (globalFactor < 0f)
            {
                return;
            }
            var totalFactor = globalFactor;
            UpdateTamingTime(totalFactor);
        }

        private void UpdateTamingTime(float totalFactor)
        {
            if (totalFactor >= 0) // yes, we do allow 0, too
            {
                m_tameable.m_tamingTime = GetBaseTamingTime() * totalFactor;
            }
        }

        public bool OnConsumedItem(ItemDrop item)
        {
            if (m_nview.IsOwner() == false)
            {
                return true;
            }

            if (IsFedTimerDisabled())
            {
                return true;
            }

            if (!m_tameable.IsTamed() && (IsTamingDisabled() || !CanBeTamed()))
            {
                return true;
            }

            var customFactor = m_nview.GetZDO().GetFloat(Plugin.ZDOVars.z_fedDurationFactor, 1f);

            if (m_baseAITrait && m_baseAITrait.HasCustomConsumeItems(out var consumeItems))
            {
                var sharedName = item.m_itemData.m_shared.m_name;
                var newFactor = 1f;

                foreach (var consumeItem in consumeItems)
                {
                    if (OTABSpecialConsumableItem.TryGet(consumeItem.itemDrop.gameObject, out var component))
                    {
                        if (component.Compare(item))
                        {
                            newFactor *= consumeItem.fedDurationFactor;
                        }
                        continue;
                    }
                    if (consumeItem.itemDrop.m_itemData.m_shared.m_name == sharedName)
                    {
                        newFactor *= consumeItem.fedDurationFactor;
                        // what if the same prefab exists multiple times in list?
                        // just keep going, maybe one day we gonna expand the feature with more options or values
                        //break;
                    }
                }
                if (newFactor >= 0) // Intentionally allow 0
                {
                    customFactor = newFactor;
                }
            }

            var globalFactor = Plugin.Configs.GlobalFedDurationFactor.Value;
            var totalFactor = customFactor * globalFactor;
            ZNetUtils.SetFloat(m_nview.GetZDO(), Plugin.ZDOVars.z_fedDurationFactor, customFactor);
            m_nview.InvokeRPC(ZNetView.Everybody, "RPC_UpdateFedDuration", totalFactor);
            UpdateFedDuration(totalFactor);

            UpdateStarvingTimePoint(ZNet.instance.GetTime());
            // calling UpdateStarvingTimePoint before RequireFoodDroppedByPlayer-check
            // because every valid food should stop starvation
            // RequireFoodDroppedByPlayer is only designed to prevent unwanted taming/breeding

            if (Plugin.Configs.RequireFoodDroppedByPlayer.Value)
            {
                if (StaticContext.ItemConsumeContext.hasValue && item && StaticContext.ItemConsumeContext.lastItemInstanceId == item.GetInstanceID())
                {
                    var droppedByAnyPlayer = StaticContext.ItemConsumeContext.lastItemDroppedByAnyPlayer;
                    if (droppedByAnyPlayer == 0)
                    {
                        // definitly not dropped by player
                        // prevent ResetFeedingTimer
                        return true;
                    }
                }
            }

            // not handled, let valheim handle
            return false;
        }

        public float GetFedTimeLeft()
        {
            long lastFedTimeLong = m_nview.GetZDO().GetLong(ZDOVars.s_tameLastFeeding, 0L);
            double secLeft;
            if (lastFedTimeLong == 0)
            {
                // never fed -> treat as "unfed since spawn"
                secLeft = -m_baseAI.GetTimeSinceSpawned().TotalSeconds;
            }
            else
            {
                var lastFedTime = new DateTime(lastFedTimeLong);
                secLeft = m_tameable.m_fedDuration - (ZNet.instance.GetTime() - lastFedTime).TotalSeconds;
            }
            return (float)secLeft;
        }

        public string GetFedTimerHoverText()
        {
            if (IsFedTimerDisabled())
            {
                return "";
            }

            var zdo = m_nview.GetZDO();

            float secondsFedLeft = GetFedTimeLeft();
            if (m_tameable.m_fedDuration > 0 && secondsFedLeft >= 0)
            {
                // is fed
                if (Plugin.Configs.HoverShowFedTimer.Value)
                {
                    return OTABUtils.StringUtils.FormatRelativeTime(
                        secondsFedLeft,
                        labelPositive:      "$otab_hover_fed",
                        labelPositiveAlt:   "$otab_hover_fed_alt",
                        labelNegative:      "$otab_hover_hungry",
                        labelNegativeAlt:   "$otab_hover_hungry_alt",
                        colorPositive:      Plugin.Configs.HoverColorGood.Value,
                        colorNegative:      Plugin.Configs.HoverColorBad.Value
                    );
                }
            }
            else
            {
                if (CanBecomeStarving())
                {
                    var z_starvingAfter = zdo.GetLong(Plugin.ZDOVars.z_starvingAfter, -1);
                    if (z_starvingAfter != -1 && Plugin.Configs.HoverShowStarvingTimer.Value)
                    {
                        // is starving
                        // and starving point is set
                        var now = ZNet.instance.GetTime();
                        var secondsUntillStarving = (new DateTime(z_starvingAfter) - now).TotalSeconds;

                        return OTABUtils.StringUtils.FormatRelativeTime(
                            secondsUntillStarving,
                            labelPositive:      "$otab_hover_starving",
                            labelPositiveAlt:   "$otab_hover_starving_alt",
                            labelNegative:      "$otab_hover_starving_alt",
                            labelNegativeAlt:   "$otab_hover_starving_alt",
                            colorPositive:      Plugin.Configs.HoverColorBad.Value,
                            colorNegative:      Plugin.Configs.HoverColorBad.Value
                        );
                    }
                }
            }
            return "";
        }

        public float GetRemainingTimeDecreaseFactor()
        {
            int stars = Mathf.Max(0, m_character.GetLevel() - 1);
            float slowdown = Plugin.Configs.TamingSlowdownPerStar.Value;
            float divisor = 1f + (stars * slowdown);
            if (divisor > 0f) // safety
            {
                return 1 / divisor; // because we wanna return mul factor
            }
            return 1;
        }

        public bool OnTamingUpdate()
        {
            if (IsTamingDisabled() == true)
            {
                // taming completly disabled
                return true; // handled
            }

            if (CanBeTamed() == false)
            {
                // currently not allowed
                return true; // handled
            }

            if (m_animalAITrait)
            {
                if (m_nview.IsValid() && m_nview.IsOwner() && !m_tameable.IsTamed() && !m_tameable.IsHungry() && !m_animalAITrait.IsAlerted())
                {
                    m_tameable.DecreaseRemainingTime(3f); // valheim is also using 3f
                    if (m_tameable.GetRemainingTime() <= 0f)
                    {
                        m_tameable.Tame();
                        // note: calling Tameable.Tame() will trigger OnTame() and OnTamed() of this component
                    }
                    else
                    {
                        // todo: broadcast me
                        m_tameable.m_sootheEffect?.Create(m_tameable.transform.position, m_tameable.transform.rotation);
                    }
                }
                return true;
            }

            return false;
        }


        public bool CanBecomeStarving()
        {
            if (!Plugin.IsServerDataLoaded())
            {
                return false;
            }

            if (Plugin.Configs.EnableStarvationSystem.Value == false)
            {
                return false;
            }

            if (m_tameable.IsTamed() == false)
            {
                // starvation is a feature for tamed creatures only
                return false;
            }

            if (m_fedTimerDisabled)
            {
                // creatures that do not eat at all wont get starving!
                return false;
            }

            return true;
        }

        public float GetStarvingGraceFactor()
        {
            if (m_starvingGraceFactor >= 0)
            {
                return m_starvingGraceFactor;
            }
            return Plugin.Configs.DefaultStarvingGraceFactor.Value;
        }

        public long UpdateStarvingTimePoint(DateTime fedTime)
        {
            if (!CanBecomeStarving())
            {
                return 0L;
            }

            var zdo = m_nview.GetZDO();
            if (m_nview.IsOwner())
            {
                if (m_tameable.m_fedDuration > 0)
                {
                    // food that does not feed the tameable
                    // should not tregger a reset of starvation point

                    var starvingGraceFactor = GetStarvingGraceFactor();
                    var starvingAfter = fedTime.AddSeconds(m_tameable.m_fedDuration + m_tameable.m_fedDuration * starvingGraceFactor);
                    var ticks = starvingAfter.Ticks;
                    ZNetUtils.SetLong(zdo, Plugin.ZDOVars.z_starvingAfter, ticks);
                    return ticks;
                }
            }
            return zdo.GetLong(Plugin.ZDOVars.z_starvingAfter, -1);
        }

        public bool IsTamed()
        {
            return m_tameable.IsTamed();
        }

        public bool IsHungry()
        {
            return m_tameable.IsHungry();
        }

        public bool IsStarving()
        {
            if (!m_nview || !m_nview.IsValid())
            {
                return false;
            }

            if (!CanBecomeStarving())
            {
                return false;
            }

            var timeNow = Time.time;
            if (timeNow - m_starvingCheckTimer < 1)
            {
                return m_isStarving;
            }
            m_starvingCheckTimer = timeNow;

            var zdo = m_nview.GetZDO();
            var z_starvingAfter = zdo.GetLong(Plugin.ZDOVars.z_starvingAfter, -1);
            if (z_starvingAfter == -1)
            {
                m_isStarving = false;
                return m_isStarving;
            }

            m_isStarving = z_starvingAfter < ZNet.instance.GetTime().Ticks;
            return m_isStarving;
        }

        public void OnTame()
        {
            TameAnimal();
        }
       
        private void TameAnimal()
        {
            if (m_animalAITrait && m_nview.IsValid() && m_nview.IsOwner() && (bool)m_character && !m_tameable.IsTamed())
            {
                m_animalAITrait.MakeTame();
                var t = m_tameable.transform;
                m_tameable.m_tamedEffect?.Create(t.position, t.rotation); // only for owner is okay
                Player closestPlayer = Player.GetClosestPlayer(t.position, 30f);
                if ((bool)closestPlayer)
                {
                    closestPlayer.Message(MessageHud.MessageType.Center, m_character.m_name + " $hud_tamedone");
                }
            }
        }

        public bool RPC_Command(long sender, ZDOID characterID, bool message)
        {
            // owner already checked

            if (m_animalAITrait)
            {
                Player player = m_tameable.GetPlayer(characterID);
                if (player == null)
                {
                    return false;
                }

                if ((bool)m_animalAITrait.GetFollowTarget())
                {
                    m_animalAITrait.SetFollowTarget(null);
                    m_animalAITrait.SetPatrolPoint();
                    if (m_nview.IsOwner())
                    {
                        m_nview.GetZDO().Set(ZDOVars.s_follow, "");
                    }

                    if (message)
                    {
                        player.Message(MessageHud.MessageType.Center, m_tameable.GetHoverName() + " $hud_tamestay");
                    }
                }
                else
                {
                    m_animalAITrait.ResetPatrolPoint();
                    m_animalAITrait.SetFollowTarget(player.gameObject);
                    if (m_nview.IsOwner())
                    {
                        m_nview.GetZDO().Set(ZDOVars.s_follow, player.GetPlayerName());
                    }

                    if (message)
                    {
                        player.Message(MessageHud.MessageType.Center, m_tameable.GetHoverName() + " $hud_tamefollow");
                    }

                    // well, the following is realy hard to port to AnimalAI
                    // but maybe i dont need to

                    //int num = m_nview.GetZDO().GetInt(ZDOVars.s_maxInstances);
                    //if (num > 0)
                    //{
                    //UnsummonMaxInstances(num);
                    //}
                }
                //m_unsummonTime = 0f;
                return true;
            }
            return false;
        }

        public string GetTamingProgress(float precision, int decimals)
        {
            if (!m_tameable || !m_nview.IsValid() || m_tameable.IsTamed() || IsTamingDisabled() || !CanBeTamed())
            {
                return "";
            }

            var zdo = m_nview.GetZDO();
            
            var tamingTime = m_tameable.m_tamingTime;
            var remainingTime = zdo.GetFloat(ZDOVars.s_tameTimeLeft, tamingTime);
            if (remainingTime < tamingTime)
            {
                var percent = (float)(int)((1f - Mathf.Clamp01(remainingTime / tamingTime)) * 100f * precision) / precision;
                string percentText = percent.ToString($"F{decimals}", System.Globalization.CultureInfo.InvariantCulture);
                return Localization.instance.Localize("$otab_hud_tameness", percentText);
            }

            return "";
        }

        public string GetName()
        {
            return m_tameable.GetName();
        }

        public string GetAdminHoverInfoText()
        {
            if (!m_nview.IsValid())
            {
                return "";
            }

            var zdo = m_nview.GetZDO();
            var text = "";

            var fedDurationFactor = zdo.GetFloat(Plugin.ZDOVars.z_fedDurationFactor, 1f);
            var tamingTimeDecreaseFactor = GetRemainingTimeDecreaseFactor();

            text += "\n" + Localization.instance.Localize("$otab_hover_admin_info", $"Fed duration: cur:{m_tameable.m_fedDuration} base:{GetBaseFedDuration()} enabled:" + (IsFedTimerDisabled() ? "false" : "true"));
            text += "\n" + Localization.instance.Localize("$otab_hover_admin_info", "Duration factor: " + fedDurationFactor);
            if (!m_tameable.IsTamed())
            {
                text += "\n" + Localization.instance.Localize("$otab_hover_admin_info", $"Taming time: cur:{m_tameable.m_tamingTime} base:{GetBaseTamingTime()} enabled:" + (IsTamingDisabled() ? "false" : "true"));
                text += "\n" + Localization.instance.Localize("$otab_hover_admin_info", "Decrease factor: " + tamingTimeDecreaseFactor);
            }

            return text;
        }

    }
}
