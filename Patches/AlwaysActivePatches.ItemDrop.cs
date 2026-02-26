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

        [HarmonyPatch(typeof(ItemDrop), "GetHoverText")]
        [HarmonyPostfix]
        private static void ItemDrop_GetHoverText_Postfix(ItemDrop __instance, ref string __result)
        {
            __instance.GetHoverText_PatchPostfix(ref __result);
        }

    }
}
