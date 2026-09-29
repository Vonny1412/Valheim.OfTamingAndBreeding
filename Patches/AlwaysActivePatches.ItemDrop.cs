using HarmonyLib;
using OfTamingAndBreeding.Components.Traits;

namespace OfTamingAndBreeding.Patches
{
    internal partial class AlwaysActivePatches
    {
        [HarmonyPatch(typeof(ItemDrop), "GetHoverText")]
        [HarmonyPostfix]
        private static void ItemDrop_GetHoverText_Postfix(ItemDrop __instance, ref string __result)
        {
            if (!EggGrowTrait.TryGet(__instance.gameObject, out var eggGrowTrait))
            {
                return;
            }

            var text = eggGrowTrait.On_GetHoverText();
            if (text.Length == 0)
            {
                return;
            }

            var newlineIndex = __result.IndexOf('\n');
            if (newlineIndex <= 0)
            {
                return;
            }

            __result = __result[..newlineIndex] + " " + text + __result[newlineIndex..];
        }
        
    }
}
