using HarmonyLib;
using UnityEngine;

namespace OfTamingAndBreeding.Patches
{
    internal partial class AlwaysActivePatches
    {

        [HarmonyPatch(typeof(Hud), "Awake")]
        [HarmonyPostfix]
        private static void Hud_Awake_Postfix(Hud __instance)
        {
            var hover = __instance.m_hoverName;
            if (!hover)
            {
                return;
            }
            const float width = 600f;
            hover.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            hover.SetLayoutDirty();
            hover.SetVerticesDirty();
        }

    }
}
