using Jotunn.Managers;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace OfTamingAndBreeding.Utilities
{
    internal static class SpriteUtils
    {
        private const float DefaultPixelsPerUnit = 100f;

        public static Sprite RenderGameObject(GameObject item)
        {
            var request = new RenderManager.RenderRequest(item)
            {
                Rotation = RenderManager.IsometricRotation,
                UseCache = true
            };
            return RenderManager.Instance.Render(request);
        }

        public static Sprite TextureToSprite(Texture2D texture, string name = null, float pixelsPerUnit = DefaultPixelsPerUnit)
        {
            if (!texture)
            {
                return null;
            }
            if (!string.IsNullOrEmpty(name))
            {
                texture.name = name;
            }
            var sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            sprite.name = texture.name;
            return sprite;
        }

        public static bool TryLoadValidImage(string base64, out Texture2D texture)
        {
            texture = null;
            if (string.IsNullOrWhiteSpace(base64))
            {
                return false;
            }
            try
            {
                return TryLoadValidImage(Convert.FromBase64String(base64), out texture);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static bool TryLoadValidImage(byte[] bytes, out Texture2D texture)
        {
            texture = LoadTextureFromBytes(bytes, readable: false, filterMode: FilterMode.Point);
            return texture;
        }

        public static Texture2D LoadTextureFromFile(string path, bool readable = true, FilterMode filterMode = FilterMode.Bilinear)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return null;
            }
            return LoadTextureFromBytes(File.ReadAllBytes(path), readable, filterMode);
        }

        public static Texture2D LoadTextureFromResource(string path, Assembly assembly = null, bool readable = true, FilterMode filterMode = FilterMode.Bilinear)
        {
            assembly ??= Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(path);
            if (stream == null)
            {
                return null;
            }
            var data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);
            return LoadTextureFromBytes(data, readable, filterMode);
        }

        public static Texture2D LoadTextureFromBytes(byte[] data, bool readable = true, FilterMode filterMode = FilterMode.Bilinear)
        {
            if (data == null || data.Length == 0)
            {
                return null;
            }
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, false);
            try
            {
                if (!ImageConversion.LoadImage(texture, data, !readable))
                {
                    UnityEngine.Object.Destroy(texture);
                    return null;
                }

                if (texture.width <= 0 || texture.height <= 0)
                {
                    UnityEngine.Object.Destroy(texture);
                    return null;
                }
                texture.wrapMode = TextureWrapMode.Clamp;
                texture.filterMode = filterMode;
                return texture;
            }
            catch
            {
                UnityEngine.Object.Destroy(texture);
                return null;
            }
        }

        public static Sprite LoadSpriteFromFile(string path, string name = null, float pixelsPerUnit = DefaultPixelsPerUnit)
        {
            var texture = LoadTextureFromFile(path);
            return TextureToSprite(texture, name, pixelsPerUnit);
        }

        public static Sprite LoadSpriteFromResource(string path, string name = null, Assembly assembly = null, float pixelsPerUnit = DefaultPixelsPerUnit)
        {
            var texture = LoadTextureFromResource(path, assembly);
            return TextureToSprite(texture, name, pixelsPerUnit);
        }

        public static void ExportSpriteToPng(Sprite sprite, string path)
        {
            var texture = RenderSpriteToTexture(sprite);
            if (!texture)
            {
                return;
            }
            try
            {
                File.WriteAllBytes(path, texture.EncodeToPNG());
            }
            finally
            {
                UnityEngine.Object.Destroy(texture);
            }
        }

        private static Texture2D RenderSpriteToTexture(Sprite sprite)
        {
            if (!sprite)
            {
                return null;
            }
            int width = Mathf.CeilToInt(sprite.rect.width);
            int height = Mathf.CeilToInt(sprite.rect.height);
            var renderTexture = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var previousRenderTexture = RenderTexture.active;
            const int IconLayer = 31;
            var spriteObject = new GameObject("IconExport_TMP")
            {
                hideFlags = HideFlags.HideAndDontSave,
                layer = IconLayer
            };
            var spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = Color.white;
            spriteRenderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            var cameraObject = new GameObject("IconExport_CAM")
            {
                hideFlags = HideFlags.HideAndDontSave,
                layer = IconLayer
            };
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            camera.targetTexture = renderTexture;
            camera.cullingMask = 1 << IconLayer;
            camera.allowMSAA = false;
            camera.allowHDR = false;
            try
            {
                camera.orthographicSize = (height / sprite.pixelsPerUnit) * 0.5f;
                spriteObject.transform.position = Vector3.zero;
                camera.transform.position = new Vector3(0f, 0f, -10f);
                camera.Render();
                RenderTexture.active = renderTexture;
                var texture = new Texture2D(width, height, TextureFormat.RGBA32, false, false);
                texture.name = $"{sprite.name}_export";
                texture.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
                texture.Apply(false, false);
                return texture;
            }
            finally
            {
                RenderTexture.active = previousRenderTexture;
                RenderTexture.ReleaseTemporary(renderTexture);
                UnityEngine.Object.DestroyImmediate(cameraObject);
                UnityEngine.Object.DestroyImmediate(spriteObject);
            }
        }
    }
}