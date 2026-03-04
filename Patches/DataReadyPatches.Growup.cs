using HarmonyLib;
using OfTamingAndBreeding.Components.Traits;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {

        [HarmonyPatch(typeof(Growup), "GrowUpdate")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        private static bool Growup_GrowUpdate_Prefix(Growup __instance)
        {
            if (__instance.TryGetComponent<GrowupTrait>(out var trait) && trait.GrowUpdate())
            {
                return false;
            }
            return true;
        }

    }
}
