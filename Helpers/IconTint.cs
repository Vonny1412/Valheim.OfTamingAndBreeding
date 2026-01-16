using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Helpers
{
    using UnityEngine;

    internal static class IconTint
    {
        public static Sprite CreateTintedSprite(Sprite src, Color tint)
        {
            if (!src) return null;

            // Try CPU path (fastest) if readable
            var tex = src.texture;
            if (tex && tex.isReadable)
                return CreateTintedSpriteCpu(src, tint);

            // Fallback: GPU copy + readback (works even if not readable)
            return CreateTintedSpriteGpu(src, tint);
        }

        private static Sprite CreateTintedSpriteCpu(Sprite src, Color tint)
        {
            var r = src.textureRect;
            int w = Mathf.RoundToInt(r.width);
            int h = Mathf.RoundToInt(r.height);

            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false, false);
            tex.name = $"{src.name}_tinted";

            // Copy only the sprite rect from the atlas
            var pixels = src.texture.GetPixels(
                Mathf.RoundToInt(r.x),
                Mathf.RoundToInt(r.y),
                w, h);

            // Multiply tint (keep alpha)
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = pixels[i] * tint;

            tex.SetPixels(pixels);
            tex.Apply(false, true);

            return Sprite.Create(tex, new Rect(0, 0, w, h), src.pivot / r.size, src.pixelsPerUnit);
        }

        private static Sprite CreateTintedSpriteGpu(Sprite src, Color tint)
        {
            var r = src.textureRect;
            int w = Mathf.RoundToInt(r.width);
            int h = Mathf.RoundToInt(r.height);

            // Render only the sprite's rect into a temporary RT
            var rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var prev = RenderTexture.active;

            try
            {
                // Draw the sprite's rect into RT (no need for readable texture)
                // We'll use Graphics.Blit to copy, then tint in CPU.
                // Step 1: copy sprite rect into RT
                // Create a small temp texture with the rect via ReadPixels after setting active
                // We can do a rect blit by using a material, but simplest is to use a custom shader.
                // To keep this dependency-free: do a full blit and then read only the rect.
                Graphics.Blit(src.texture, rt);

                RenderTexture.active = rt;

                // Read only the sprite rect from the blitted atlas image
                var tex = new Texture2D(w, h, TextureFormat.RGBA32, false, false);
                tex.name = $"{src.name}_tinted";

                // Note: RT origin is bottom-left, textureRect origin is top-left in atlas space.
                // Because we blitted full atlas to RT at its original size, we can't map rect 1:1.
                // -> So for GPU fallback without atlas-size RT, we need a rect-copy approach.
                //
                // EASIEST practical fallback: render the sprite itself with SpriteRenderer to an RT.
                //
                // We'll do that here instead, since mapping atlas coords is messy.

                Object.Destroy(tex);
            }
            finally
            {
                RenderTexture.active = prev;
                RenderTexture.ReleaseTemporary(rt);
            }

            // Practical GPU fallback: render the sprite itself
            return CreateTintedSpriteGpu_RenderSprite(src, tint);
        }

        private static Sprite CreateTintedSpriteGpu_RenderSprite(Sprite src, Color tint)
        {
            int w = Mathf.CeilToInt(src.rect.width);
            int h = Mathf.CeilToInt(src.rect.height);

            var rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var prev = RenderTexture.active;

            // Create a tiny temporary GO with SpriteRenderer
            var go = new GameObject("IconTint_TMP");
            go.hideFlags = HideFlags.HideAndDontSave;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = src;
            sr.color = tint;

            var camGo = new GameObject("IconTint_CAM");
            camGo.hideFlags = HideFlags.HideAndDontSave;
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0, 0, 0, 0);
            cam.targetTexture = rt;

            try
            {
                // Frame the sprite
                cam.orthographicSize = (h / src.pixelsPerUnit) * 0.5f;
                go.transform.position = Vector3.zero;
                cam.transform.position = new Vector3(0, 0, -10);

                cam.Render();

                RenderTexture.active = rt;

                var tex = new Texture2D(w, h, TextureFormat.RGBA32, false, false);
                tex.name = $"{src.name}_tinted";
                tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
                tex.Apply(false, true);

                return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), src.pixelsPerUnit);
            }
            finally
            {
                RenderTexture.active = prev;
                RenderTexture.ReleaseTemporary(rt);
                Object.DestroyImmediate(camGo);
                Object.DestroyImmediate(go);
            }
        }
    }

}
