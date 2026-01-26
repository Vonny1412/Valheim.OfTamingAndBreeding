using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace OfTamingAndBreeding.Patches
{
    [HarmonyPatch(typeof(Hud), "UpdateCrosshair")]
    static class Hud_UpdateCrosshair_Patch
    {
        static void Postfix(Hud __instance)
        {
            var hover = AccessTools.Field(typeof(Hud), "m_hoverName")?.GetValue(__instance) as TextMeshProUGUI;
            if (hover == null) return;

            float width = 500; // todo: maybe add config for this?

            var rt = hover.rectTransform;
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            hover.SetLayoutDirty();
            hover.SetVerticesDirty();

        }
    }
}
