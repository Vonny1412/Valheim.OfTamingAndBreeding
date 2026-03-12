using System;
using UnityEngine;

namespace OfTamingAndBreeding.OTABUtils
{
    internal static class SpriteUtils
    {

        public static Sprite TextureToSprite(Texture2D tex)
        {
            if (!tex) return null;
            return Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                100f
            );
        }

        public static bool TryLoadValidImage(string base64, out Texture2D texture)
        {
            byte[] bytes = Convert.FromBase64String(base64);
            return TryLoadValidImage(bytes, out texture);
        }

        public static bool TryLoadValidImage(byte[] bytes, out Texture2D texture)
        {
            texture = null;

            if (bytes == null || bytes.Length == 0)
                return false;

            try
            {
                var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false)
                {
                    filterMode = FilterMode.Bilinear
                };
                bool loaded = tex.LoadImage(bytes, markNonReadable: true);
                if (!loaded)
                {
                    UnityEngine.Object.Destroy(tex);
                    return false;
                }

                if (tex.width <= 0 || tex.height <= 0)
                {
                    UnityEngine.Object.Destroy(tex);
                    return false;
                }

                texture = tex;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static Texture2D RenderSpriteToTexture(Sprite src)
        {
            if (!src) return null;

            int w = Mathf.CeilToInt(src.rect.width);
            int h = Mathf.CeilToInt(src.rect.height);

            var rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var prev = RenderTexture.active;

            const int IconLayer = 31;

            var go = new GameObject("IconExport_TMP");
            go.hideFlags = HideFlags.HideAndDontSave;
            go.layer = IconLayer;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = src;
            sr.color = Color.white;
            sr.sharedMaterial = new Material(Shader.Find("Sprites/Default"));

            var camGo = new GameObject("IconExport_CAM");
            camGo.hideFlags = HideFlags.HideAndDontSave;
            camGo.layer = IconLayer;

            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0, 0, 0, 0);
            cam.targetTexture = rt;
            cam.cullingMask = 1 << IconLayer;
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
                tex.name = $"{src.name}_export";
                tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
                tex.Apply(false, false);

                return tex;
            }
            finally
            {
                RenderTexture.active = prev;
                RenderTexture.ReleaseTemporary(rt);
                UnityEngine.Object.DestroyImmediate(camGo);
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        public static void ExportSpriteToPng(Sprite src, string path)
        {
            var tex = RenderSpriteToTexture(src);
            if (tex == null) return;

            var png = tex.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, png);
            UnityEngine.Object.Destroy(tex);
        }

        public static Texture2D LoadImageFromFile(string path, bool readable = true)
        {
            if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
                return null;

            byte[] data = System.IO.File.ReadAllBytes(path);
            return LoadImageFromBytes(data, readable);
        }

        public static Texture2D LoadImageFromBytes(byte[] data, bool readable = true)
        {
            if (data == null || data.Length == 0)
                return null;

            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false, false);

            // requires UnityEngine.ImageConversionModule
            if (!UnityEngine.ImageConversion.LoadImage(tex, data, !readable))
            {
                UnityEngine.Object.Destroy(tex);
                return null;
            }

            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            return tex;
        }
    }
}
