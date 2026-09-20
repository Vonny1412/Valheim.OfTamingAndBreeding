using HarmonyLib;
using OfTamingAndBreeding.Components;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {

        [HarmonyPatch(typeof(ZSyncAnimation), "SetFloat", new[] { typeof(int), typeof(float) })]
        [HarmonyPrefix]
        private static void ZSyncAnimation_SetFloat_Prefix(ZSyncAnimation __instance, int hash, ref float value)
        {
            if (__instance && ScaledCreature.TryGet(__instance.gameObject, out var scaled))
            {
                value *= scaled.m_animationScale;
            }
        }

    }
}
