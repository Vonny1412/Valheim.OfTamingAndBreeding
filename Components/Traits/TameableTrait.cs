using OfTamingAndBreeding.Components.Core;
using OfTamingAndBreeding.Components.Extensions;
using OfTamingAndBreeding.Utilities;
using System;
using UnityEngine;


//todo: cleanup



namespace OfTamingAndBreeding.Components.Traits
{
    public class TameableTrait : OTABComponent<TameableTrait>
    {

        // set by registry processor
        [SerializeField] public bool m_feedingDisabled = false;
        [SerializeField] public bool m_tamingDisabled = false;
        [SerializeField] public string m_petCommand = null;
        [SerializeField] public string m_requireGlobalKey = null;

        // set in awake
        [NonSerialized] private ZNetView m_nview = null;
        [NonSerialized] private Tameable m_tameable = null;
        [NonSerialized] private BaseAI m_baseAI = null;
        [NonSerialized] private Character m_character = null;
        [NonSerialized] private AnimalAITrait m_animalAITrait = null;
        [NonSerialized] private BaseAITrait m_baseAITrait = null;
        [NonSerialized] private float m_baseFedDuration = 600;
        [NonSerialized] private float m_baseTamingTime = 1800;

        /*

        Valheim:
        Tameable.TamingUpdate() -> Tameable.Tame();
        Tameable.TameAllInArea() -> Tameable.Tame();
        Tameable.Tame() -> MonsterAI.MakeTame()
        MonsterAI.MakeTame() -> Character.SetTame()
        Character.SetTame() -> InvokeRPC -> Character.RPC_SetTamed()
        
        OTAB Patches:
        Postfix Tameable.Tame() -> TameableTrait.On_Tame() -> TameableTrait.TameAnimal() -> AnimalAITrait.MakeTame() -> Character.SetTamed()
        Postfix Character.RPC_SetTamed() -> CharacterTrait.On_RPC_SetTamed(tamed) -> if (tamed) CharacterTrait.SetTamedCharacteristics()

        */

