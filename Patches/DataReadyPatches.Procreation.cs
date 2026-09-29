using HarmonyLib;
using OfTamingAndBreeding.Components.Traits;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {

        [HarmonyPatch(typeof(Procreation), "IsDue")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        private static void Procreation_IsDue_Prefix(Procreation __instance)
        {
            if (ProcreationTrait.TryGet(__instance.gameObject, out var trait))
            {
                trait.SetRealPregnancyDuration(__instance.m_pregnancyDuration);
            }
        }

        [HarmonyPatch(typeof(Procreation), "Procreate")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        private static bool Procreation_Procreate_Prefix(Procreation __instance, bool __runOriginal)
        {
            if (!__runOriginal)
            {
                return false;
            }

            if (ProcreationTrait.TryGet(__instance.gameObject, out var trait))
            {
                trait.SetRealPregnancyChance(__instance.m_pregnancyChance);
                trait.On_Procreate();
                return false;
            }

            return true;
        }



    }
}
