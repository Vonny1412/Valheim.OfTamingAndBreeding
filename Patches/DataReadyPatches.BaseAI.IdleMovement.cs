using HarmonyLib;
using OfTamingAndBreeding.Components.Traits;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {

        [HarmonyPatch(typeof(BaseAI), "IdleMovement")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        private static bool BaseAI_IdleMovement_Prefix(BaseAI __instance, float dt)
        {
            var trait = __instance.GetComponent<BaseAITrait>();
            if (trait.IdleMovement(dt)) // handled by trait
            {
                return false; // block original
            }
            return true;
        }

    }
}
