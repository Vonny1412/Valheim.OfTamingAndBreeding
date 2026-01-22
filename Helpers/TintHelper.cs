using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OfTamingAndBreeding.Helpers
{
    internal static class TintHelper
    {

        public static bool TryParseTint(string src, out Color color)
        {
            color = default;

            if (string.IsNullOrWhiteSpace(src))
                return false;

            src = src.Trim();

            if (src[0] == '#')
            {
                string hex = src.Substring(1);

                if (hex.Length != 6 && hex.Length != 8)
                    return false;

                if (!TryParseHexByte(hex, 0, out byte r) ||
                    !TryParseHexByte(hex, 2, out byte g) ||
                    !TryParseHexByte(hex, 4, out byte b))
                    return false;

                byte a = 255;
                if (hex.Length == 8)
                {
                    if (!TryParseHexByte(hex, 6, out a))
                        return false;
                }

                color = new Color(
                    r / 255f,
                    g / 255f,
                    b / 255f,
                    a / 255f
                );
                return true;
            }

            string[] parts = src.Split(',');
            if (parts.Length != 3 && parts.Length != 4)
                return false;

            if (!TryParseByte(parts[0], out byte cr) ||
                !TryParseByte(parts[1], out byte cg) ||
                !TryParseByte(parts[2], out byte cb))
                return false;

            byte ca = 255;
            if (parts.Length == 4)
            {
                if (!TryParseByte(parts[3], out ca))
                    return false;
            }

            color = new Color(
                cr / 255f,
                cg / 255f,
                cb / 255f,
                ca / 255f
            );
            return true;
        }

        private static bool TryParseHexByte(string s, int index, out byte value)
        {
            value = 0;
            if (index + 2 > s.Length)
                return false;

            return byte.TryParse(
                s.Substring(index, 2),
                System.Globalization.NumberStyles.HexNumber,
                System.Globalization.CultureInfo.InvariantCulture,
                out value
            );
        }

        private static bool TryParseByte(string s, out byte value)
        {
            return byte.TryParse(s.Trim(), out value);
        }


        private static Color Multiply(Color baseCol, Color tint)
        {
            // komponentenweise Multiplikation, Alpha typischerweise beibehalten
            return new Color(baseCol.r * tint.r, baseCol.g * tint.g, baseCol.b * tint.b, baseCol.a);
        }

        private static Gradient Multiply(Gradient grad, Color tint)
        {
            var outG = new Gradient();
            var cks = grad.colorKeys;
            for (int i = 0; i < cks.Length; ++i)
                cks[i].color = Multiply(cks[i].color, tint);

            outG.SetKeys(cks, grad.alphaKeys); // AlphaKeys unverändert
            return outG;
        }

        public static void TintPrefab(GameObject prefab, Color tint, bool multiply)
        {
            if (!prefab) return;

            foreach (var r in prefab.GetComponentsInChildren<Renderer>(true))
            {
                var mats = r.sharedMaterials;
                if (mats == null) continue;

                for (int i = 0; i < mats.Length; i++)
                {
                    if (!mats[i]) continue;
                    var m2 = UnityEngine.Object.Instantiate(mats[i]);
                    ApplyTintToMaterial(m2, tint, multiply);
                    mats[i] = m2;
                }

                r.sharedMaterials = mats;
            }
        }

        private static ParticleSystem.MinMaxGradient Multiply(ParticleSystem.MinMaxGradient g, Color tint)
        {
            // Es gibt 4 Modi: Color, TwoColors, Gradient, TwoGradients
            switch (g.mode)
            {
                case ParticleSystemGradientMode.Color:
                    g.color = Multiply(g.color, tint);
                    break;

                case ParticleSystemGradientMode.TwoColors:
                    g.colorMin = Multiply(g.colorMin, tint);
                    g.colorMax = Multiply(g.colorMax, tint);
                    break;

                case ParticleSystemGradientMode.Gradient:
                    g.gradient = Multiply(g.gradient, tint);
                    break;

                case ParticleSystemGradientMode.TwoGradients:
                    g.gradientMin = Multiply(g.gradientMin, tint);
                    g.gradientMax = Multiply(g.gradientMax, tint);
                    break;
            }
            return g;
        }

        public static void TintParticleSystems(GameObject go, Color tint, bool multiply)
        {
            foreach (var ps in go.GetComponentsInChildren<ParticleSystem>(true))
            {
                var main = ps.main;
                if (multiply)
                    main.startColor = Multiply(main.startColor, tint);
                else
                    main.startColor = new ParticleSystem.MinMaxGradient(tint);
                var col = ps.colorOverLifetime;
                if (col.enabled)
                {
                    if (multiply) col.color = Multiply(col.color, tint);
                    else col.color = new ParticleSystem.MinMaxGradient(tint);
                }
                var bySpeed = ps.colorBySpeed;
                if (bySpeed.enabled)
                {
                    if (multiply) bySpeed.color = Multiply(bySpeed.color, tint);
                    else bySpeed.color = new ParticleSystem.MinMaxGradient(tint);
                }
            }
            // optional: Material-tinting on ParticleSystemRenderern
            foreach (var r in go.GetComponentsInChildren<ParticleSystemRenderer>(true))
            {
                var mat = r.sharedMaterial;
                if (!mat) continue;

                ApplyTintToMaterial(mat, tint, multiply);
            }
        }

        public static void TintLights(GameObject go, Color tint, bool multiply)
        {
            foreach (var l in go.GetComponentsInChildren<Light>(true))
            {
                l.color = multiply ? Multiply(l.color, tint) : tint;
                // keep intensity
            }
            foreach (var mb in go.GetComponentsInChildren<MonoBehaviour>(true))
            {
                var t = mb.GetType();
                if (t.Name != "LightFlicker") continue;

                var baseColorField = t.GetField("m_baseColor",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);

                if (baseColorField != null && baseColorField.FieldType == typeof(Color))
                {
                    var baseCol = (Color)baseColorField.GetValue(mb);
                    baseColorField.SetValue(mb, multiply ? Multiply(baseCol, tint) : tint);
                }
            }
        }

        private static void ApplyTintToMaterial(Material mat, Color tint, bool multiply)
        {
            if (mat.HasProperty("_Color"))
            {
                var baseCol = mat.GetColor("_Color");
                mat.SetColor("_Color", multiply ? Multiply(baseCol, tint) : tint);
            }
            if (mat.HasProperty("_TintColor"))
            {
                var baseCol = mat.GetColor("_TintColor");
                mat.SetColor("_TintColor", multiply ? Multiply(baseCol, tint) : tint);
            }
            // if (mat.HasProperty("_EmissionColor")) ...
        }

        //-------------------------
        // icon stuff
        //-------------------------

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

        /*
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
        */



    }
}
