using HarmonyLib;
using System;
using TMPro;
using UnityEngine;

namespace OfTamingAndBreeding.Patches
{

    internal partial class AlwaysActivePatches
    {
        [HarmonyPatch(typeof(Hud), "UpdateCrosshair")]
        [HarmonyPostfix]
        private static void Hud_UpdateCrosshair_Postfix(Hud __instance)
        {
            var hover = AccessTools.Field(typeof(Hud), "m_hoverName")?.GetValue(__instance) as TextMeshProUGUI;
            if (hover == null) return;

            float width = 500; // todo: maybe add config for this?
            var rt = hover.rectTransform;
            if (rt.rect.width < width)
            {
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
                hover.SetLayoutDirty();
                hover.SetVerticesDirty();
            }
        }
    }
}
