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
    /*
    internal static class DebugHud
    {

        private static TextMeshProUGUI debugTextMesh;

        internal static void Initialize()
        {
            if (debugTextMesh != null)
                return;

            if (Hud.instance == null)
                return;

            var canvas = Hud.instance.m_rootObject
                .GetComponentInChildren<Canvas>(true);

            if (canvas == null)
                return;

            var go = new GameObject("OTAB_HelloWorld");
            go.transform.SetParent(canvas.transform, false);

            debugTextMesh = go.AddComponent<TextMeshProUGUI>();
            debugTextMesh.text = "Hello World";
            debugTextMesh.fontSize = 18;
            debugTextMesh.color = Color.white;
            debugTextMesh.alignment = TextAlignmentOptions.TopLeft;

            var rt = debugTextMesh.rectTransform;
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(10, -10);
        }

        internal static void Update()
        {
            if (debugTextMesh == null)
                return;

            debugTextMesh.text = $"Hello World 👋\nTime: {Time.time:F2}";
        }

    }

    [HarmonyPatch(typeof(Hud), "Awake")]
    static class Hud_Awake_Patch
    {
        static void Postfix()
        {
            DebugHud.Initialize();
        }
    }

    [HarmonyPatch(typeof(Hud), "Update")]
    static class Hud_Update_Patch
    {
        static void Postfix()
        {
            DebugHud.Update();
        }
    }

    */
}
