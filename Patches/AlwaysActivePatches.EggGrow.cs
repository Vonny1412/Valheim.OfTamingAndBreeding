using HarmonyLib;
using OfTamingAndBreeding.Components.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Patches
{
    internal partial class AlwaysActivePatches
    {

        [HarmonyPatch(typeof(EggGrow), "GetHoverText")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        private static bool EggGrow_GetHoverText_Prefix(EggGrow __instance, ref string __result)
        {
            var item = __instance.GetItemDrop();
            if (!item)
            {
                __result = "";
                return false;
            }
            // egg status is getting handled in ItemDrop_GetHoverText_Patch
            __result = item.GetHoverText();
            return false;
        }

    }
}
