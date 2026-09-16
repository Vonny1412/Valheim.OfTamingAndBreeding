using Jotunn.Managers;
using OfTamingAndBreeding.Components;
using OfTamingAndBreeding.Components.Traits;
using OfTamingAndBreeding.Data.Files;
using OfTamingAndBreeding.Data.Files.SubData;
using OfTamingAndBreeding.OTABUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OfTamingAndBreeding.Registry.Processing
{
    internal partial class ItemProcessor : Base.DataProcessor<ItemFile>
    {

        public override string DirectoryName => ItemFile.DirectoryName;

        public override string PrefabTypeName => "item";

        public override string GetDataKey(string filePath) => null;

        public override bool LoadFromFile(string filePath) => LoadFromYamlFile(filePath);

        //------------------------------------------------

        private readonly Dictionary<string, Sprite[]> originalIcons = new Dictionary<string, Sprite[]>();

        private readonly Dictionary<string, GameObject> customVisuals = new Dictionary<string, GameObject>();

        private readonly Dictionary<string, Dictionary<GameObject, bool>> originalVisualStates = new Dictionary<string, Dictionary<GameObject, bool>>();

        //------------------------------------------------
        // PREPARE
        //------------------------------------------------

        public override void PrepareProcess()
        {

        }

        //------------------------------------------------
        // VALIDATE DATA
        //------------------------------------------------

        public override bool ValidateData(string itemName, ItemFile data)
        {
            var model = $"{nameof(ItemFile)}.{itemName}";
            var error = false;

            if (data.Clone != null)
            {
                if (data.Clone.Name == null)
                {
                    Plugin.LogError($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.Name)}: Missing field");
                    error = true;
                }
                if (data.Clone.Description == null)
                {
                    Plugin.LogWarning($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.Description)}: Missing field - Setting to ''");
                    data.Clone.Description = "";
                }
                if (data.Clone.Scale.HasValue)
                {
                    if (data.Clone.Scale.Value <= 0)
                    {
                        Plugin.LogWarning($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.Scale)}: Negative/zero value not allow - Setting to null");
                        data.Clone.Scale = null;
                    }
                }
            }

            switch (data.Components.Item)
            {
                case ComponentBehavior.Remove:
                    Plugin.LogWarning($"{model}.{nameof(data.Components)}.{nameof(data.Components.Item)}({nameof(ComponentBehavior.Remove)}): Component cannot be removed");
                    break;
                case ComponentBehavior.Patch:
                    if (data.Item == null)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Components)}.{nameof(data.Components.Item)}({nameof(ComponentBehavior.Patch)}): Missing component data");
                        error = true;
                    }
                    break;
                case ComponentBehavior.Inherit:
                    if (data.Item != null)
                    {
                        Plugin.LogWarning($"{model}.{nameof(data.Components)}.{nameof(data.Components.Item)}({nameof(ComponentBehavior.Inherit)}): Component data will be ignored");
                    }
                    break;
            }

            switch (data.Components.Floating)
            {
                case ComponentBehavior.Remove:
                    // can be removed
                    break;
                case ComponentBehavior.Patch:
                    if (data.Floating == null)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Components)}.{nameof(data.Components.Floating)}({nameof(ComponentBehavior.Patch)}): Missing component data");
                        error = true;
                    }
                    break;
                case ComponentBehavior.Inherit:
                    if (data.Floating != null)
                    {
                        Plugin.LogWarning($"{model}.{nameof(data.Components)}.{nameof(data.Components.Floating)}({nameof(ComponentBehavior.Inherit)}): Component data will be ignored");
                    }
                    break;
            }

            switch (data.Components.EggGrow)
            {
                case ComponentBehavior.Remove:
                    // can be removed
                    break;
                case ComponentBehavior.Patch:
                    if (data.EggGrow == null)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Components)}.{nameof(data.Components.EggGrow)}({nameof(ComponentBehavior.Patch)}): Missing component data");
                        error = true;
                    }
                    break;
                case ComponentBehavior.Inherit:
                    if (data.EggGrow != null)
                    {
                        Plugin.LogWarning($"{model}.{nameof(data.Components)}.{nameof(data.Components.EggGrow)}({nameof(ComponentBehavior.Inherit)}): Component data will be ignored");
                    }
                    break;
            }

            if (data.EggGrow != null && data.Components.EggGrow == ComponentBehavior.Patch)
            {
                if (data.EggGrow.Grown == null || data.EggGrow.Grown.Length == 0)
                {
                    Plugin.LogError($"{model}.{nameof(data.EggGrow)}.{nameof(data.EggGrow.Grown)}: List is null or empty");
                    error = true;
                }
                else
                {
                    foreach (var (grownData, i) in data.EggGrow.Grown.Select((value, i) => (value, i)))
                    {
                        grownData.Weight = Math.Max(0f, grownData.Weight);
                        if (grownData.Prefab == null)
                        {
                            Plugin.LogError($"{model}.{nameof(data.EggGrow)}.{nameof(data.EggGrow.Grown)}.{i}.{nameof(grownData.Prefab)}: Field is empty");
                            error = true;
                        }
                    }
                }
            }

            return error == false;
        }

        //------------------------------------------------
        // RESERVE PREFAB
        //------------------------------------------------

        public override bool ReservePrefab(string itemName, ItemFile data)
        {
            var model = $"{nameof(ItemFile)}.{itemName}";

            var item = OTABRegistry.Instance.GetReservedPrefab(itemName);
            if (item == null)
            {

                var custom = OTABRegistry.Instance.GetCustomPrefab(itemName);
                item = OTABRegistry.Instance.GetOriginalPrefab(itemName);
                if (item == null || custom != null) // need clone (not cloned yet / previously cloned, reactivate)
                {

                    if (data.Clone == null)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Clone)} missing");
                        return false;
                    }

                    if (data.Clone.From == null)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.From)}: Field missing");
                        return false;
                    }

                    if (OTABRegistry.IsCustomPrefab(data.Clone.From))
                    {
                        Plugin.LogError($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.From)}: Cannot clone from cloned prefab '{data.Clone.From}'");
                        return false;
                    }

                    var cloneFrom = OTABRegistry.Instance.GetOriginalPrefab(data.Clone.From);
                    if (!cloneFrom)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.From)}: Prefab '{data.Clone.From}' not found");
                        return false;
                    }

                    if (!cloneFrom.GetComponent<ItemDrop>())
                    {
                        Plugin.LogError($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.From)}: Prefab '{data.Clone.From}' has no ItemDrop");
                        return false;
                    }


                    if (custom == null)
                    {
                        // not cloned yet
                        item = OTABRegistry.Instance.CreateCustomPrefab(itemName, cloneFrom.name);
                    }
                    else
                    {
                        // previously cloned - reactivate
                        Plugin.LogServerDebug($"{model}.{nameof(data.Clone)}: Reactivating cloned prefab for '{cloneFrom.name}'");
                        item = OTABRegistry.Instance.ReactivateCustomPrefab(itemName, cloneFrom.name);
                    }

                    if (data.Clone.VisualFrom != null)
                    {
                        if (OTABRegistry.IsCustomPrefab(data.Clone.VisualFrom))
                        {
                            Plugin.LogError($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.VisualFrom)}: Cannot clone visuals from cloned prefab '{data.Clone.VisualFrom}'");
                            return false;
                        }
                        GameObject cloneVisualsFrom = OTABRegistry.Instance.GetOriginalPrefab(data.Clone.VisualFrom);
                        if (!cloneVisualsFrom)
                        {
                            Plugin.LogError( $"{model}.{nameof(data.Clone)}.{nameof(data.Clone.VisualFrom)}: Prefab '{data.Clone.VisualFrom}' not found" );
                            return false;
                        }
                        if (!ApplyVisualFrom(item, cloneVisualsFrom, model))
                        {
                            return false;
                        }
                    }

                    ItemManager.Instance.AddItem(new Jotunn.Entities.CustomItem(item, fixReference: false));
                }
                else
                {
                    OTABRegistry.Instance.MakeOriginalBackup(itemName);
                }

                OTABRegistry.Instance.ReservePrefab(itemName, item);
            }

            return true;
        }

        //------------------------------------------------
        // VALIDATE PREFAB
        //------------------------------------------------

        public override bool ValidatePrefab(string itemName, ItemFile data)
        {
            var model = $"{nameof(ItemFile)}.{itemName}";
            var error = false;

            var item = OTABRegistry.Instance.GetReservedPrefab(itemName);
            if (!item)
            {
                Plugin.LogError($"{model}: Prefab not found");
                error = true;
            }
            else
            {
                if (!item.GetComponent<ItemDrop>())
                {
                    Plugin.LogError($"{model}: Prefab has no ItemDrop (Prefab needs to be an item)");
                    error = true;
                }
            }

            if (data.Clone != null)
            {
                if (data.Clone.CustomIconName != null)
                {
                    var iconName = data.Clone.CustomIconName;
                    var iconExists = StaticContext.IconDataContext.iconTextures.ContainsKey(iconName);
                    if (iconExists == false)
                    {
                        Plugin.LogWarning($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.CustomIconName)}: Icon with name '{iconName}' not found - using original icon");
                        data.Clone.CustomIconName = null;
                    }
                }
            }

            if (data.Components.EggGrow == ComponentBehavior.Patch)
            {
                if (data.EggGrow.Grown != null)
                {
                    foreach (var (grownData, i) in data.EggGrow.Grown.Select((value, i) => (value, i)))
                    {
                        if (!OTABRegistry.Instance.PrefabExists(grownData.Prefab))
                        {
                            Plugin.LogError($"{model}.{nameof(data.EggGrow)}.{nameof(data.EggGrow.Grown)}.{i}.{nameof(grownData.Prefab)}: '{grownData.Prefab}' not found");
                            error = true;
                        }
                    }
                }
            }

            return error == false;
        }

        //------------------------------------------------
        // REGISTER PREFAB
        //------------------------------------------------

        public override void RegisterPrefab(string itemName, ItemFile data)
        {
            var model = $"{nameof(ItemFile)}.{itemName}";

            if (OTABRegistry.IsCustomPrefab(itemName))
            {
                Plugin.LogServerDebug($"{model}: Registering prefab");
                var item = OTABRegistry.Instance.GetReservedPrefab(itemName);
                ItemManager.Instance.RegisterItemInObjectDB(item);
            }
        }

        //------------------------------------------------
        // EDIT PREFAB
        //------------------------------------------------

        public override void EditPrefab(string itemName, ItemFile data)
        {
            var model = $"{nameof(ItemFile)}.{itemName}";

            var item = OTABRegistry.Instance.GetReservedPrefab(itemName);

            var itemItemDrop = item.GetComponent<ItemDrop>();
            var itemItemData = itemItemDrop.m_itemData;
            var itemItemDataShared = itemItemData.m_shared;

            var baseIcon = itemItemDataShared.m_icons?.FirstOrDefault();
            SaveIcon(baseIcon, $"{itemName} (original)");

            if (OTABRegistry.IsCustomPrefab(itemName))
            {
                PrepareClone(itemName, data, item);
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
                var itemFloating = OTABRegistry.Instance.GetOrAddComponent<Floating>(itemName, item);

                Plugin.LogServerDebug($"{model}.{nameof(data.Floating)}: Setting values");

                if (data.Floating.WaterLevelOffset.HasValue)
                {
                    itemFloating.m_waterLevelOffset = data.Floating.WaterLevelOffset.Value;
                }
            }
            else if (data.Components.Floating == ComponentBehavior.Remove)
            {
                OTABRegistry.Instance.DestroyComponentIfExists<Floating>(itemName, item);
            }

            //
            // EggGrow
            //

            if (data.Components.EggGrow == ComponentBehavior.Patch)
            {
                var itemEggGrow = OTABRegistry.Instance.GetOrAddComponent<EggGrow>(itemName, item);
                var itemEggGrowTrait = EggGrowTrait.GetOrAddComponent(item);

                if (data.EggGrow != null)
                {
                    Plugin.LogServerDebug($"{model}.{nameof(data.EggGrow)}: Setting values");

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
                        Heightmap.Biome mask = Heightmap.Biome.None;
                        foreach(var biome in data.EggGrow.RequireAnyBiome)
                        {
                            mask |= biome;
                        }
                        itemEggGrowTrait.m_requireBiome = mask;
                    }

                    if (data.EggGrow.RequireLiquid.HasValue)
                    {
                        itemEggGrowTrait.m_requireLiquid = data.EggGrow.RequireLiquid.Value;
                        if (data.EggGrow.RequireLiquidDepth.HasValue)
                        {
                            itemEggGrowTrait.m_requireLiquidDepth = data.EggGrow.RequireLiquidDepth.Value;
                        }
                    }

                    if (data.EggGrow.RequireGlobalKeys != null)
                    {
                        var keysList = ParseGlobalKeys(data.EggGrow.RequireGlobalKeys);
                        itemEggGrowTrait.SetRequiredGlobalKeys(keysList);
                    }

                    if (data.EggGrow.Grown != null)
                    {
                        var grownList = data.EggGrow.Grown.Select((g) => new EggGrowTrait.EggGrown(
                            weight: g.Weight,
                            prefab: g.Prefab,
                            tamed: g.Tamed,
                            showHatchEffect: g.ShowHatchEffect
                        )).ToArray();
                        itemEggGrowTrait.SetCustomGrownList(grownList);
                    }

                }

                Plugin.LogServerDebug($"{model}: Setting effects");
                if (itemEggGrow.m_hatchEffect == null || itemEggGrow.m_hatchEffect.m_effectPrefabs.Length == 0)
                {
                    itemEggGrow.m_hatchEffect = new EffectList
                    {
                        m_effectPrefabs = OTABUtils.PrefabUtils.CreateEffectList(new string[] {
                        "fx_chicken_birth",
                    })
                    };
                }

                // will be set seperatly
                itemEggGrow.m_tamed = true;
                itemEggGrow.m_grownPrefab = null;

            }
            else if (data.Components.EggGrow == ComponentBehavior.Remove)
            {
                OTABRegistry.Instance.DestroyComponentIfExists<EggGrow>(itemName, item);
            }

            // set last remaining egg values

            if (item.GetComponent<EggGrow>())
            {
                itemItemDrop.m_autoPickup = false;
                itemItemDrop.m_autoDestroy = false;
                itemItemDataShared.m_autoStack = false;
                StaticContext.ItemDataContext.RegisterEggSharedName(item);
            }
            //eggItemDataShared.m_value = 0; // todo: add yaml option for that

        }

        private void PrepareClone(string itemName, ItemFile data, UnityEngine.GameObject item)
        {
            var model = $"{nameof(ItemFile)}.{itemName}";

            Plugin.LogServerDebug($"{model}.{nameof(data.Clone)}: Setting ItemDrop.ItemData values");

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
                    var component = OTABRegistry.Instance.GetOrAddComponent<ScaledEgg>(itemName, item);
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
            if (data.Clone.CustomIconName != null)
            {
                Plugin.LogServerDebug($"{model}.{nameof(data.Clone)}: Setting custom icon");
                var tex2d = StaticContext.IconDataContext.iconTextures[data.Clone.CustomIconName];
                customIcon = SpriteUtils.TextureToSprite(tex2d);
            }

            // color/lights stuff

            if (data.Clone.ItemHueShift.HasValue || data.Clone.ItemSaturationShift.HasValue || data.Clone.ItemBrightnessShift.HasValue)
            {
                Plugin.LogServerDebug($"{model}.{nameof(data.Clone)}: Shifting item colors");
                float hueShift = data.Clone.ItemHueShift ?? 0f;
                float saturationShift = data.Clone.ItemSaturationShift ?? 0f;
                float brightnessShift = data.Clone.ItemBrightnessShift ?? 0f;
                ShiftItemColors(item, hueShift, saturationShift, brightnessShift);

                /*
                if (customIcon == null)
                {
                    customIcon = RenderItemIcon(item);
                }
                */

                if (customIcon == null && baseIcon != null)
                {
                    Plugin.LogServerDebug($"{model}.{nameof(data.Clone)}: Shifting icon colors");
                    customIcon = CreateShiftedSprite(baseIcon, hueShift, saturationShift, brightnessShift);
                }
            }

            if (data.Clone.LightsHueShift.HasValue || data.Clone.LightsSaturationShift.HasValue)
            {
                Plugin.LogServerDebug($"{model}.{nameof(data.Clone)}: Shifting light colors");
                float hueShift = data.Clone.LightsHueShift ?? 0f;
                float saturationShift = data.Clone.LightsSaturationShift ?? 0f;
                ShiftLightColors(item, hueShift, saturationShift);
            }

            if (data.Clone.DisableParticles.HasValue && data.Clone.DisableParticles.Value == true)
            {
                foreach (var r in item.GetComponentsInChildren<UnityEngine.ParticleSystemRenderer>(true))
                {
                    UnityEngine.Object.DestroyImmediate(r);
                    //r.enabled = false;
                }
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
                SaveIcon(customIcon, $"{itemName} (final)");
                originalIcons[itemName] = itemItemDataShared.m_icons;
                itemItemDataShared.m_icons = new[] { customIcon };
            }

        }

        //------------------------------------------------
        // CLEANUP
        //------------------------------------------------

        public override void FinalizeProcess()
        {
        }

        //------------------------------------------------
        // UNREGISTER PREFAB
        //------------------------------------------------

        public override void RestorePrefab(string itemName)
        {
            if (originalIcons.TryGetValue(itemName, out UnityEngine.Sprite[] icons))
            {
                // for some reasons we need to set the original icon back before restoring the prefab
                // this seems unneccessary but I tell you it is!
                // otherwiese the icons could look distorted if we rejoin the world
                var itemItemDrop = OTABRegistry.Instance.GetOriginalPrefab(itemName).GetComponent<ItemDrop>();
                foreach (var s in itemItemDrop.m_itemData.m_shared.m_icons)
                {
                    if (s.texture) UnityEngine.Object.DestroyImmediate(s.texture);
                    UnityEngine.Object.DestroyImmediate(s);
                }
                itemItemDrop.m_itemData.m_shared.m_icons = icons;
                // now the next time we (re)enter a world the icons of cloned prefabs still look nice <3
            }

            if (customVisuals.TryGetValue(itemName, out var customVisual))
            {
                if (customVisual) UnityEngine.Object.DestroyImmediate(customVisual);
                customVisuals.Remove(itemName);
            }

            if (originalVisualStates.TryGetValue(itemName, out var visualStates))
            {
                foreach (var pair in visualStates)
                {
                    if (pair.Key) pair.Key.SetActive(pair.Value);
                }
                originalVisualStates.Remove(itemName);
            }

            OTABRegistry.Instance.RestorePrefab(itemName, (current, backup) => {
                PrefabUtils.RestoreChildRenderers(current, backup);
                PrefabUtils.RestoreChildLights(current, backup);
                PrefabUtils.RestoreChildParticleSystems(current, backup);
                PrefabUtils.RestoreChildParticleRenderers(current, backup);
                PrefabUtils.RestoreChildLightFlickerBaseColor(current, backup);
                // we dont need to use this anymore (or do we? it doesnt seem to be the case)
                //RestoreHelper.RestoreItemIcons(current, backup);
            });

            if (OTABRegistry.IsCustomPrefab(itemName))
            {
                ItemManager.Instance.RemoveItem(itemName);
            }
        }

        //------------------------------------------------
        // CLEANUP
        //------------------------------------------------

        public override void CleanupProcess()
        {
            originalIcons.Clear();
            customVisuals.Clear();
            originalVisualStates.Clear();
        }

    }

}
