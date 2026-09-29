using HarmonyLib;
using OfTamingAndBreeding.Components;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {

        [HarmonyPatch(typeof(ItemDrop), "SetQuality")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void ItemDrop_SetQuality_Postfix(ItemDrop __instance)
        {
            if (ScaledItem.TryGet(__instance.gameObject, out var scaler))
            {
                // we need to multiply because localScale has already been set to variable scaling according to stuff like quality
                __instance.transform.localScale *= scaler.m_scale;
            }
        }

    }
}
