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
            /*
             
             // todo: can this be removed?
             
            var item = __instance.GetItemDrop();
            if (!item)
            {
                __result = "";
                return false;
            }
            // egg status is getting handled in ItemDrop_GetHoverText_Patch
            __result = item.GetHoverText();
            return false;
            */

            int nl = __result.IndexOf('\n');
            if (nl <= 0)
            {
                return true;
            }

            if (__instance.TryGetComponent<EggGrowTrait>(out var eggGrowTrait))
            {
                var text = eggGrowTrait.GetHoverText();
                if (string.IsNullOrEmpty(text) == false)
                {
                    __result = __result[..nl] + " " + text + __result[nl..];
                }
            }
            return false;
        }

    }
}
