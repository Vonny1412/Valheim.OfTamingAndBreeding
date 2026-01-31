using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OfTamingAndBreeding.Patches
{

    // toggle particle effect
    // not growing (too cold) -> show glitter
    // growing (warm) -> hide glitter
    // this works well with our "DisableParticles" option, because:
    /*
                    if (data.Item.DisableParticles)
                    {
                        foreach (var r in egg.GetComponentsInChildren<ParticleSystemRenderer>(true))
                        {
                            UnityEngine.Object.DestroyImmediate(r);
                        }
                    }
    */

    [HarmonyPatch(typeof(EggGrow), "UpdateEffects")]
    static class EggGrow_UpdateEffects_Patch
    {
        static void Postfix(EggGrow __instance, float grow)
        {
            foreach (var r in __instance.GetComponentsInChildren<ParticleSystemRenderer>(true))
            {
                r.enabled = grow == 0;
            }
        }
    }
}
