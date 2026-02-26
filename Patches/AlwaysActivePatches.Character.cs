using HarmonyLib;
using OfTamingAndBreeding.Components.Extensions;
using OfTamingAndBreeding.ValheimAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Patches
{
    internal partial class AlwaysActivePatches
    {

        [HarmonyPatch(typeof(Character), "GetHoverName")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void Character_GetHoverName_Postfix(Character __instance, ref string __result)
        {
            __instance.GetHoverName_PatchPostfix(ref __result);
        }

        [HarmonyPatch(typeof(Character), "GetHoverText")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void Character_GetHoverText_Postfix(Character __instance, ref string __result)
        {
            __instance.GetHoverText_PatchPostfix(ref __result);
        }

    }
}
