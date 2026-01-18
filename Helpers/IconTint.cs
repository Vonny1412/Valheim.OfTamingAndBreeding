using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OfTamingAndBreeding.Utils
{
    internal static class IconTint
    {
        private const float AlphaThreshold = 0.05f;

        public static Sprite CreateTintedSprite(Sprite src, Color tint)
        {
            if (!src) return null;
            tint.a = 1f;

            var tex = src.texture;
            if (tex && tex.isReadable)
                return CreateTintedSpriteCpu(src, tint);

            return CreateTintedSpriteGpu_RenderSprite(src, tint);
        }

        private static Sprite CreateTintedSpriteCpu(Sprite src, Color tint)
        {
            var r = src.textureRect;
            int w = Mathf.RoundToInt(r.width);
            int h = Mathf.RoundToInt(r.height);

            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false, false);
            tex.name = $"{src.name}_tinted";

            var pixels = src.texture.GetPixels(
                Mathf.RoundToInt(r.x),
                Mathf.RoundToInt(r.y),
                w, h);

            for (int i = 0; i < pixels.Length; i++)
            {
                var p = pixels[i];

                if (p.a <= AlphaThreshold)
                {
                    pixels[i] = new Color(0f, 0f, 0f, 0f);
                    continue;
                }

                pixels[i] = new Color(
                    p.r * tint.r,
                    p.g * tint.g,
                    p.b * tint.b,
                    p.a
                );
            }

            tex.SetPixels(pixels);
            tex.Apply(false, false); // <-- NICHT unreadable machen

            return Sprite.Create(tex, new Rect(0, 0, w, h), src.pivot / r.size, src.pixelsPerUnit);
        }

        private static Sprite CreateTintedSpriteGpu_RenderSprite(Sprite src, Color tint)
        {
            int w = Mathf.CeilToInt(src.rect.width);
            int h = Mathf.CeilToInt(src.rect.height);

            var rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var prev = RenderTexture.active;

            // pick a layer that is almost never used by Valheim/mods
            const int IconLayer = 31;

            var go = new GameObject("IconTint_TMP");
            go.hideFlags = HideFlags.HideAndDontSave;
            go.layer = IconLayer;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = src;
            sr.color = tint;

            // Make sure the renderer uses a normal sprite shader/material
            // (optional, but helps avoid weird materials)
            sr.sharedMaterial = new Material(Shader.Find("Sprites/Default"));

            var camGo = new GameObject("IconTint_CAM");
            camGo.hideFlags = HideFlags.HideAndDontSave;
            camGo.layer = IconLayer;

            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0, 0, 0, 0);
            cam.targetTexture = rt;

            // CRITICAL: render ONLY our layer
            cam.cullingMask = 1 << IconLayer;

            // reduce surprises
            cam.allowMSAA = false;
            cam.allowHDR = false;

            try
            {
                cam.orthographicSize = (h / src.pixelsPerUnit) * 0.5f;

                go.transform.position = Vector3.zero;
                cam.transform.position = new Vector3(0, 0, -10);

                cam.Render();

                RenderTexture.active = rt;

                var tex = new Texture2D(w, h, TextureFormat.RGBA32, false, false);
                tex.name = $"{src.name}_tinted";

                tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
                tex.Apply(false, false); // keep readable (icons are small)

                return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), src.pixelsPerUnit);
            }
            finally
            {
                RenderTexture.active = prev;
                RenderTexture.ReleaseTemporary(rt);
                UnityEngine.Object.DestroyImmediate(camGo);
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        private static void CleanupLowAlphaPixels(Texture2D tex)
        {
            var pixels = tex.GetPixels();
            bool changed = false;

            for (int i = 0; i < pixels.Length; i++)
            {
                var p = pixels[i];

                if (p.a <= AlphaThreshold)
                {
                    if (p.a != 0f || p.r != 0f || p.g != 0f || p.b != 0f)
                    {
                        pixels[i] = new Color(0f, 0f, 0f, 0f);
                        changed = true;
                    }
                }
            }

            if (changed)
                tex.SetPixels(pixels);
        }






    }

}
