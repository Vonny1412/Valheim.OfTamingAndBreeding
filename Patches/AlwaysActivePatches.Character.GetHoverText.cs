using HarmonyLib;
using OfTamingAndBreeding.Components.Traits;
using System.Collections.Generic;
using System.Linq;

namespace OfTamingAndBreeding.Patches
{
    internal partial class AlwaysActivePatches
    {

        [HarmonyPatch(typeof(Character), "GetHoverText")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void Character_GetHoverText_Postfix(Character __instance, ref string __result)
        {
            //var trait = __instance.GetComponent<CharacterTrait>();
            var trait = CharacterTrait.GetUnsafe(__instance.gameObject);
            __result = trait.GetHoverText(__result);
        }

    }
}
