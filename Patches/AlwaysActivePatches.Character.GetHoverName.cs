using HarmonyLib;
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
            var precision = 1f / Plugin.Configs.HudProgressPrecision.Value;
            int decimals = Mathf.Max(0, Mathf.RoundToInt(-Mathf.Log10(precision)));

            if (Plugin.Configs.HudShowTamingProgress.Value && __instance.TryGetComponent<TameableTrait>(out var tameableTrait))
            {
                var tamingProgress = tameableTrait.GetTamingProgress(precision, decimals);
                if (string.IsNullOrEmpty(tamingProgress) == false)
                {
                    __result += " " + tamingProgress;
                }
            }

            if (Plugin.Configs.HudShowOffspringGrowProgress.Value && __instance.TryGetComponent<GrowupTrait>(out var growupTrait))
            {
                var growupProgress = growupTrait.GetGrowupProgress(precision, decimals);
                if (string.IsNullOrEmpty(growupProgress) == false)
                {
                    __result += " " + growupProgress;
                }
            }
        }

    }
}
