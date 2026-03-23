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
            //var trait = __instance.GetComponent<ProcreationTrait>();
            var trait = ProcreationTrait.GetUnsafe(__instance.gameObject);
            trait.SetRealPregnancyDuration(__instance.m_pregnancyDuration);
        }

        [HarmonyPatch(typeof(Procreation), "Procreate")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        private static bool Procreation_Procreate_Prefix(Procreation __instance)
        {
            var trait = ProcreationTrait.GetUnsafe(__instance.gameObject);
            if (trait.OnProcreate())
            {
                return false;
            }
            return true;
        }

    }
}
