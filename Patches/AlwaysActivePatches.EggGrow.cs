using HarmonyLib;
using OfTamingAndBreeding.Components.Extensions;
using OfTamingAndBreeding.Components.Traits;
using System;

namespace OfTamingAndBreeding.Patches
{
    internal partial class AlwaysActivePatches
    {

        [HarmonyPatch(typeof(EggGrow), "GetHoverText")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        private static bool EggGrow_GetHoverText_Prefix(EggGrow __instance, ref string __result)
        {
            // egg status is getting handled in ItemDrop_GetHoverText_Patch
            var itemDrop = __instance.GetComponent<ItemDrop>();
            __result = itemDrop.GetHoverText();
            return false;
        }

    }
}
