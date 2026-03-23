using HarmonyLib;
using Jotunn;
using OfTamingAndBreeding.Components.Traits;
using UnityEngine;

namespace OfTamingAndBreeding.Patches
{
    internal partial class AlwaysActivePatches
    {

        [HarmonyPatch(typeof(Character), "GetHoverName")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void Character_GetHoverName_Postfix(Character __instance, ref string __result)
        {
            if (CharacterTrait.TryGet(__instance.gameObject, out var trait))
            {
                var text = trait.GetHoverName();
                if (text.Length > 0)
                {
                    __result += " " + text;
                }
            }
        }

    }
}
