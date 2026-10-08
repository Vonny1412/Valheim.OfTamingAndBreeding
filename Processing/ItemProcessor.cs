using Jotunn.Managers;
using OfTamingAndBreeding.Components.Traits;
using OfTamingAndBreeding.Data.Models;
using OfTamingAndBreeding.Data.Models.SubData;
using OfTamingAndBreeding.Processing.Core;
using OfTamingAndBreeding.Processing.Registry;
using System.Collections.Generic;
using System.IO;
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

        private readonly List<Sprite> customIcons = new List<Sprite>();

        private readonly Dictionary<string, GameObject> customVisuals = new Dictionary<string, GameObject>();

        private readonly Dictionary<string, Dictionary<GameObject, bool>> originalVisualStates = new Dictionary<string, Dictionary<GameObject, bool>>();

        private readonly HashSet<Texture2D> customTextures = new HashSet<Texture2D>();

        private static readonly HashSet<int> eggSharedNameHashes = new HashSet<int>();

        public static bool IsRegisteredEgg(string sharedName)
        {
            return eggSharedNameHashes.Contains(sharedName.GetStableHashCode());
        }

        private static readonly string path_icons = "_icons";
        private static readonly string path_icons_original = "original";
        private static readonly string path_icons_final = "final";

        //------------------------------------------------

        public override bool PrepareProcess()
        {
            if (ZNet.instance.IsServer() && Plugin.Configs.ExportIconsToCache.Value == true)
            {
                var icons_cache = Path.Combine(Plugin.CacheDir, path_icons);
                if (Directory.Exists(icons_cache))
                {
                    Directory.Delete(icons_cache);
                }
            }
            return true;
        }

        public override bool ReservePrefabName(string itemName)
        {
            if (!OTABPrefabRegistry.Instance.ReservePrefabName(itemName))
            {
                var model = $"{nameof(ItemFile)}.{itemName}";
                Plugin.LogError($"{model}: Prefab is already reserved");
                return false;
            }
            return true;
        }

        public override bool ValidateData(string itemName, ItemFile data)
        {
            var model = $"{nameof(ItemFile)}.{itemName}";
            var valid = true;

            // ---------------------------
            // Clone
            // ---------------------------

            //var registeredPrefab = OTABPrefabRegistry.Instance.GetRegisteredPrefab(itemName);
            var isOriginalPrefab = OTABPrefabRegistry.IsCustomPrefab(itemName) == false;

            if (data.Clone == null)
            {
                // we dont want to clone
                // but we need to check if original exists
                if (!isOriginalPrefab)
                {
                    Plugin.LogError($"{model}: Prefab not found - Field '{nameof(data.Clone)}' missing?");
                    valid = false;
                }
            }
            else
            {
                if (isOriginalPrefab)
                {
                    Plugin.LogError($"{model}.{nameof(data.Clone)}: Cannot create cloned prefab with name '{itemName}' because it already exists.");
                    valid = false;
                }

                if (string.IsNullOrEmpty(data.Clone.From))
                {
                    Plugin.LogError($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.From)}: Missing field");
                    valid = false;
                }
                else
                {
                    if (OTABPrefabRegistry.IsCustomPrefab(data.Clone.From))
                    {
                        Plugin.LogError($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.From)}: Source '{data.Clone.From}' needs to be valid original prefab");
                        valid = false;
                    }
                }

                if (data.Clone.Scale.HasValue)
                {
                    if (data.Clone.Scale.Value <= 0)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.Scale)}: Zero or negative values not allowed");
                        valid = false;
                    }
                }

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
                if (data.Clone.VisualFrom != null)
                {
                    if (OTABPrefabRegistry.IsCustomPrefab(data.Clone.VisualFrom))
                    {
                        Plugin.LogError($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.VisualFrom)}: Cannot clone visuals from cloned prefab '{data.Clone.VisualFrom}'");
                        valid = false;
                    }
                    GameObject cloneVisualsFrom = OTABPrefabRegistry.Instance.GetRegisteredPrefab(data.Clone.VisualFrom);
                    if (!cloneVisualsFrom)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.VisualFrom)}: Prefab '{data.Clone.VisualFrom}' not found");
                        valid = false;
                    }
                }
            }

            if (!valid)
            {
                // cloning needs to be valid first!
                return false;
            }

            var sourceName = data.Clone?.From ?? itemName;
            var source = OTABPrefabRegistry.Instance.GetRegisteredPrefab(sourceName);
            if (!source.GetComponent<ItemDrop>())
            {
                Plugin.LogError($"{model}: Prefab has no ItemDrop (Prefab needs to be an item)");
                return false;
            }


            // ---------------------------
            // Item
            // ---------------------------

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

            // ---------------------------
            // Floating
            // ---------------------------

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

            // ---------------------------
            // EggGrow
            // ---------------------------

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

            if (data.EggGrow != null)
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
                        if (grownData.Prefab == null)
                        {
                            Plugin.LogError($"{model}.{nameof(data.EggGrow)}.{nameof(data.EggGrow.Grown)}.{i}.{nameof(grownData.Prefab)}: Field is empty");
                            valid = false;
                        }
                        else
                        {
                            if (!OTABPrefabRegistry.Instance.PrefabWillExist(grownData.Prefab))
                            {
                                Plugin.LogError($"{model}.{nameof(data.EggGrow)}.{nameof(data.EggGrow.Grown)}.{i}.{nameof(grownData.Prefab)}: '{grownData.Prefab}' not found");
                                valid = false;
                            }
                        }
                    }
                }
            }

            return valid;
        }







        public override bool RegisterPrefab(string itemName, ItemFile data)
        {
            var model = $"{nameof(ItemFile)}.{itemName}";

            var custom = OTABPrefabRegistry.Instance.GetCustomPrefab(itemName);
            var item = OTABPrefabRegistry.Instance.GetRegisteredPrefab(itemName);
            if (item == null || custom != null) // need clone (not cloned yet / previously cloned, reactivate)
            {
                if (data.Clone == null)
                {
                    // should have been validated already
                    return false;
                }

                if (custom == null)
                {
                    // not cloned yet
                    item = OTABPrefabRegistry.Instance.CreateCustomPrefab(itemName, data.Clone.From);
                }
                else
                {
                    // previously cloned - reactivate
                    Plugin.LogDebug($"{model}.{nameof(data.Clone)}: Reactivating cloned prefab for '{data.Clone.From}'");
                    item = OTABPrefabRegistry.Instance.ReactivateCustomPrefab(itemName, data.Clone.From);
                }

                if (item)
                {
                    if (data.Clone.VisualFrom != null)
                    {
                        GameObject cloneVisualsFrom = OTABPrefabRegistry.Instance.GetRegisteredPrefab(data.Clone.VisualFrom);
                        if (!ApplyVisualFrom(item, cloneVisualsFrom, model))
                        {
                            return false;
                        }
                    }

                    ItemManager.Instance.AddItem(new Jotunn.Entities.CustomItem(item, fixReference: false));
                    ItemManager.Instance.RegisterItemInObjectDB(item);
                }

            }
            else
            {
                OTABPrefabRegistry.Instance.MakeOriginalBackup(itemName);
            }

            if (!item)
            {
                Plugin.LogDebug($"{model}: Prefab '{itemName}' not found");
                return false;
            }

            return true;
        }

        public override bool FinalizeProcess()
        {
            return true;
        }

        public override void RestorePrefab(string itemName)
        {
            OTABPrefabRegistry.Instance.RestorePrefab(itemName, (current, backup) => {

                PrefabUtils.RestoreComponent<EggGrow>(current, backup);
                PrefabUtils.RestoreComponent<Floating>(current, backup);
                PrefabUtils.RestoreComponent<ItemDrop>(current, backup);

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

                currentItemDrop.m_itemData.m_shared.m_icons = backupItemDrop.m_itemData.m_shared.m_icons;
            });

            if (OTABPrefabRegistry.IsCustomPrefab(itemName))
            {
                ItemManager.Instance.RemoveItem(itemName);
            }
        }

        public override void CleanupProcess()
        {

            foreach (var texture in customTextures)
            {
                if (texture)
                {
                    UnityEngine.Object.DestroyImmediate(texture);
                }
            }

            foreach (var customVisual in customVisuals.Values)
            {
                if (customVisual)
                {
                    UnityEngine.Object.DestroyImmediate(customVisual);
                }
            }

            foreach (var visualStates in originalVisualStates.Values)
            {
                foreach (var pair in visualStates)
                {
                    if (pair.Key)
                    {
                        pair.Key.SetActive(pair.Value);
                    }
                }
            }

            foreach (var sprite in customIcons)
            {
                if (!sprite)
                {
                    continue;
                }
                if (sprite.texture)
                {
                    UnityEngine.Object.DestroyImmediate(sprite.texture);
                }
                UnityEngine.Object.DestroyImmediate(sprite);
            }

            customTextures.Clear();
            customIcons.Clear();
            customVisuals.Clear();
            originalVisualStates.Clear();
            eggSharedNameHashes.Clear();

            EggGrowTrait.s_grownListStore.Clear();
        }

    }

}
