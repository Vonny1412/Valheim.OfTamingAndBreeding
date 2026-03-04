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
            StaticContext.ItemDropContext.DroppedByPlayer = __instance.IsPlayer() ? 1 : 0;
        }

        [HarmonyPatch(typeof(Humanoid), "DropItem")]
        [HarmonyFinalizer]
        private static void Humanoid_DropItem_Finalizer()
        {
            StaticContext.ItemDropContext.Clear();
        }

    }
}
