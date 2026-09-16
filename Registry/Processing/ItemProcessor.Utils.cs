using Jotunn.Managers;
using OfTamingAndBreeding.Data.Files;
using OfTamingAndBreeding.OTABUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OfTamingAndBreeding.Registry.Processing
{
    internal partial class ItemProcessor : Base.DataProcessor<ItemFile>
    {
        /*
        private static void LogHierarchy(Transform transform, string indent = "")
        {
            Plugin.LogWarning($"{indent}{transform.name}");
            foreach (var component in transform.GetComponents<Component>())
            {
                Plugin.LogWarning(
                    $"{indent}  [{component.GetType().Name}]"
                );
            }
            foreach (Transform child in transform)
            {
                LogHierarchy(child, indent + "  ");
            }
        }
        */

        private bool ApplyVisualFrom(GameObject item, GameObject visualSource, string model)
        {
            if (!item || !visualSource)
            {
                return false;
            }

            //LogHierarchy(item.transform);
            //LogHierarchy(visualSource.transform);

            var visualName = $"{item.name}_CustomVisual";
            var oldVisuals = GetVisualObjects(item);

            // remember their original active state
            originalVisualStates[item.name] = oldVisuals.ToDictionary(
                visual => visual,
                visual => visual.activeSelf
            );

            var visualRoot = new GameObject(visualName);
            visualRoot.transform.SetParent(item.transform, false);

            // remember what OTAB added
            customVisuals[item.name] = visualRoot;

            foreach (Transform sourceChild in visualSource.transform)
            {
                if (ContainsForbiddenComponents(sourceChild))
                {
                    Plugin.LogServerDebug($"{model}: Skipping visual child '{sourceChild.name}' because it contains gameplay/network components");
                    continue;
                }

                //Plugin.LogMessage(sourceChild.gameObject.name);
                var clonedChild = UnityEngine.Object.Instantiate(sourceChild.gameObject, visualRoot.transform, false);
                clonedChild.name = sourceChild.name;
                clonedChild.transform.localPosition = sourceChild.localPosition;
                clonedChild.transform.localRotation = sourceChild.localRotation;
                clonedChild.transform.localScale = sourceChild.localScale;
            }

            // disable old visuals
            foreach (var oldVisual in oldVisuals)
            {
                oldVisual.SetActive(false);
            }

            // get new icon for new model
            var icon = SpriteUtils.RenderGameObject(item);
            item.GetComponent<ItemDrop>().m_itemData.m_shared.m_icons = new[] { icon };

            return true;
        }

        private static List<GameObject> GetVisualObjects(GameObject prefab)
        {
            var result = new List<GameObject>();

            foreach (Transform child in prefab.transform)
            {
                if (
                    child.GetComponentInChildren<Renderer>(true) ||
                    child.GetComponentInChildren<Light>(true) ||
                    child.GetComponentInChildren<ParticleSystem>(true)
                )
                {
                    result.Add(child.gameObject);
                }
            }

            return result;
        }

        private static bool ContainsForbiddenComponents(Transform root)
        {
            return
                root.GetComponentInChildren<ZNetView>(true) ||
                root.GetComponentInChildren<ItemDrop>(true) ||
                root.GetComponentInChildren<EggGrow>(true) ||
                root.GetComponentInChildren<EggHatch>(true) ||
                root.GetComponentInChildren<Destructible>(true) ||
                root.GetComponentInChildren<DropOnDestroyed>(true);
        }

        private void SaveIcon(Sprite sprite, string name)
        {
            if (ZNet.instance.IsServer() && Plugin.Configs.ExportIconsToCache.Value == true)
            {
                if (sprite != null)
                {
                    var pngFile = System.IO.Path.Combine(Plugin.CacheDir, "Icons", $"{name}.png");
                    var dir = System.IO.Path.GetDirectoryName(pngFile);
                    if (System.IO.Directory.Exists(dir) == false)
                    {
                        System.IO.Directory.CreateDirectory(dir);
                    }
                    if (System.IO.File.Exists(pngFile))
                    {
                        System.IO.File.Delete(pngFile);
                    }
                    SpriteUtils.ExportSpriteToPng(sprite, pngFile);
                }
            }
        }

        private static Color ShiftColor(Color color, float hueShift, float saturationShift, float brightnessShift = 0f)
        {
            Color.RGBToHSV(color, out float h, out float s, out float v);
            h = Mathf.Repeat(h + hueShift, 1f);
            s = Mathf.Clamp01(s + saturationShift);
            v = Mathf.Clamp01(v + brightnessShift);
            var result = Color.HSVToRGB(h, s, v);
            result.a = color.a;
            return result;
        }

        private static void ShiftItemColors(GameObject item, float hueShift, float saturationShift, float brightnessShift)
        {
            if (!item)
                return;

            foreach (var renderer in item.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer is ParticleSystemRenderer)
                    continue; // keep particles original

                var materials = renderer.sharedMaterials;
                if (materials == null)
                    continue;

                for (int i = 0; i < materials.Length; i++)
                {
                    if (!materials[i])
                        continue;

                    var material = UnityEngine.Object.Instantiate(materials[i]);
                    ShiftMaterialColors(material, hueShift, saturationShift, brightnessShift);
                    materials[i] = material;
                }

                renderer.sharedMaterials = materials;
            }
        }

        private static void ShiftMaterialColors(Material material, float hueShift, float saturationShift, float brightnessShift)
        {
            bool bakedMainColor = TryShiftMainTexture(material, hueShift, saturationShift, brightnessShift);
            if (!bakedMainColor)
            {
                if (material.HasProperty("_Color"))
                {
                    var color = material.GetColor("_Color");
                    material.SetColor("_Color", ShiftColor(color, hueShift, saturationShift, brightnessShift));
                }
                if (material.HasProperty("_TintColor"))
                {
                    var color = material.GetColor("_TintColor");
                    material.SetColor("_TintColor", ShiftColor(color, hueShift, saturationShift, brightnessShift));
                }
            }
            if (material.HasProperty("_EmissionColor"))
            {
                var color = material.GetColor("_EmissionColor");
                material.SetColor("_EmissionColor", ShiftColor(color, hueShift, saturationShift, brightnessShift));
            }
            if (material.HasProperty("_EmissiveColor"))
            {
                var color = material.GetColor("_EmissiveColor");
                material.SetColor("_EmissiveColor", ShiftColor(color, hueShift, saturationShift, brightnessShift));
            }
        }

        private static bool TryShiftMainTexture(Material material, float hueShift, float saturationShift, float brightnessShift)
        {
            if (!material.HasProperty("_MainTex") || !material.HasProperty("_Color"))
            {
                return false;
            }

            var source = material.GetTexture("_MainTex") as Texture2D;
            if (!source)
            {
                return false;
            }

            var materialColor = material.GetColor("_Color");

            Texture2D readable = MakeReadableCopy(source);
            if (!readable)
            {
                return false;
            }

            var pixels = readable.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                var p = pixels[i];

                // Bake shader's normal "texture * color" relationship
                //var combined = new Color(p.r * materialColor.r, p.g * materialColor.g, p.b * materialColor.b, p.a * materialColor.a);
                var combined = new Color(p.r * materialColor.r, p.g * materialColor.g, p.b * materialColor.b, p.a);
                pixels[i] = ShiftColor(combined, hueShift, saturationShift, brightnessShift);
            }

            readable.SetPixels(pixels);
            readable.Apply(false, false);

            material.SetTexture("_MainTex", readable);

            // Color has already been baked into the texture.
            //material.SetColor("_Color", new Color(1f, 1f, 1f, 1f));
            material.SetColor("_Color", new Color(1f, 1f, 1f, materialColor.a));

            return true;
        }

        private static Texture2D MakeReadableCopy(Texture2D source)
        {
            if (source.isReadable)
            {
                var copy = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
                copy.SetPixels(source.GetPixels());
                copy.Apply(false, false);
                return copy;
            }

            var rt = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var previous = RenderTexture.active;

            try
            {
                Graphics.Blit(source, rt);
                RenderTexture.active = rt;

                var copy = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
                copy.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
                copy.Apply(false, false);
                return copy;
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(rt);
            }
        }

        private static void ShiftLightColors(GameObject item, float hueShift, float saturationShift)
        {
            foreach (var light in item.GetComponentsInChildren<Light>(true))
            {
                light.color = ShiftColor(light.color, hueShift, saturationShift);
            }
            foreach (var mb in item.GetComponentsInChildren<MonoBehaviour>(true))
            {
                var type = mb.GetType();

                if (type.Name != "LightFlicker")
                    continue;

                var baseColorField = type.GetField("m_baseColor",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Public);

                if (baseColorField == null || baseColorField.FieldType != typeof(Color))
                    continue;

                var color = (Color)baseColorField.GetValue(mb);
                baseColorField.SetValue(mb, ShiftColor(color, hueShift, saturationShift));
            }
        }

        private static Sprite CreateShiftedSprite(Sprite src, float hueShift, float saturationShift, float brightnessShift)
        {
            if (!src)
                return null;

            var tex = src.texture;
            if (tex && tex.isReadable)
            {
                return CreateShiftedSpriteCpu(src, hueShift, saturationShift, brightnessShift);
            }
            return CreateShiftedSpriteGpu(src, hueShift, saturationShift, brightnessShift);
        }

        private static Sprite CreateShiftedSpriteCpu(Sprite src, float hueShift, float saturationShift, float brightnessShift)
        {
            const float alphaThreshold = 0.05f;

            var rect = src.textureRect;
            int width = Mathf.RoundToInt(rect.width);
            int height = Mathf.RoundToInt(rect.height);

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false, false);
            texture.name = $"{src.name}_shifted";

            var pixels = src.texture.GetPixels(Mathf.RoundToInt(rect.x), Mathf.RoundToInt(rect.y), width, height);
            for (int i = 0; i < pixels.Length; i++)
            {
                var pixel = pixels[i];
                if (pixel.a <= alphaThreshold)
                {
                    pixels[i] = new Color(0f, 0f, 0f, 0f);
                    continue;
                }
                pixels[i] = ShiftColor(pixel, hueShift, saturationShift, brightnessShift);
            }

            texture.SetPixels(pixels);
            texture.Apply(false, false);

            return Sprite.Create(
                texture,
                new Rect(0, 0, width, height),
                src.pivot / rect.size,
                src.pixelsPerUnit);
        }

        private static Sprite CreateShiftedSpriteGpu(Sprite src, float hueShift, float saturationShift, float brightnessShift)
        {
            const float alphaThreshold = 0.05f;

            int width = Mathf.CeilToInt(src.rect.width);
            int height = Mathf.CeilToInt(src.rect.height);

            var rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var previous = RenderTexture.active;

            const int iconLayer = 31;

            var go = new GameObject("IconShift_TMP");
            go.hideFlags = HideFlags.HideAndDontSave;
            go.layer = iconLayer;

            var spriteRenderer = go.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = src;
            spriteRenderer.color = Color.white;
            spriteRenderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"));

            var cameraObject = new GameObject("IconShift_CAM");
            cameraObject.hideFlags = HideFlags.HideAndDontSave;
            cameraObject.layer = iconLayer;

            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            camera.targetTexture = rt;
            camera.cullingMask = 1 << iconLayer;
            camera.allowMSAA = false;
            camera.allowHDR = false;

            try
            {
                go.transform.position = Vector3.zero;
                camera.orthographicSize = (height / src.pixelsPerUnit) * 0.5f;
                camera.transform.position = new Vector3(0f, 0f, -10f);
                camera.Render();

                RenderTexture.active = rt;

                var texture = new Texture2D(width, height, TextureFormat.RGBA32, false, false);
                texture.name = $"{src.name}_shifted";
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                texture.Apply(false, false);

                var pixels = texture.GetPixels();
                for (int i = 0; i < pixels.Length; i++)
                {
                    var pixel = pixels[i];
                    if (pixel.a <= alphaThreshold)
                    {
                        pixels[i] = new Color(0f, 0f, 0f, 0f);
                        continue;
                    }

                    pixels[i] = ShiftColor(pixel, hueShift, saturationShift, brightnessShift);
                }

                texture.SetPixels(pixels);
                texture.Apply(false, false);

                return Sprite.Create(
                    texture,
                    new Rect(0, 0, width, height),
                    new Vector2(0.5f, 0.5f),
                    src.pixelsPerUnit);
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(rt);
                UnityEngine.Object.DestroyImmediate(cameraObject);
                UnityEngine.Object.DestroyImmediate(go);
            }
        }






    }
}
