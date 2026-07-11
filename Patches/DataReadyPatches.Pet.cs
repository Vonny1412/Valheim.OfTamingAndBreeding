using HarmonyLib;
using OfTamingAndBreeding.Components.Traits;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {

        [HarmonyPatch(typeof(Pet), "UpdateMaterial")]
        [HarmonyPrefix]
        private static bool Pet_UpdateMaterial_Prefix(Pet __instance, bool __runOriginal)
        {
            if (!__runOriginal)
            {
                return false;
            }
            var trait = PetTrait.GetUnsafe(__instance.gameObject);
            trait.UpdateMaterial();
            return false;
        }

    }
}
