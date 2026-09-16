using HarmonyLib;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {

        [HarmonyPatch(typeof(Humanoid), "DropItem")]
        [HarmonyPrefix]
        private static void Humanoid_DropItem_Prefix(Humanoid __instance)
        {
            // entry point for RequireFoodDroppedByPlayer-feature
            Runtime.ItemDropContext.DroppedByPlayer = __instance.IsPlayer();
        }

        [HarmonyPatch(typeof(Humanoid), "DropItem")]
        [HarmonyFinalizer]
        private static void Humanoid_DropItem_Finalizer()
        {
            Runtime.ItemDropContext.Clear();
        }

    }
}
