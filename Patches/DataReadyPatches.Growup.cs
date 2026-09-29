using HarmonyLib;
using OfTamingAndBreeding.Components.Traits;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {

        [HarmonyPatch(typeof(Growup), "GrowUpdate")]
        [HarmonyPrefix]
        private static bool Growup_GrowUpdate_Prefix(Growup __instance, bool __runOriginal)
        {
            if (!__runOriginal)
            {
                return false;
            }

            if (GrowupTrait.TryGet(__instance.gameObject, out var trait))
            {
                trait.On_GrowUpdate();
                return false;
            }

            return true;
        }

    }
}
