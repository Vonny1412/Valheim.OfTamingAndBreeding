using HarmonyLib;
using OfTamingAndBreeding.Components.Traits;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {

        [HarmonyPatch(typeof(BaseAI), "IdleMovement")]
        [HarmonyPrefix]
        private static bool BaseAI_IdleMovement_Prefix(BaseAI __instance, bool __runOriginal, float dt)
        {
            if (!__runOriginal)
            {
                return false;
            }

            var trait = BaseAITrait.GetUnsafe(__instance.gameObject);
            if (trait.IdleMovement(dt))
            {
                return false; // block original
            }
            return true;
        }

    }
}
