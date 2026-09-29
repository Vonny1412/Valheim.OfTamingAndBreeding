using HarmonyLib;
using OfTamingAndBreeding.Components.Traits;

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
                var text = trait.On_GetHoverName();
                if (text.Length > 0)
                {
                    __result += " " + text;
                }
            }
        }

    }
}
