using OfTamingAndBreeding.Internals.API;
using OfTamingAndBreeding.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OfTamingAndBreeding.Internals
{

    internal class GrowupAPI : API.Growup
    {

        private static readonly ConditionalWeakTable<Growup, GrowupAPI> instances
            = new ConditionalWeakTable<Growup, GrowupAPI>();

        public static GrowupAPI GetOrCreate(Growup __instance)
        {
            return instances.GetValue(__instance, inst =>
            {
                Lifecycle.CleanupMarks.Mark(inst.GetComponent<ZNetView>());
                return new GrowupAPI(inst);
            });
        }

        public static bool TryGet(Growup __instance, out GrowupAPI api)
            => instances.TryGetValue(__instance, out api);
        public static void Remove(Growup __instance)
            => instances.Remove(__instance);

        public GrowupAPI(Growup __instance) : base(__instance)
        {
        }


        public bool GrowUpdate_Prefix()
        {
            var __instance = (Growup)__IAPI_instance;
            if (!Helpers.ZNetHelper.TryGetZDO(__instance, out ZDO zdo, out ZNetView nview) || !nview.IsOwner())
            {
                return false;
            }
            // m_baseAI is set on awake. shouldnt be null. if its null, i dont care
            if (!(m_baseAI.GetTimeSinceSpawned().TotalSeconds > (double)m_growTime))
            {
                //
                // still growing
                //
                return false;
            }

            //
            // ready to grow up
            //

            Character myCharacter = GetComponent<Character>();
            GameObject spawned = UnityEngine.Object.Instantiate(GetPrefab(), transform.position, transform.rotation);
            Character spawnedCharacter = spawned.GetComponent<Character>();
            if ((bool)myCharacter && (bool)spawnedCharacter)
            {
                if (m_inheritTame)
                {
                    if (myCharacter.IsTamed())
                    {
                        spawnedCharacter.SetTamed(true);
                    }
                    else
                    {
                        Tameable tameable1 = GetComponent<Tameable>();
                        Tameable tameable2 = spawned.GetComponent<Tameable>();
                        if ((bool)tameable1 && (bool)tameable2 && Helpers.ZNetHelper.TryGetZDO(spawned, out ZDO zdo2))
                        {

                            //
                            // pass taming progress to growup
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

                            //
                            // pass fed time to growup (not procentual)
                            //

                            var oldFedDuration = tameable1.m_fedDuration;
                            var newFedDuration = tameable2.m_fedDuration;
                            if (oldFedDuration > 0 && newFedDuration > 0)
                            {
                                var lastFeeding = zdo.GetLong(ZDOVars.s_tameLastFeeding, 0L);
                                Helpers.ZNetHelper.SetLong(zdo2, ZDOVars.s_tameLastFeeding, lastFeeding);
                            }

                        }
                    }
                }
                spawnedCharacter.SetLevel(myCharacter.GetLevel());
            }
            else
            {
                // just in case someone tries this - i'll allow it
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
