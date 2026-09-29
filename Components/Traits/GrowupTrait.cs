using OfTamingAndBreeding.Components.Core;
using OfTamingAndBreeding.Components.Extensions;
using OfTamingAndBreeding.Utilities;
using System;
using UnityEngine;


//todo: cleanup



namespace OfTamingAndBreeding.Components.Traits
{
    public class GrowupTrait : OTABComponent<GrowupTrait>
    {

        [SerializeField] public bool m_requireFeeding = false;
        [SerializeField] public string m_requireGlobalKey = null;

        [NonSerialized] private ZNetView m_nview = null;
        [NonSerialized] private Growup m_growup = null;
        [NonSerialized] private Character m_character = null;
        [NonSerialized] private Tameable m_tameable = null;
        [NonSerialized] private float m_baseGrowTime = 600;

        private void Awake()
        {
            m_nview = GetComponent<ZNetView>();
            m_growup = GetComponent<Growup>();
            m_character = GetComponent<Character>();
            m_tameable = GetComponent<Tameable>();

            m_baseGrowTime = m_growup.m_growTime;

            if (m_nview.IsValid() && m_nview.IsOwner())
            {
                var zdo = m_nview.GetZDO();
                var growTime = m_growup.m_growTime;
                var remainingTime = zdo.GetFloat(Plugin.ZDOVars.z_growTimeLeft, growTime);
                if (remainingTime > growTime)
                {
                    zdo.Set(Plugin.ZDOVars.z_growTimeLeft, growTime);
                }
            }

            UpdateGrowTime();

            Register();
        }

        private void OnDestroy()
        {
            Unregister();
        }

        public Growup GetGrowup() {
            return m_growup;
        }

        public float GetBaseGrowTime()
        {
            return m_baseGrowTime;
        }

        /*
        public void UpdateGrowTime()
        {
            if (!m_nview.IsValid()) return;

            var globalFactor = Plugin.Configs.GlobalGrowTimeFactor.Value;
            if (globalFactor >= 0) // yes, we do allow 0, too
            {
                m_growup.m_growTime = GetBaseGrowTime() * globalFactor;
            }
        }
        */

        public void UpdateGrowTime()
        {
            if (!m_nview.IsValid()) return;

            var globalFactor = Plugin.Configs.GlobalGrowTimeFactor.Value;
            if (globalFactor < 0) return; // yes, we also allow == 0

            var oldGrowTime = m_growup.m_growTime;
            var growingStarted = IsGrowingStarted(out var oldRemainingTime);

            var newGrowTime = GetBaseGrowTime() * globalFactor;

            // Keep the original Growup component up to date
            m_growup.m_growTime = newGrowTime;

            if (!m_nview.IsOwner() || !growingStarted)
            {
                return;
            }

            float newRemainingTime;

            if (oldGrowTime > 0f)
            {
                var remainingFactor = Mathf.Clamp01(oldRemainingTime / oldGrowTime);
                newRemainingTime = newGrowTime * remainingFactor;
            }
            else
            {
                newRemainingTime = 0f;
            }

            ZDOUtils.SetFloat(m_nview.GetZDO(), Plugin.ZDOVars.z_growTimeLeft, newRemainingTime, oldRemainingTime);
        }

        public bool CanGrow()
        {
            return CanGrow(out var _);
        }

        public bool CanGrow(out int reason)
        {
            if (!string.IsNullOrEmpty(m_requireGlobalKey) && !ZoneSystem.instance.GetGlobalKey(m_requireGlobalKey))
            {
                reason = 1;
                return false;
            }

            if (m_requireFeeding && m_tameable && m_tameable.IsHungry())
            {
                reason = 2;
                return false;
            }

            reason = 0;
            return true;
        }

        public bool IsGrowingStarted(out float remainingTime)
        {
            remainingTime = GetRemainingTime();
            return remainingTime < m_growup.m_growTime;
        }

        private void DecreaseRemainingTime(float time)
        {
            var remainingTime = GetRemainingTime();
            remainingTime -= time;
            if (remainingTime < 0f)
            {
                remainingTime = 0f;
            }
            ZDOUtils.SetFloat(m_nview.GetZDO(), Plugin.ZDOVars.z_growTimeLeft, remainingTime);
        }

