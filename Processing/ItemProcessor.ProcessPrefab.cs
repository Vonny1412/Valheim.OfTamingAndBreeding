using OfTamingAndBreeding.Components;
using OfTamingAndBreeding.Components.Traits;
using OfTamingAndBreeding.Data.Models;
using OfTamingAndBreeding.Data.Models.SubData;
using OfTamingAndBreeding.Processing.Core;
using OfTamingAndBreeding.Processing.Registry;
using OfTamingAndBreeding.Utilities;
using System;
using System.Linq;
using UnityEngine;

namespace OfTamingAndBreeding.Processing
{
    internal partial class ItemProcessor : DataProcessor<ItemFile>
    {

        public override bool ProcessPrefab(string itemName, ItemFile data)
        {
            var model = $"{nameof(ItemFile)}.{itemName}";
            var error = false;

            var item = OTABPrefabRegistry.Instance.GetRegisteredPrefab(itemName);

            var itemItemDrop = item.GetComponent<ItemDrop>();
            var itemItemData = itemItemDrop.m_itemData;
            var itemItemDataShared = itemItemData.m_shared;

            var baseIcon = itemItemDataShared.m_icons?.FirstOrDefault();
            SaveIcon(baseIcon, path_icons_original, itemName);

            if (OTABPrefabRegistry.IsCustomPrefab(itemName))
            {
                PrepareClone(itemName, data, item);
            }
            else
            {
                SaveIcon(baseIcon, path_icons_final, itemName);
            }

            //
            // Item
            //

            if (data.Components.Item == ComponentBehavior.Patch)
            {
                if (data.Item.MaxStackSize.HasValue)
                {
                    itemItemDataShared.m_maxStackSize = data.Item.MaxStackSize.Value;
                }

                if (data.Item.MaxQuality.HasValue)
                {
                    itemItemDataShared.m_maxQuality = data.Item.MaxQuality.Value;
                }

                if (data.Item.ScaleByQuality.HasValue)
                {
                    itemItemDataShared.m_scaleByQuality = data.Item.ScaleByQuality.Value;
                }

                if (data.Item.ScaleWeightByQuality.HasValue)
                {
                    itemItemDataShared.m_scaleWeightByQuality = data.Item.ScaleWeightByQuality.Value;
                }
            }
            else if (data.Components.Item == ComponentBehavior.Remove)
            {
                // ignore, cannot be removed
            }

            //
            // Floating
            //

            if (data.Components.Floating == ComponentBehavior.Patch)
            {
                var itemFloating = OTABPrefabRegistry.Instance.GetOrAddComponent<Floating>(itemName, item);

                Plugin.LogDebug($"{model}.{nameof(data.Floating)}: Setting values");

                if (data.Floating.WaterLevelOffset.HasValue)
                {
                    itemFloating.m_waterLevelOffset = data.Floating.WaterLevelOffset.Value;
                }
            }
            else if (data.Components.Floating == ComponentBehavior.Remove)
            {
                //OTABPrefabRegistry.Instance.DestroyComponentIfExists<Floating>(itemName, item);
                // do not destroy! we can just disable it
                var itemFloating = item.GetComponent<Floating>();
                if (itemFloating)
                {
                    itemFloating.enabled = false;
                }
            }

            //
            // EggGrow
            //

            if (data.Components.EggGrow == ComponentBehavior.Patch)
            {
                var itemEggGrow = OTABPrefabRegistry.Instance.GetOrAddComponent<EggGrow>(itemName, item);
                var itemEggGrowTrait = EggGrowTrait.GetOrAddComponent(item);

                if (data.EggGrow != null)
                {
                    Plugin.LogDebug($"{model}.{nameof(data.EggGrow)}: Setting values");

                    if (data.EggGrow.GrowTime.HasValue)
                    {
                        itemEggGrow.m_growTime = data.EggGrow.GrowTime.Value;
                    }

                    if (data.EggGrow.UpdateInterval.HasValue)
                    {
                        itemEggGrow.m_updateInterval = data.EggGrow.UpdateInterval.Value;
                    }

                    if (data.EggGrow.RequireNearbyFire.HasValue)
                    {
                        itemEggGrow.m_requireNearbyFire = data.EggGrow.RequireNearbyFire.Value;
                    }
                    else
                    {
                        itemEggGrow.m_requireNearbyFire = false;
                    }

                    if (data.EggGrow.RequireUnderRoof.HasValue)
                    {
                        itemEggGrow.m_requireUnderRoof = data.EggGrow.RequireUnderRoof.Value;
                    }
                    else
                    {
                        itemEggGrow.m_requireUnderRoof = false;
                    }

                    if (data.EggGrow.RequireCoverPercentige.HasValue)
                    {
                        itemEggGrow.m_requireCoverPercentige = data.EggGrow.RequireCoverPercentige.Value;
                    }
                    else
                    {
                        itemEggGrow.m_requireCoverPercentige = 0;
                    }

                    if (data.EggGrow.RequireAnyBiome != null)
                    {
                        var biomes = data.EggGrow.RequireAnyBiome;
                        var mask = Heightmap.Biome.None;
                        foreach (var biome in biomes)
                        {
                            mask |= biome;
                        }
                        itemEggGrowTrait.m_requireBiome = mask;
                        itemEggGrowTrait.m_requireBiomes = biomes;
                    }

                    if (data.EggGrow.RequireLiquid.HasValue)
                    {
                        itemEggGrowTrait.m_requireLiquid = data.EggGrow.RequireLiquid.Value;
                    }

                    if (data.EggGrow.RequireGlobalKey != null)
                    {
                        itemEggGrowTrait.m_requireGlobalKey = EnvironmentUtils.ParseGlobalKey(data.EggGrow.RequireGlobalKey);
                    }

                    if (data.EggGrow.Grown != null)
                    {
                        var grownList = data.EggGrow.Grown.Select((g) => new EggGrowTrait.EggGrown(
                            weight: g.Weight,
                            requireGlobalKey: EnvironmentUtils.ParseGlobalKey(g.RequireGlobalKey),
                            prefab: g.Prefab,
                            tamed: g.Tamed,
                            showHatchEffect: g.ShowHatchEffect
                        )).ToArray();
                        itemEggGrowTrait.m_grownListStoreIndex = EggGrowTrait.s_grownListStore.Add(grownList);
                    }

                }

                Plugin.LogDebug($"{model}: Setting effects");
                if (itemEggGrow.m_hatchEffect == null || itemEggGrow.m_hatchEffect.m_effectPrefabs.Length == 0)
                {
                    itemEggGrow.m_hatchEffect = new EffectList
                    {
                        m_effectPrefabs = Utilities.EffectUtils.CreateEffectList(new string[] {
                        "fx_chicken_birth",
                    })
                    };
                }

                // will be set seperatly
                itemEggGrow.m_grownPrefab = null;
                itemEggGrow.m_tamed = true;

            }
            else if (data.Components.EggGrow == ComponentBehavior.Remove)
            {
                OTABPrefabRegistry.Instance.DestroyComponentIfExists<EggGrow>(itemName, item);
                // seems we need to destroy. eggrow.start is using invokerepeating
            }

            // set last remaining egg values

            if (item.GetComponent<EggGrow>())
            {
                itemItemDrop.m_autoPickup = false;
                itemItemDrop.m_autoDestroy = false;
                itemItemDataShared.m_autoStack = false;
                eggSharedNameHashes.Add(itemItemDataShared.m_name.GetStableHashCode());
            }

            //eggItemDataShared.m_value = 0; // todo: maybe add yaml option for that

            return error == false;
        }

