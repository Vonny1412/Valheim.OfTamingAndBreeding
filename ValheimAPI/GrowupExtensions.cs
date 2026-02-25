using Jotunn.Managers;
using OfTamingAndBreeding.Data;
using OfTamingAndBreeding.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;

namespace OfTamingAndBreeding.ValheimAPI
{
    internal static class GrowupExtensions
    {
        internal sealed class GrowupExtraData : Lifecycle.ExtraData<Growup, GrowupExtraData>
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ZNetView GetZNetView(this Growup that)
            => LowLevel.Growup.__IAPI_m_nview_Invoker.Get(that);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BaseAI GetBaseAI(this Growup that)
            => LowLevel.Growup.__IAPI_m_baseAI_Invoker.Get(that);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GameObject GetPrefab(this Growup that)
            => LowLevel.Growup.__IAPI_GetPrefab_Invoker1.Invoke(that);

        public static void Start_PatchPostfix(this Growup growup)
        {

            if (growup.TryGetComponent<Custom.OTAB_GrowupTrait>(out _) == false)
            {
                // we are using late-registration
                // instead of adding component in CreatureProcessor

                var prefabName = Utils.GetPrefabName(growup.gameObject.name);
                var prefab = PrefabManager.Instance.GetPrefab(prefabName);
                var prefabGrowup = prefab.GetComponent<Growup>();

                var c1 = growup.gameObject.gameObject.AddComponent<Custom.OTAB_GrowupTrait>();
                var c2 = prefab.gameObject.AddComponent<Custom.OTAB_GrowupTrait>();

                c1.m_baseGrowTime = prefabGrowup.m_growTime;
                c2.m_baseGrowTime = prefabGrowup.m_growTime;

            }

            growup.UpdateGrowTime();
        }

        public static void UpdateGrowTime(this Growup growup)
        {
            var m_nview = growup.GetZNetView();
            if (!m_nview || !m_nview.IsValid()) return;
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
            growup.UpdateGrowTime(totalFactor);
        }

        private static void UpdateGrowTime(this Growup growup, float totalFactor)
        {
            if (totalFactor >= 0) // yes, we do allow 0, too
            {
                var trait = growup.GetComponent<Custom.OTAB_GrowupTrait>();
                growup.m_growTime = trait.m_baseGrowTime * totalFactor;
            }
        }

        public static bool GrowUpdate_PatchPrefix(this Growup growup)
        {
            var m_nview = growup.GetZNetView();
            if (!m_nview.IsValid() || !m_nview.IsOwner())
            {
                return false;
            }

            if (!(growup.GetBaseAI().GetTimeSinceSpawned().TotalSeconds > (double)growup.m_growTime))
            {
                // still growing
                return false;
            }

            //
            // ready to grow up
            //

            Character myCharacter = growup.GetComponent<Character>();
            GameObject spawned = UnityEngine.Object.Instantiate(growup.GetPrefab(), growup.transform.position, growup.transform.rotation);
            Character spawnedCharacter = spawned.GetComponent<Character>();

            var zdo = m_nview.GetZDO();
            var zdo2 = spawned.GetComponent<ZNetView>().GetZDO();

            if ((bool)spawnedCharacter)
            {
                Tameable tameable1 = growup.GetComponent<Tameable>();
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
                        ZNetHelper.SetString(zdo2, ZDOVars.s_tamedName, name1);
                        ZNetHelper.SetString(zdo2, ZDOVars.s_tamedNameAuthor, nameAuthor1);
                    }
                
                    //
                    // pass fed time to spawned
                    //

                    var oldFedDuration = tameable1.m_fedDuration;
                    var newFedDuration = tameable2.m_fedDuration;
                    if (oldFedDuration > 0 && newFedDuration > 0)
                    {
                        var lastFeeding = zdo.GetLong(ZDOVars.s_tameLastFeeding, 0L);
                        Helpers.ZNetHelper.SetLong(zdo2, ZDOVars.s_tameLastFeeding, lastFeeding);
                    }

                }
                if (growup.m_inheritTame)
                {
                    if (myCharacter.IsTamed())
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
                                Helpers.ZNetHelper.SetFloat(zdo2, ZDOVars.s_tameTimeLeft, newLeft);
                            }
                        }
                    }
                }
                else
                {
                    spawnedCharacter.SetTamed(false);
                }
                spawnedCharacter.SetLevel(myCharacter.GetLevel());
            }
            else
            {
                // just in case someone tries this
                ItemDrop spawnedItem = spawned.GetComponent<ItemDrop>();
                if ((bool)spawnedItem)
                {
                    spawnedItem.SetQuality(myCharacter.GetLevel());
                }
            }

            ThirdParty.Mods.CllCBridge.PassTraits(zdo, spawned);

            m_nview.Destroy();
            return false;
        }

    }
}