        public float GetRemainingTime()
        {
            if (!m_nview.IsValid())
            {
                return 0f;
            }
            return m_nview.GetZDO().GetFloat(Plugin.ZDOVars.z_growTimeLeft, m_growup.m_growTime);
        }

        public void On_GrowUpdate()
        {
            if (!m_nview.IsValid() || !m_nview.IsOwner())
            {
                return;
            }

            if (!CanGrow())
            {
                return;
            }

            DecreaseRemainingTime(10f);

            if (GetRemainingTime() > 0f)
            {
                return;
            }

            GrowUp();
        }

        private void GrowUp()
        {
            var t = transform;

            GameObject spawned = UnityEngine.Object.Instantiate(m_growup.GetPrefab(), t.position, t.rotation);
            Character spawnedCharacter = spawned.GetComponent<Character>();

            var zdo = m_nview.GetZDO();
            var nview2 = spawned.GetComponent<ZNetView>();
            var zdo2 = nview2.GetZDO();

            if ((bool)spawnedCharacter)
            {

                // keep old spawnpoint
                if (nview2.IsOwner())
                {
                    var spawnPoint = zdo.GetVec3(ZDOVars.s_spawnPoint, t.position);
                    zdo2.Set(ZDOVars.s_spawnPoint, spawnPoint);
                }

                Tameable tameable1 = m_growup.GetComponent<Tameable>();
                Tameable tameable2 = spawned.GetComponent<Tameable>();
                if (tameable1 && tameable2)
                {
                    // pass custom name to spawned
                    var name1 = zdo.GetString(ZDOVars.s_tamedName, "");
                    var nameAuthor1 = zdo.GetString(ZDOVars.s_tamedNameAuthor, "");
                    if (name1.Length != 0)
                    {
                        ZDOUtils.SetString(zdo2, ZDOVars.s_tamedName, name1);
                        ZDOUtils.SetString(zdo2, ZDOVars.s_tamedNameAuthor, nameAuthor1);
                    }

                    // pass fed time to spawned
                    var oldFedDuration = tameable1.m_fedDuration;
                    var newFedDuration = tameable2.m_fedDuration;
                    if (oldFedDuration > 0 && newFedDuration > 0)
                    {
                        var lastFeeding = zdo.GetLong(ZDOVars.s_tameLastFeeding, 0L);
                        Utilities.ZDOUtils.SetLong(zdo2, ZDOVars.s_tameLastFeeding, lastFeeding);
                    }

                }
                if (m_growup.m_inheritTame)
                {
                    if (m_character.IsTamed())
                    {
                        spawnedCharacter.SetTamed(true);
                    }
                    else
                    {
                        if ((bool)tameable1 && (bool)tameable2)
                        {

                            // pass taming progress to spawned
                            var oldTotal = tameable1.m_tamingTime;
                            var newTotal = tameable2.m_tamingTime;
                            if (oldTotal > 0 && newTotal > 0)
                            {
                                var oldLeft = zdo.GetFloat(ZDOVars.s_tameTimeLeft, oldTotal);
                                oldLeft = Mathf.Clamp(oldLeft, 0f, oldTotal);

                                var progress = (oldTotal <= 0f) ? 0f : 1f - (oldLeft / oldTotal);
                                progress = Mathf.Clamp01(progress);

                                var newLeft = (newTotal <= 0f) ? 0f : (1f - progress) * newTotal;
                                Utilities.ZDOUtils.SetFloat(zdo2, ZDOVars.s_tameTimeLeft, newLeft);
                            }
                        }
                    }
                }
                spawnedCharacter.SetLevel(m_character.GetLevel());
            }
            else
            {
                // just in case someone tries this
                ItemDrop spawnedItem = spawned.GetComponent<ItemDrop>();
                if ((bool)spawnedItem)
                {
                    spawnedItem.SetQuality(m_character.GetLevel());
                }
            }

            Integrations.Mods.CllCBridge.PassTraits(zdo, spawned);

            m_nview.Destroy();
        }

    }
}
