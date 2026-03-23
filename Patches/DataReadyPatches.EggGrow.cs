using HarmonyLib;
using OfTamingAndBreeding.Components;
using OfTamingAndBreeding.Components.Traits;
using OfTamingAndBreeding.OTABUtils;
using UnityEngine;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {

        [HarmonyPatch(typeof(EggGrow), "CanGrow")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void EggGrow_CanGrow_Postfix(EggGrow __instance, ref bool __result)
        {
            // postfix = less calls
            if (__result == false)
            {
                // cannot grow afterall
                return;
            }

            //var trait = __instance.GetComponent<EggGrowTrait>();
            var trait = EggGrowTrait.GetUnsafe(__instance.gameObject);
            if (trait.CanGrow() == false)
            {
                __result = false;
            }
        }

        [HarmonyPatch(typeof(EggGrow), "UpdateEffects")]
        [HarmonyPostfix]
        private static void EggGrow_UpdateEffects_Postfix(EggGrow __instance, float grow)
        {
            foreach (var r in __instance.GetComponentsInChildren<ParticleSystemRenderer>(true))
            {
                r.enabled = grow == 0;
            }
        }

        [HarmonyPatch(typeof(EggGrow), "GrowUpdate")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        private static bool EggGrow_GrowUpdate_Prefix(EggGrow __instance)
        {
            var trait = EggGrowTrait.GetUnsafe(__instance.gameObject);
            if (trait.GrowUpdate())
            {
                return false;
            }
            return true;
        }

    }
}
