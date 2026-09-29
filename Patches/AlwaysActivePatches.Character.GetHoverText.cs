using HarmonyLib;
using OfTamingAndBreeding.Components.Traits;

namespace OfTamingAndBreeding.Patches
{
    internal partial class AlwaysActivePatches
    {

        [HarmonyPatch(typeof(Character), "GetHoverText")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void Character_GetHoverText_Postfix(Character __instance, ref string __result)
        {
            if (CharacterTrait.TryGet(__instance.gameObject, out var trait))
            {
                __result = trait.On_GetHoverText(__result);
            }
        }

    }
}
