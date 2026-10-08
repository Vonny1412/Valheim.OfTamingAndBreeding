using HarmonyLib;
using OfTamingAndBreeding.Runtime;

namespace OfTamingAndBreeding.Patches
{
    [HarmonyPatch]
    internal partial class DataReadyPatches : Core.PatchGroup<DataReadyPatches>
    {

        [HarmonyPatch(typeof(Container), "Awake")]
        [HarmonyPostfix]
        private static void Container_Awake_Postfix(Container __instance)
        {
            ContainerFeeding.RegisterContainerInstance(__instance);
        }

        [HarmonyPatch(typeof(Container), "OnDestroyed")]
        [HarmonyPrefix]
        private static void Container_OnDestroy_Prefix(Container __instance)
        {
            ContainerFeeding.UnregisterContainerInstance(__instance);
        }

    }
}
