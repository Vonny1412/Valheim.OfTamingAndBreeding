using HarmonyLib;
using OfTamingAndBreeding.Components.Traits;
using System;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {

        [HarmonyPatch(typeof(Procreation), "IsDue")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        private static void Procreation_IsDue_Prefix(Procreation __instance)
        {
            var trait = __instance.GetComponent<ProcreationTrait>();
            trait.SetRealPregnancyDuration(__instance.m_pregnancyDuration);
        }

        [HarmonyPatch(typeof(Procreation), "Procreate")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        private static bool Procreation_Procreate_Prefix(Procreation __instance)
        {
            try
            {
                if (__instance.TryGetComponent<ProcreationTrait>(out var trait) && trait.OnProcreate() == true)
                {
                    return false; // block
                }
            }
            catch (Exception ex)
            {
                Plugin.LogFatal($"Procreation_Procreate_Prefix: {ex}");
            }
            return true; // fail-open: allow vanilla + other mods
        }

    }
}
