using OfTamingAndBreeding.Components.Base;
using OfTamingAndBreeding.Components.Extensions;
using OfTamingAndBreeding.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OfTamingAndBreeding.Components.Traits
{
    public class TameableTrait : OTABComponent<TameableTrait>
    {
        // set by registry processor
        [SerializeField] public bool m_fedTimerDisabled = false;
        [SerializeField] public bool m_tamingDisabled = false;
        [SerializeField] public float m_starvingGraceFactor = -1; // todo: add bool m_useDefaultStarvingGraceFactor (?)

        // set in awake
        [NonSerialized] private ZNetView m_nview = null;
        [NonSerialized] private Tameable m_tameable = null;
        [NonSerialized] private BaseAI m_baseAI = null;
        [NonSerialized] private Character m_character = null;
        [NonSerialized] private OTABCreature m_otabCreature = null;
        [NonSerialized] private ExtendedAnimaAI m_exAnimalAI = null;
        [NonSerialized] private CharacterTrait m_characterTrait = null;
        [NonSerialized] private float m_baseFedDuration = 600;
        [NonSerialized] private float m_baseTamingTime = 1800;

        private void Awake()
        {
            m_nview = GetComponent<ZNetView>();
            m_tameable = GetComponent<Tameable>();
            m_baseAI = GetComponent<BaseAI>();
            m_otabCreature = GetComponent<OTABCreature>();
            m_character = GetComponent<Character>();
            m_exAnimalAI = GetComponent<ExtendedAnimaAI>();
            m_characterTrait = GetComponent<CharacterTrait>();

            if (m_nview.IsValid())
            {
                m_nview.Register<float>("RPC_UpdateFedDuration", RPC_UpdateFedDuration);
            }

            if (TryGetComponent<ExtendedAnimaAI>(out var exAnimalAI))
            {
                exAnimalAI.m_onConsumedItem = (Action<ItemDrop>)Delegate.Combine(exAnimalAI.m_onConsumedItem, new Action<ItemDrop>(m_tameable.OnConsumedItem));
            }

            m_baseFedDuration = m_tameable.m_fedDuration;
            m_baseTamingTime = m_tameable.m_tamingTime;

            UpdateFedDuration();
            UpdateTamingTime();
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
                //tameable.UpdateFedDuration(1f); // back to base
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
                // should not be possible but whatever
                //tameable.UpdateTamingTime(1f); // back to base
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

            var customFactor = m_nview.GetZDO().GetFloat(Plugin.ZDOVars.z_fedDurationFactor, 1f);

            if (m_otabCreature && m_otabCreature.HasCustomConsumeItems(out var consumeItems))
            {
                var sharedName = item.m_itemData.m_shared.m_name;
                var newFactor = 1f;
                foreach (var consumeItem in consumeItems)
                {
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

            UpdateStarvingTimePoint();
            // calling UpdateStarvingTimePoint before RequireFoodDroppedByPlayer-check
            // because every valid food should stop starvation
            // RequireFoodDroppedByPlayer is only designed to prevent unwanted taming/breeding

            if (Plugin.Configs.RequireFoodDroppedByPlayer.Value)
            {
                /*
                 // todo: remove if unneccessary
                 
                int droppedByAnyPlayer = -1;
                if (item)
                {
                    var item_nview = item.GetComponent<ZNetView>();
                    if (item_nview && item_nview.IsValid())
                    {
                        droppedByAnyPlayer = item_nview.GetZDO().GetInt(Plugin.ZDOVars.z_droppedByAnyPlayer, 0);
                    }
                }
                if (droppedByAnyPlayer < 0 && ConsumeContext.hasValue && item && ConsumeContext.lastItemInstanceId == item.GetInstanceID())
                {
                    droppedByAnyPlayer = ConsumeContext.lastItemDroppedByAnyPlayer;
                }
                if (droppedByAnyPlayer < 0)
                {
                    // we dont know if it has been dropped by player or not
                    // let valheim handle
                }
                if (droppedByAnyPlayer == 0)
                {
                    // definitly not dropped by player
                    // prevent ResetFeedingTimer
                    return false;
                }
                */
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

        public void GetFedTimerHoverText(List<string> lines)
        {
            var zdo = m_nview.GetZDO();
            var L = Localization.instance;

            float secondsFedLeft = GetFedTimeLeft();
            if (m_tameable.m_fedDuration > 0 && secondsFedLeft >= 0)
            {
                // is fed
                if (Plugin.Configs.HoverShowFedTimer.Value)
                {
                    lines.Add(Utils.StringUtils.FormatRelativeTime(
                        secondsFedLeft,
                        labelPositive:      "$otab_hover_fed",
                        labelPositiveAlt:   "$otab_hover_fed_alt",
                        labelNegative:      "$otab_hover_hungry",
                        labelNegativeAlt:   "$otab_hover_hungry_alt",
                        colorPositive:      Plugin.Configs.HoverColorGood.Value,
                        colorNegative:      Plugin.Configs.HoverColorBad.Value
                    ));
                }
            }
            else
            {
                if (Plugin.IsServerDataLoaded() && m_tameable.IsTamed())
                {
                    // starvation is an otab feature

                    var z_starvingAfter = zdo.GetLong(Plugin.ZDOVars.z_starvingAfter, -1);
                    if (z_starvingAfter != -1 && Plugin.Configs.HoverShowStarvingTimer.Value)
                    {
                        // is starving
                        // and starving point is set
                        var now = ZNet.instance.GetTime();
                        var secondsUntillStarving = (new DateTime(z_starvingAfter) - now).TotalSeconds;

                        lines.Add(Utils.StringUtils.FormatRelativeTime(
                            secondsUntillStarving,
                            labelPositive:      "$otab_hover_starving",
                            labelPositiveAlt:   "$otab_hover_starving_alt",
                            labelNegative:      "$otab_hover_starving_alt",
                            labelNegativeAlt:   "$otab_hover_starving_alt",
                            colorPositive:      Plugin.Configs.HoverColorBad.Value,
                            colorNegative:      Plugin.Configs.HoverColorBad.Value
                        ));
                    }
                }
            }
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
            if (m_tamingDisabled == true)
            {
                // taming completly disabled
                return true;
            }

            if (TryGetComponent<ExtendedAnimaAI>(out var exAnimalAI))
            {
                if (m_nview.IsValid() && m_nview.IsOwner() && !m_tameable.IsTamed() && !m_tameable.IsHungry() && !exAnimalAI.IsAlerted())
                {
                    m_tameable.DecreaseRemainingTime(3f); // valheim is also using 3f
                    if (m_tameable.GetRemainingTime() <= 0f)
                    {
                        m_tameable.Tame();
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

        public float GetStarvingGraceFactor()
        {
            if (m_starvingGraceFactor >= 0)
            {
                return m_starvingGraceFactor;
            }
            return Plugin.Configs.DefaultStarvingGraceFactor.Value;
        }

        public long UpdateStarvingTimePoint()
        {
            return UpdateStarvingTimePoint(ZNet.instance.GetTime());
        }

        public long UpdateStarvingTimePoint(DateTime now)
        {
            var zdo = m_nview.GetZDO();
            if (m_nview.IsOwner())
            {
                if (m_tameable.m_fedDuration > 0)
                {
                    // food that does not feed the tameable
                    // should not tregger a reset of starvation point

                    var starvingGraceFactor = GetStarvingGraceFactor();
                    var starvingAfter = now.AddSeconds(m_tameable.m_fedDuration + m_tameable.m_fedDuration * starvingGraceFactor);
                    var ticks = starvingAfter.Ticks;
                    ZNetUtils.SetLong(zdo, Plugin.ZDOVars.z_starvingAfter, ticks);
                    return ticks;
                }
            }
            return zdo.GetLong(Plugin.ZDOVars.z_starvingAfter, -1);
        }

        public bool IsStarving()
        {
            if (!m_nview || !m_nview.IsValid()) return false;
            var zdo = m_nview.GetZDO();

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

            var now = ZNet.instance.GetTime();

            var z_starvingAfter = zdo.GetLong(Plugin.ZDOVars.z_starvingAfter, -1);
            if (z_starvingAfter == -1)
            {
                // not set yet?
                if (m_nview.IsOwner())
                {
                    var s_tameLastFeeding = zdo.GetLong(ZDOVars.s_tameLastFeeding, -1);
                    if (s_tameLastFeeding != -1)
                    {
                        // how valheim stores time: m_nview.GetZDO().Set(ZDOVars.s_tameLastFeeding, ZNet.instance.GetTime().Ticks);
                        z_starvingAfter = UpdateStarvingTimePoint(new DateTime(s_tameLastFeeding));
                    }
                    else
                    {
                        z_starvingAfter = UpdateStarvingTimePoint(now - m_baseAI.GetTimeSinceSpawned());
                    }
                }
            }
            if (z_starvingAfter == -1)
            {
                return false;
            }

            var isStarving = z_starvingAfter < now.Ticks;
            return isStarving;
        }

        public bool OnTame()
        {
            // adding tame status via Tameable.Tame() is still allowed
            // even if taming is disabled for the tameable
            // disabling taming will only disable the taming process, not the Tame() event
            // afterall its called "m_tamingDisabled" (tamING) and not "m_disableGettingTamed"
            if (m_exAnimalAI)
            {
                Game.instance.IncrementPlayerStat(PlayerStatType.CreatureTamed);

                if (m_nview.IsValid() && m_nview.IsOwner() && (bool)m_character && !m_tameable.IsTamed())
                {
                    m_exAnimalAI.MakeTame();
                    m_tameable.m_tamedEffect?.Create(m_tameable.transform.position, m_tameable.transform.rotation); // only for owner is okay
                    Player closestPlayer = Player.GetClosestPlayer(m_tameable.transform.position, 30f);
                    if ((bool)closestPlayer)
                    {
                        closestPlayer.Message(MessageHud.MessageType.Center, m_character.m_name + " $hud_tamedone");
                    }
                }
                return true;
            }
            return false;
        }

        public void OnTamed()
        {
            m_characterTrait.OnTamed();
        }

        public bool RPC_Command(long sender, ZDOID characterID, bool message)
        {
            if (m_exAnimalAI)
            {
                Player player = m_tameable.GetPlayer(characterID);
                if (player == null)
                {
                    return false;
                }

                if ((bool)m_exAnimalAI.GetFollowTarget())
                {
                    m_exAnimalAI.SetFollowTarget(null);
                    m_exAnimalAI.SetPatrolPoint();
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
                    m_exAnimalAI.ResetPatrolPoint();
                    m_exAnimalAI.SetFollowTarget(player.gameObject);
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
            if (m_tameable.IsTamed() || m_tamingDisabled == true)
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

            // hotfix
            if (remainingTime > tamingTime)
            {
                zdo.Set(ZDOVars.s_tameTimeLeft, tamingTime);
            }

            return "";
        }

    }
}
