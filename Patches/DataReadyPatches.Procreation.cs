using HarmonyLib;
using OfTamingAndBreeding.Components;
using OfTamingAndBreeding.Components.Extensions;
using OfTamingAndBreeding.ValheimAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {

        [HarmonyPatch(typeof(Procreation), "Awake")]
        [HarmonyPostfix]
        private static void Procreation_Awake_Postfix(Procreation __instance)
        {
            __instance.Awake_PatchPostfix();
        }

        [HarmonyPatch(typeof(Procreation), "IsDue")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        private static void Procreation_IsDue_Prefix(Procreation __instance)
        {
            var trait = __instance.GetComponent<OTAB_ProcreationTrait>();
            trait.m_realPregnancyDuration = __instance.m_pregnancyDuration;
        }

        [HarmonyPatch(typeof(Procreation), "Procreate")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        private static bool Procreation_Procreate_Prefix(Procreation __instance)
        {
            try
            {
                return __instance.Procreate_PatchPrefix(); // override vanilla
            }
            catch (Exception ex)
            {
                Plugin.LogFatal($"Procreation_Procreate_Prefix: {ex}");
                return true; // fail-open: allow vanilla + other mods
            }
        }

    }
}
