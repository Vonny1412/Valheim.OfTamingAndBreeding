using HarmonyLib;
using OfTamingAndBreeding.Components.Extensions;
using OfTamingAndBreeding.ValheimAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            __result = __instance.CanGrow_PatchPostfix();
        }

        [HarmonyPatch(typeof(EggGrow), "GrowUpdate")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        private static bool EggGrow_GrowUpdate_Prefix(EggGrow __instance)
        {
            return __instance.GrowUpdate_PatchPrefix();
        }

        [HarmonyPatch(typeof(EggGrow), "Start")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void EggGrow_Start_Postfix(EggGrow __instance)
        {
            __instance.Start_PatchPostfix();
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

    }
}
