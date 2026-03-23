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
            int nl = __result.IndexOf('\n');
            if (nl <= 0) return;

            if (__instance.TryGetComponent<EggGrowTrait>(out var eggGrowTrait))
            {
                var text = eggGrowTrait.GetEggGrowProgress();
                if (string.IsNullOrEmpty(text) == false)
                {
                    __result = __result[..nl] + " " + text + __result[nl..];
                }
            }
        }
        
    }
}