        private void Awake()
        {
            m_nview = GetComponent<ZNetView>();
            m_tameable = GetComponent<Tameable>();
            m_baseAI = GetComponent<BaseAI>();
            m_character = GetComponent<Character>();
            m_animalAITrait = GetComponent<AnimalAITrait>();
            m_baseAITrait = GetComponent<BaseAITrait>();

            if (m_nview.IsValid())
            {
                m_nview.Register<float>("RPC_UpdateFedDuration", RPC_UpdateFedDuration);

                if (m_nview.IsOwner())
                {
                    var zdo = m_nview.GetZDO();
                    var tamingTime = m_tameable.m_tamingTime;
                    var remainingTime = zdo.GetFloat(ZDOVars.s_tameTimeLeft, tamingTime);
                    if (remainingTime > tamingTime)
                    {
                        zdo.Set(ZDOVars.s_tameTimeLeft, tamingTime);
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

            Register();
        }

        private void Start()
        {
            if (m_tamingDisabled)
            {
                CancelTamingUpdate();
            }
        }

        private void OnDestroy()
        {
            Unregister();
        }

        public Tameable GetTameable() {
            return m_tameable;
        }

        public void CancelTamingUpdate()
        {
            if (m_tameable.IsInvoking("TamingUpdate"))
            {
                m_tameable.CancelInvoke("TamingUpdate");
            }
        }

        public void StartTamingUpdate()
        {
            if (!m_tameable.IsInvoking("TamingUpdate"))
            {
                m_tameable.InvokeRepeating("TamingUpdate", 3f, 3f);
            }
        }

        public bool IsTamingStarted()
        {
            return IsTamingStarted(out _);
        }

        public bool IsTamingStarted(out float remainingTime)
        {
            remainingTime = m_tameable.GetRemainingTime();
            return remainingTime < m_tameable.m_tamingTime;
        }











        public bool CanBeTamed()
        {
            if (!string.IsNullOrEmpty(m_requireGlobalKey) && !ZoneSystem.instance.GetGlobalKey(m_requireGlobalKey))
            {
                return false;
            }
            if (m_baseAITrait.IsConfined())
            {
                return false;
            }
            if (m_baseAI.IsSleeping())
            {
                return false;
            }
            return true;
        }

        public float GetBaseFedDuration()
        {
            return m_baseFedDuration;
        }

        public float GetBaseTamingTime()
        {
            return m_baseTamingTime;
        }

        public bool IsFeedingDisabled()
        {
            return m_feedingDisabled == true;
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
            if (totalFactor >= 0) // we do allow 0 too
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
            if (totalFactor >= 0) // we do allow 0 too
            {
                m_tameable.m_tamingTime = GetBaseTamingTime() * totalFactor;
            }
        }

        public bool On_ConsumedItem(ItemDrop item)
        {
            if (m_nview.IsOwner() == false)
            {
                return true;
            }

            if (IsFeedingDisabled())
            {
                return true;
            }

            if (!m_tameable.IsTamed() && (IsTamingDisabled() || !CanBeTamed()))
            {
                // this just disables the consume effect (pink hearts)
                return true;
            }






            Runtime.ItemConsumeContext.ResolveExternalFeedItem(ref item);
            if (!item)
            {
                return true; // or maybe false?
            }




            if (Plugin.Configs.RequireFoodDroppedByPlayer.Value)
            {
                if (Runtime.ItemConsumeContext.CheckItem(item, out bool droppedByPlayer) && droppedByPlayer == false)
                {
                    // definitly not dropped by player
                    // prevent ResetFeedingTimer
                    return true;
                }
            }





            // prevent catch-up regeneration after feeding
            // todo: add config for this? with true as default?
            m_baseAI.GetWorldTimeDelta();

            // calculate new fed duration based on consumed food
            var customFactor = m_nview.GetZDO().GetFloat(Plugin.ZDOVars.z_fedDurationFactor, 1f);
            if (m_baseAITrait && m_baseAITrait.m_consumeItems != null)
            {
                var sharedName = item.m_itemData.m_shared.m_name;
                var newFactor = 1f;
                foreach (var consumeItem in m_baseAITrait.m_consumeItems)
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
            ZDOUtils.SetFloat(m_nview.GetZDO(), Plugin.ZDOVars.z_fedDurationFactor, customFactor);
            m_nview.InvokeRPC(ZNetView.Everybody, "RPC_UpdateFedDuration", totalFactor);
            UpdateFedDuration(totalFactor);

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

        public bool On_TamingUpdate()
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
                    m_tameable.DecreaseRemainingTime(3f); // original update repeat time is 3f seconds
                    if (m_tameable.GetRemainingTime() <= 0f)
                    {
                        m_tameable.Tame();
                        // note: we patched Tameable.Tame() via postfix !! if its monsterai the originale func will not run
                        // but because of that postfix patch OnTame() of this trait will be called!
                    }
                    else
                    {
                        m_tameable.m_sootheEffect?.Create(m_tameable.transform.position, m_tameable.transform.rotation);
                    }
                }
                return true;
            }

            return false;
        }


        public bool IsTamed()
        {
            return m_tameable.IsTamed();
        }

        public bool IsHungry()
        {
            return m_tameable.IsHungry();
        }

        public bool IsHungry(float delay)
        {
            if (!m_character) return false;
            if (m_nview == null) return false;

            ZDO zDO = m_nview.GetZDO();
            if (zDO == null) return false;
            
            DateTime dateTime = new DateTime(zDO.GetLong(ZDOVars.s_tameLastFeeding, 0L));
            return (ZNet.instance.GetTime() - dateTime).TotalSeconds > m_tameable.m_fedDuration + delay;
        }

        public void On_Tame()
        {
            // remember: the original Tameable.Tame() method only gets called when the creature actually becomes tamed
            // it does not get called for already tamed creates when loading the world
            // use CharacterTrait.SetTamedCharacteristics() to apply changes for every tamed creature whether already or actualy tamed
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

        public bool On_RPC_Command(long sender, ZDOID characterID, bool message)
        {
            // owner already checked

            if (m_animalAITrait)
            {
                Player player = m_tameable.GetPlayer(characterID);
                if (player == null)
                {
                    return true;
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
            var tamingTime = m_tameable.m_tamingTime;
            var remainingTime = m_tameable.GetRemainingTime();
            var percent = (float)(int)((1f - Mathf.Clamp01(remainingTime / tamingTime)) * 100f * precision) / precision;
            var percentText = percent.ToString($"F{decimals}", System.Globalization.CultureInfo.InvariantCulture);
            return Localization.instance.Localize("$otab_hud_tameness", percentText);
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

            text += "\n" + Localization.instance.Localize("$otab_hover_admin_info", $"Fed duration: cur:{m_tameable.m_fedDuration} base:{GetBaseFedDuration()} enabled:" + (IsFeedingDisabled() ? "false" : "true"));
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