        //------------------------------------------------

        private void PrepareClone(string itemName, ItemFile data, UnityEngine.GameObject item)
        {
            var model = $"{nameof(ItemFile)}.{itemName}";

            Plugin.LogDebug($"{model}.{nameof(data.Clone)}: Setting ItemDrop.ItemData values");

            var itemItemDrop = item.GetComponent<ItemDrop>();
            var itemItemData = itemItemDrop.m_itemData;
            var itemItemDataShared = itemItemData.m_shared;

            if (data.Clone.Name != null)
            {
                itemItemDataShared.m_name = data.Clone.Name;
            }

            if (data.Clone.Description != null)
            {
                var descr = data.Clone.Description;
                itemItemDataShared.m_description = descr;
            }

            if (data.Clone.ItemType != null && Enum.TryParse<ItemDrop.ItemData.ItemType>(data.Clone.ItemType, ignoreCase: true, out var result))
            {
                itemItemDataShared.m_itemType = result;
            }

            if (data.Clone.Scale.HasValue)
            {
                var customScale = data.Clone.Scale.Value;
                if (customScale != 1 && customScale > 0)
                {
                    var component = OTABPrefabRegistry.Instance.GetOrAddComponent<ScaledItem>(itemName, item);
                    component.m_scale = customScale;
                }

            }

            if (data.Clone.Weight.HasValue)
            {
                itemItemDataShared.m_weight = data.Clone.Weight.Value;
            }

            if (data.Clone.Teleportable.HasValue)
            {
                itemItemDataShared.m_teleportable = data.Clone.Teleportable.Value;
            }

            // custom icon

            UnityEngine.Sprite customIcon = null;
            var baseIcon = itemItemDataShared.m_icons?.FirstOrDefault();
            if (data.Clone.CustomIcon != null)
            {
                Plugin.LogDebug($"{model}.{nameof(data.Clone)}: Setting custom icon");
                TextureProcessor.TryGetSprite(data.Clone.CustomIcon, out customIcon);
            }

            // color/lights stuff

            if (data.Clone.ItemHueShift.HasValue || data.Clone.ItemSaturationShift.HasValue || data.Clone.ItemBrightnessShift.HasValue)
            {
                Plugin.LogDebug($"{model}.{nameof(data.Clone)}: Shifting item colors");
                float hueShift = data.Clone.ItemHueShift ?? 0f;
                float saturationShift = data.Clone.ItemSaturationShift ?? 0f;
                float brightnessShift = data.Clone.ItemBrightnessShift ?? 0f;
                ShiftItemColors(item, hueShift, saturationShift, brightnessShift);

                if (customIcon == null && baseIcon != null)
                {
                    Plugin.LogDebug($"{model}.{nameof(data.Clone)}: Shifting icon colors");
                    customIcon = CreateShiftedSprite(baseIcon, hueShift, saturationShift, brightnessShift);
                    if (customIcon)
                    {
                        customIcons.Add(customIcon);
                    }
                }
            }

            if (data.Clone.LightsHueShift.HasValue || data.Clone.LightsSaturationShift.HasValue)
            {
                Plugin.LogDebug($"{model}.{nameof(data.Clone)}: Shifting light colors");
                float hueShift = data.Clone.LightsHueShift ?? 0f;
                float saturationShift = data.Clone.LightsSaturationShift ?? 0f;
                ShiftLightColors(item, hueShift, saturationShift);
            }

            if (data.Clone.LightsScale.HasValue)
            {
                var lightsScale = data.Clone.LightsScale.Value;
                if (lightsScale != 1.0f)
                {
                    foreach (var l in item.GetComponentsInChildren<UnityEngine.Light>(true))
                    {
                        if (lightsScale <= 0f)
                        {
                            l.enabled = false;
                            l.range = 0;
                            continue;
                        }

                        //l.enabled = true;
                        l.range *= lightsScale;
                    }
                }
            }
            if (customIcon != null)
            {
                SaveIcon(customIcon, path_icons_final, itemName);
                itemItemDataShared.m_icons = new[] { customIcon };
            }
            else
            {
                SaveIcon(baseIcon, path_icons_final, itemName);
            }

            if (data.Clone.AttachedSprite != null)
            {
                var texName = data.Clone.AttachedSprite;
                TextureProcessor.TryGetSprite(data.Clone.AttachedSprite, out var sprite);

                var component = AttachedSprite.GetOrAddComponent(item);
                component.m_sprite = sprite;
                component.m_size = data.Clone.AttachedSpriteScale ?? 1f;
                if (data.Clone.AttachedSpriteOffset != null)
                {
                    component.m_offset = new Vector3(
                        data.Clone.AttachedSpriteOffset.X ?? 0f,
                        data.Clone.AttachedSpriteOffset.Y ?? 0.02f,
                        data.Clone.AttachedSpriteOffset.Z ?? 0f);
                }
                else
                {
                    component.m_offset = new Vector3(0f, 0.02f, 0f);
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

        private void ShiftItemColors(GameObject item, float hueShift, float saturationShift, float brightnessShift)
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

        private void ShiftMaterialColors(Material material, float hueShift, float saturationShift, float brightnessShift)
        {
            bool bakedMainColor = TryShiftMainTexture(material, hueShift, saturationShift, brightnessShift, out Texture2D customTexture);
            if (customTexture)
            {
                customTextures.Add(customTexture);
            }
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

        private static bool TryShiftMainTexture(Material material, float hueShift, float saturationShift, float brightnessShift, out Texture2D createdTexture)
        {
            createdTexture = null;

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

                // bake shaders normal "texture * color" relationship
                //var combined = new Color(p.r * materialColor.r, p.g * materialColor.g, p.b * materialColor.b, p.a * materialColor.a);
                var combined = new Color(p.r * materialColor.r, p.g * materialColor.g, p.b * materialColor.b, p.a);
                pixels[i] = ShiftColor(combined, hueShift, saturationShift, brightnessShift);
            }

            readable.SetPixels(pixels);
            readable.Apply(false, false);

            material.SetTexture("_MainTex", readable);

            // Color has already been baked into the texture
            //material.SetColor("_Color", new Color(1f, 1f, 1f, 1f));
            material.SetColor("_Color", new Color(1f, 1f, 1f, materialColor.a));

            createdTexture = readable;
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

                return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), src.pixelsPerUnit);
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(rt);
                UnityEngine.Object.DestroyImmediate(cameraObject);
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        //------------------------------------------------

        private void SaveIcon(Sprite sprite, string dir, string name)
        {
            if (!sprite || !ZNet.instance.IsServer() || Plugin.Configs.ExportIconsToCache.Value == false)
            {
                return;
            }

            var outpath = System.IO.Path.Combine(Plugin.CacheDir, "_icons", dir, $"{name}.png");
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(outpath));
            SpriteUtils.ExportSpriteToPng(sprite, outpath);
        }


        // todo: put constants in constants file
        // also for prefab dumps

        // todo: move all methods for dumping/extracting of prefabs/icons into one class file 

    }
}