using OfTamingAndBreeding.Components.Base;
using OfTamingAndBreeding.Components.Extensions;
using OfTamingAndBreeding.Utils;
using System;
using UnityEngine;

namespace OfTamingAndBreeding.Components.Traits
{
    public class GrowupTrait : OTABComponent<GrowupTrait>
    {

        // set in Start
        [NonSerialized] private ZNetView m_nview = null;
        [NonSerialized] private Growup m_growup = null;
        [NonSerialized] private BaseAI m_baseAI = null;
        [NonSerialized] private Character m_character = null;
        [NonSerialized] private float m_baseGrowTime = 600;
        
        private void Start()
        {
            m_nview = GetComponent<ZNetView>();
            m_growup = GetComponent<Growup>();
            m_baseAI = GetComponent<BaseAI>();
            m_character = GetComponent<Character>();

            m_baseGrowTime = m_growup.m_growTime;

            UpdateGrowTime();
        }

        public float GetBaseGrowTime()
        {
            return m_baseGrowTime;
        }

        public void UpdateGrowTime()
        {
            var zdo = m_nview.GetZDO();
            if (zdo == null) return;

            var globalFactor = Plugin.Configs.GlobalGrowTimeFactor.Value;
            if (globalFactor < 0f)
            {
                // should not be possible but whatever
                //growup.UpdateGrowTime(1f); // back to base
                return;
            }
            var totalFactor = globalFactor;
            UpdateGrowTime(totalFactor);
        }

        private void UpdateGrowTime(float totalFactor)
        {
            if (totalFactor >= 0) // yes, we do allow 0, too
            {
                m_growup.m_growTime = GetBaseGrowTime() * totalFactor;
            }
        }

        public bool GrowUpdate()
        {
            if (!m_nview.IsValid() || !m_nview.IsOwner())
            {
                return false;
            }

            if (!(m_baseAI.GetTimeSinceSpawned().TotalSeconds > (double)m_growup.m_growTime))
            {
                // still growing
                return true;
            }

            // ready to grow up

            GameObject spawned = UnityEngine.Object.Instantiate(m_growup.GetPrefab(), m_growup.transform.position, m_growup.transform.rotation);
            Character spawnedCharacter = spawned.GetComponent<Character>();

            var zdo = m_nview.GetZDO();
            var zdo2 = spawned.GetComponent<ZNetView>().GetZDO();

            if ((bool)spawnedCharacter)
            {
                Tameable tameable1 = m_growup.GetComponent<Tameable>();
                Tameable tameable2 = spawned.GetComponent<Tameable>();
                if (tameable1 && tameable2)
                {
                    //
                    // pass custom name to spawned
                    //

                    var name1 = zdo.GetString(ZDOVars.s_tamedName, "");
                    var nameAuthor1 = zdo.GetString(ZDOVars.s_tamedNameAuthor, "");
                    if (name1.Length != 0)
                    {
                        ZNetUtils.SetString(zdo2, ZDOVars.s_tamedName, name1);
                        ZNetUtils.SetString(zdo2, ZDOVars.s_tamedNameAuthor, nameAuthor1);
                    }

                    //
                    // pass fed time to spawned
                    //

                    var oldFedDuration = tameable1.m_fedDuration;
                    var newFedDuration = tameable2.m_fedDuration;
                    if (oldFedDuration > 0 && newFedDuration > 0)
                    {
                        var lastFeeding = zdo.GetLong(ZDOVars.s_tameLastFeeding, 0L);
                        Utils.ZNetUtils.SetLong(zdo2, ZDOVars.s_tameLastFeeding, lastFeeding);
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

                            //
                            // pass taming progress to spawned
                            //

                            var oldTotal = tameable1.m_tamingTime;
                            var newTotal = tameable2.m_tamingTime;

                            if (oldTotal > 0 && newTotal > 0)
                            {
                                var oldLeft = zdo.GetFloat(ZDOVars.s_tameTimeLeft, oldTotal);
                                oldLeft = Mathf.Clamp(oldLeft, 0f, oldTotal);

                                var progress = (oldTotal <= 0f) ? 0f : 1f - (oldLeft / oldTotal);
                                progress = Mathf.Clamp01(progress);

                                var newLeft = (newTotal <= 0f) ? 0f : (1f - progress) * newTotal;
                                Utils.ZNetUtils.SetFloat(zdo2, ZDOVars.s_tameTimeLeft, newLeft);
                            }
                        }
                    }
                }
                else
                {
                    spawnedCharacter.SetTamed(false);
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

            ThirdParty.Mods.CllCBridge.PassTraits(zdo, spawned);

            m_nview.Destroy();
            return true;
        }

        public string GetGrowupProgress(float precision, int decimals)
        {
            var growTime = m_growup.m_growTime;
            var remainingTime = growTime - (float)m_baseAI.GetTimeSinceSpawned().TotalSeconds;

            var percent = (float)(int)((1f - Mathf.Clamp01(remainingTime / growTime)) * 100f * precision) / precision;
            string percentText = percent.ToString($"F{decimals}", System.Globalization.CultureInfo.InvariantCulture);
            return Localization.instance.Localize("$otab_hud_growth", percentText);
        }

    }
}
