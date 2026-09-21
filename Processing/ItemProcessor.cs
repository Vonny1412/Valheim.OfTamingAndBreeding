using Jotunn.Managers;
using OfTamingAndBreeding.Components;
using OfTamingAndBreeding.Components.Traits;
using OfTamingAndBreeding.Data.Models;
using OfTamingAndBreeding.Data.Models.SubData;
using OfTamingAndBreeding.Processing.Core;
using OfTamingAndBreeding.Registry;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OfTamingAndBreeding.Processing
{
    internal partial class ItemProcessor : DataProcessor<ItemFile>
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
            var valid = true;

            if (data.Clone != null)
            {
                if (data.Clone.Name == null)
                {
                    Plugin.LogError($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.Name)}: Missing field");
                    valid = false;
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
                        valid = false;
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
                        valid = false;
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
                        valid = false;
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
                    valid = false;
                }
                else
                {
                    foreach (var (grownData, i) in data.EggGrow.Grown.Select((value, i) => (value, i)))
                    {
                        grownData.Weight = Math.Max(0f, grownData.Weight);
                        if (grownData.Prefab == null)
                        {
                            Plugin.LogError($"{model}.{nameof(data.EggGrow)}.{nameof(data.EggGrow.Grown)}.{i}.{nameof(grownData.Prefab)}: Field is empty");
                            valid = false;
                        }
                    }
                }
            }

            return valid;
        }

        //------------------------------------------------
        // RESERVE PREFAB
        //------------------------------------------------

        public override bool ReservePrefab(string itemName, ItemFile data)
        {
            var model = $"{nameof(ItemFile)}.{itemName}";
            var valid = true;

            var item = OTABPrefabRegistry.Instance.GetReservedPrefab(itemName);
            if (item == null)
            {

                var custom = OTABPrefabRegistry.Instance.GetCustomPrefab(itemName);
                item = OTABPrefabRegistry.Instance.GetOriginalPrefab(itemName);
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

                    if (OTABPrefabRegistry.IsCustomPrefab(data.Clone.From))
                    {
                        Plugin.LogError($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.From)}: Cannot clone from cloned prefab '{data.Clone.From}'");
                        return false;
                    }

                    var cloneFrom = OTABPrefabRegistry.Instance.GetOriginalPrefab(data.Clone.From);
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
                        item = OTABPrefabRegistry.Instance.CreateCustomPrefab(itemName, cloneFrom.name);
                    }
                    else
                    {
                        // previously cloned - reactivate
                        Plugin.LogDebug($"{model}.{nameof(data.Clone)}: Reactivating cloned prefab for '{cloneFrom.name}'");
                        item = OTABPrefabRegistry.Instance.ReactivateCustomPrefab(itemName, cloneFrom.name);
                    }
                    if (!item)
                    {
                        return false;
                    }

                    if (data.Clone.VisualFrom != null)
                    {
                        if (OTABPrefabRegistry.IsCustomPrefab(data.Clone.VisualFrom))
                        {
                            Plugin.LogError($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.VisualFrom)}: Cannot clone visuals from cloned prefab '{data.Clone.VisualFrom}'");
                            return false;
                        }
                        GameObject cloneVisualsFrom = OTABPrefabRegistry.Instance.GetOriginalPrefab(data.Clone.VisualFrom);
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
                    OTABPrefabRegistry.Instance.MakeOriginalBackup(itemName);
                }

                OTABPrefabRegistry.Instance.ReservePrefab(itemName, item);
            }

            return valid;
        }

        //------------------------------------------------
        // VALIDATE PREFAB
        //------------------------------------------------

        public override bool ValidatePrefab(string itemName, ItemFile data)
        {
            var model = $"{nameof(ItemFile)}.{itemName}";
            var valid = true;

            var item = OTABPrefabRegistry.Instance.GetReservedPrefab(itemName);
            if (!item)
            {
                Plugin.LogError($"{model}: Prefab not found");
                valid = false;
            }
            else
            {
                if (!item.GetComponent<ItemDrop>())
                {
                    Plugin.LogError($"{model}: Prefab has no ItemDrop (Prefab needs to be an item)");
                    valid = false;
                }
            }

            if (data.Clone != null)
            {
                if (!string.IsNullOrEmpty(data.Clone.CustomIcon))
                {
                    if (!TextureProcessor.TryGetSprite(data.Clone.CustomIcon, out var _))
                    {
                        Plugin.LogWarning($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.CustomIcon)}: Texture '{data.Clone.CustomIcon}' not found");
                        valid = false;
                    }
                }
                if (!string.IsNullOrEmpty(data.Clone.AttachedSprite))
                {
                    if (!TextureProcessor.TryGetSprite(data.Clone.AttachedSprite, out var _))
                    {
                        Plugin.LogWarning($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.AttachedSprite)}: Texture '{data.Clone.AttachedSprite}' not found");
                        valid = false;
                    }
                }
            }


            if (data.Components.EggGrow == ComponentBehavior.Patch)
            {
                if (data.EggGrow.Grown != null)
                {
                    foreach (var (grownData, i) in data.EggGrow.Grown.Select((value, i) => (value, i)))
                    {
                        if (!OTABPrefabRegistry.Instance.PrefabExists(grownData.Prefab))
                        {
                            Plugin.LogError($"{model}.{nameof(data.EggGrow)}.{nameof(data.EggGrow.Grown)}.{i}.{nameof(grownData.Prefab)}: '{grownData.Prefab}' not found");
                            valid = false;
                        }
                    }
                }
            }

            return valid;
        }

        //------------------------------------------------
        // REGISTER PREFAB
        //------------------------------------------------

        public override void RegisterPrefab(string itemName, ItemFile data)
        {
            var model = $"{nameof(ItemFile)}.{itemName}";

            if (OTABPrefabRegistry.IsCustomPrefab(itemName))
            {
                Plugin.LogDebug($"{model}: Registering prefab");
                var item = OTABPrefabRegistry.Instance.GetReservedPrefab(itemName);
                ItemManager.Instance.RegisterItemInObjectDB(item);
            }
        }

        //------------------------------------------------
        // EDIT PREFAB
        //------------------------------------------------

        public override bool ProcessPrefab(string itemName, ItemFile data)
        {
            var model = $"{nameof(ItemFile)}.{itemName}";
            var error = false;

            var item = OTABPrefabRegistry.Instance.GetReservedPrefab(itemName);

            var itemItemDrop = item.GetComponent<ItemDrop>();
            var itemItemData = itemItemDrop.m_itemData;
            var itemItemDataShared = itemItemData.m_shared;

            var baseIcon = itemItemDataShared.m_icons?.FirstOrDefault();
            SaveIcon(baseIcon, $"{itemName} (original)");

            if (OTABPrefabRegistry.IsCustomPrefab(itemName))
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
                        itemEggGrowTrait.m_requiredGlobalKeysStoreIndex = EggGrowTrait.s_requiredGlobalKeysStore.Add(keysList);
                    }

                    if (data.EggGrow.Grown != null)
                    {
                        var grownList = data.EggGrow.Grown.Select((g) => new EggGrowTrait.EggGrown(
                            weight: g.Weight,
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
                itemEggGrow.m_tamed = true;
                itemEggGrow.m_grownPrefab = null;

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





        [NonSerialized] private static readonly HashSet<int> eggSharedNameHashes = new HashSet<int>();
        public static bool IsRegisteredEgg(string sharedName)
        {
            return eggSharedNameHashes.Contains(sharedName.GetStableHashCode());
        }





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
                }
            }

            if (data.Clone.LightsHueShift.HasValue || data.Clone.LightsSaturationShift.HasValue)
            {
                Plugin.LogDebug($"{model}.{nameof(data.Clone)}: Shifting light colors");
                float hueShift = data.Clone.LightsHueShift ?? 0f;
                float saturationShift = data.Clone.LightsSaturationShift ?? 0f;
                ShiftLightColors(item, hueShift, saturationShift);
            }




            if (data.Clone.DisableParticles == true)
            {
                foreach (var ps in item.GetComponentsInChildren<ParticleSystem>(true))
                {
                    var emission = ps.emission;
                    emission.enabled = false;
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
                originalIcons.TryAdd(itemName, itemItemDataShared.m_icons); // maybe its already added by custom visual part
                itemItemDataShared.m_icons = new[] { customIcon };
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
                var itemItemDrop = OTABPrefabRegistry.Instance.GetOriginalPrefab(itemName).GetComponent<ItemDrop>();
                foreach (var s in itemItemDrop.m_itemData.m_shared.m_icons)
                {
                    if (!s)
                    {
                        continue;
                    }
                    if (s.texture)
                    {
                        UnityEngine.Object.DestroyImmediate(s.texture);
                    }
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

            OTABPrefabRegistry.Instance.RestorePrefab(itemName, (current, backup) => {

                var currentItemDrop = current.GetComponent<ItemDrop>();
                var backupItemDrop = backup.GetComponent<ItemDrop>();

                if (currentItemDrop && backupItemDrop)
                {
                    var currentShared = currentItemDrop.m_itemData.m_shared;
                    var backupShared = backupItemDrop.m_itemData.m_shared;

                    currentShared.m_name = backupShared.m_name;
                    currentShared.m_description = backupShared.m_description;
                    currentShared.m_itemType = backupShared.m_itemType;
                    currentShared.m_weight = backupShared.m_weight;
                    currentShared.m_teleportable = backupShared.m_teleportable;

                    currentShared.m_maxStackSize = backupShared.m_maxStackSize;
                    currentShared.m_maxQuality = backupShared.m_maxQuality;
                    currentShared.m_scaleByQuality = backupShared.m_scaleByQuality;
                    currentShared.m_scaleWeightByQuality = backupShared.m_scaleWeightByQuality;

                    currentShared.m_autoStack = backupShared.m_autoStack;
                }

                var currentFloating = current.GetComponent<Floating>();
                var backupFloating = backup.GetComponent<Floating>();

                if (currentFloating && backupFloating)
                {
                    currentFloating.enabled = backupFloating.enabled;
                }

                var currentEggGrow = current.GetComponent<EggGrow>();
                var backupEggGrow = backup.GetComponent<EggGrow>();

                if (currentEggGrow && backupEggGrow)
                {
                    currentEggGrow.m_grownPrefab = backupEggGrow.m_grownPrefab;
                    currentEggGrow.m_hatchEffect = backupEggGrow.m_hatchEffect;
                    currentEggGrow.m_growingObject = backupEggGrow.m_growingObject;
                    currentEggGrow.m_notGrowingObject = backupEggGrow.m_notGrowingObject;
                }

                PrefabUtils.RestoreChildRenderers(current, backup);
                PrefabUtils.RestoreChildLights(current, backup);
PrefabUtils.RestoreChildParticleSystems(current, backup);
                PrefabUtils.RestoreChildParticleRenderers(current, backup);
                PrefabUtils.RestoreChildLightFlickerBaseColor(current, backup);



            });

            if (OTABPrefabRegistry.IsCustomPrefab(itemName))
            {
                ItemManager.Instance.RemoveItem(itemName);
            }
        }

        //------------------------------------------------
        // CLEANUP
        //------------------------------------------------

        public override void CleanupProcess()
        {

            foreach (var texture in customTextures)
            {
                if (texture)
                {
                    // destroy baked textures
                    // not destroying them will result in ghost objects
                    UnityEngine.Object.DestroyImmediate(texture);
                }
            }
            customTextures.Clear();

            originalIcons.Clear();
            customVisuals.Clear();
            originalVisualStates.Clear();
            eggSharedNameHashes.Clear();

            EggGrowTrait.s_requiredGlobalKeysStore.Clear();
            EggGrowTrait.s_grownListStore.Clear();
        }

    }

}
