using Jotunn.Managers;
using OfTamingAndBreeding.Components;
using OfTamingAndBreeding.Data.Models;
using OfTamingAndBreeding.Data.Models.SubData;
using OfTamingAndBreeding.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OfTamingAndBreeding.Registry.Processing
{

    internal class EggProcessor : Base.DataProcessor<EggData>
    {

        public override string DirectoryName => EggData.DirectoryName;

        public override string PrefabTypeName => "item";

        public override string GetDataKey(string filePath) => null;

        public override bool LoadFromFile(string filePath) => LoadFromYamlFile(filePath);

        //------------------------------------------------
        // PREPARE
        //------------------------------------------------

        public override void PrepareProcess()
        {

        }

        //------------------------------------------------
        // VALIDATE DATA
        //------------------------------------------------

        public override bool ValidateData(string eggName, EggData data)
        {
            var model = $"{nameof(EggData)}.{eggName}";
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

            switch (data.Components.EggGrow)
            {
                case ComponentBehavior.Remove:
                    //Plugin.LogDebug($"{model}.{nameof(data.Components)}.{nameof(data.Components.EggGrow)}({nameof(ComponentBehavior.Remove)}): Component cannot be removed");
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

            if (data.Item != null && data.Components.Item == ComponentBehavior.Patch)
            {
                // nothing to validate here
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

        public override bool ReservePrefab(string eggName, EggData data)
        {
            var model = $"{nameof(EggData)}.{eggName}";

            var egg = PrefabRegistry.Instance.GetReservedPrefab(eggName);
            if (egg == null)
            {

                var custom = PrefabRegistry.Instance.GetCustomPrefab(eggName);
                egg = PrefabRegistry.Instance.GetOriginalPrefab(eggName);
                if (egg == null || custom != null) // need clone (not cloned yet / previously cloned, reactivate)
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

                    if (PrefabRegistry.IsCustomPrefab(data.Clone.From))
                    {
                        Plugin.LogError($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.From)}: Cannot clone from cloned prefab '{data.Clone.From}'");
                        return false;
                    }

                    var cloneFrom = PrefabRegistry.Instance.GetOriginalPrefab(data.Clone.From);
                    if (!cloneFrom)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.From)}: Prefab '{data.Clone.From}' not found");
                        return false;
                    }

                    if (custom == null)
                    {
                        // not cloned yet
                        egg = PrefabRegistry.Instance.CreateCustomPrefab(eggName, cloneFrom.name);
                    }
                    else
                    {
                        // previously cloned - reactivate
                        Plugin.LogServerDebug($"{model}.{nameof(data.Clone)}: Reactivating cloned prefab for '{cloneFrom.name}'");
                        egg = PrefabRegistry.Instance.ReactivateCustomPrefab(eggName, cloneFrom.name);
                    }

                    ItemManager.Instance.AddItem(new Jotunn.Entities.CustomItem(egg, fixReference: false));
                    PrepareClone(eggName, data, egg);
                }
                else
                {
                    PrefabRegistry.Instance.MakeOriginalBackup(eggName);
                }

                PrefabRegistry.Instance.ReservePrefab(eggName, egg);
            }

            return true;
        }

        private void PrepareClone(string eggName, EggData data, UnityEngine.GameObject egg)
        {
            var model = $"{nameof(EggData)}.{eggName}";

            var eggItemDrop = egg.GetComponent<ItemDrop>(); // required (checked in ValidatePrefab)
            Plugin.LogServerDebug($"{model}.{nameof(data.Clone)}: Setting ItemDrop.ItemData values");

            var eggItemData = eggItemDrop.m_itemData;
            var eggItemDataShared = eggItemData.m_shared;

            if (data.Clone.Name != null) eggItemDataShared.m_name = data.Clone.Name;
            if (data.Clone.Description != null) eggItemDataShared.m_description = data.Clone.Description;

            if (data.Clone.Scale.HasValue)
            {
                var customScale = data.Clone.Scale.Value;
                if (customScale != 1 && customScale > 0)
                {
                    var component = PrefabRegistry.Instance.GetOrAddComponent<ScaledEgg>(eggName, egg);
                    component.m_scale = customScale;
                }
            }
        }

        //------------------------------------------------
        // VALIDATE PREFAB
        //------------------------------------------------

        public override bool ValidatePrefab(string eggName, EggData data)
        {
            var model = $"{nameof(EggData)}.{eggName}";
            var error = false;

            var egg = PrefabRegistry.Instance.GetReservedPrefab(eggName);
            if (!egg)
            {
                Plugin.LogError($"{model}: Prefab not found");
                error = true;
            }
            else
            {
                if (!egg.GetComponent<ItemDrop>())
                {
                    Plugin.LogError($"{model}: Prefab has no ItemDrop (Prefab needs to be an item)");
                    error = true;
                }
            }

            if (data.Components.Item == ComponentBehavior.Patch)
            {
                if (data.Item != null)
                {
                    if (data.Item.CustomIconName != null)
                    {
                        var iconName = data.Item.CustomIconName;
                        var iconExists = StaticContext.IconDataContext.iconTextures.ContainsKey(iconName);
                        if (iconExists == false)
                        {
                            Plugin.LogWarning($"{model}.{nameof(data.Item)}.{nameof(data.Item.CustomIconName)}: Icon with name '{iconName}' not found - using original icon");
                            data.Item.CustomIconName = null;
                        }
                    }
                }
            }

            if (data.Components.EggGrow == ComponentBehavior.Patch)
            {
                if (data.EggGrow.Grown != null)
                {
                    foreach (var (grownData, i) in data.EggGrow.Grown.Select((value, i) => (value, i)))
                    {
                        if (!PrefabRegistry.Instance.PrefabExists(grownData.Prefab))
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

        public override void RegisterPrefab(string eggName, EggData data)
        {
            var model = $"{nameof(EggData)}.{eggName}";

            if (PrefabRegistry.IsCustomPrefab(eggName))
            {
                Plugin.LogServerDebug($"{model}: Registering prefab");
                var egg = PrefabRegistry.Instance.GetReservedPrefab(eggName);
                ItemManager.Instance.RegisterItemInObjectDB(egg);
            }
        }

        //------------------------------------------------
        // EDIT PREFAB
        //------------------------------------------------

        public override void EditPrefab(string eggName, EggData data)
        {
            var model = $"{nameof(EggData)}.{eggName}";

            var egg = PrefabRegistry.Instance.GetReservedPrefab(eggName);
            var eggComponent = PrefabRegistry.Instance.GetOrAddComponent<OTABEgg>(eggName, egg);

            //
            // set values
            //

            var itemDrop = egg.GetComponent<ItemDrop>();
            var baseIcon = itemDrop.m_itemData.m_shared.m_icons?.FirstOrDefault();
            SaveIcon(baseIcon, $"{eggName} (original)");

            if (data.Components.Item == ComponentBehavior.Patch)
            {
                if (data.Item != null)
                {
                    var eggItemDrop = egg.GetComponent<ItemDrop>(); // required (checked in ValidatePrefab)
                    Plugin.LogServerDebug($"{model}.{nameof(data.Item)}: Setting values");

                    var eggItemData = eggItemDrop.m_itemData;
                    var eggItemDataShared = eggItemData.m_shared;

                    // defaults
                    eggItemDataShared.m_autoStack = false;
                    eggItemDrop.m_autoPickup = false;
                    eggItemDrop.m_autoDestroy = false;

                    if (data.Item.Weight != null) eggItemDataShared.m_weight = (float)data.Item.Weight;
                    if (data.Item.ScaleByQuality != null) eggItemDataShared.m_scaleByQuality = (float)data.Item.ScaleByQuality;
                    else eggItemDataShared.m_scaleByQuality = 1; // required
                    if (data.Item.ScaleWeightByQuality != null) eggItemDataShared.m_scaleWeightByQuality = (float)data.Item.ScaleWeightByQuality;
                    else eggItemDataShared.m_scaleWeightByQuality = 1; // required
                    if (data.Item.Value != null) eggItemDataShared.m_value = (int)data.Item.Value;
                    else eggItemDataShared.m_value = 0; // required

                    if (data.Item.Teleportable != null) eggItemDataShared.m_teleportable = (bool)data.Item.Teleportable;
                    if (data.Item.MaxStackSize != null) eggItemDataShared.m_maxStackSize = (int)data.Item.MaxStackSize;
                    if (data.Item.MaxQuality != null) eggItemDataShared.m_maxQuality = (int)data.Item.MaxQuality;

                    if (data.Item.ItemType != null)
                    {
                        if (Enum.TryParse<ItemDrop.ItemData.ItemType>(data.Item.ItemType, ignoreCase: true, out var result))
                        {
                            eggItemDataShared.m_itemType = result;
                        }
                        else
                        {
                            //eggItemDataShared.m_itemType = ItemDrop.ItemData.ItemType.Misc;
                        }
                    }



                    UnityEngine.Sprite customIcon = null;
                    if (data.Item.CustomIconName != null)
                    {
                        Plugin.LogServerDebug($"{model}: Setting custom icon");
                        var tex2d = StaticContext.IconDataContext.iconTextures[data.Item.CustomIconName];
                        customIcon = SpriteUtils.TextureToSprite(tex2d);
                    }
                    else if (baseIcon != null && ColorUtils.TryParseColor(data.Item.ItemTintRgb, out UnityEngine.Color iconTint))
                    {
                        Plugin.LogServerDebug($"{model}: Tinting icon");
                        customIcon = Utils.TintUtils.CreateTintedSprite(baseIcon, iconTint);
                    }
                    if (customIcon != null)
                    {
                        SaveIcon(customIcon, $"{eggName} (final)");
                        originalIcons[eggName] = itemDrop.m_itemData.m_shared.m_icons;
                        itemDrop.m_itemData.m_shared.m_icons = new[] { customIcon };
                    }



                    Plugin.LogServerDebug($"{model}: Tinting prefab, lights and particles");

                    // multiply = true by default to preserve texture contrast;
                    // kept as parameter for possible future advanced tint modes
                    if (ColorUtils.TryParseColor(data.Item.ItemTintRgb, out UnityEngine.Color itemTint))
                    {
                        TintUtils.TintPrefab(egg, itemTint, true);
                    }
                    if (ColorUtils.TryParseColor(data.Item.LightsTintRgb, out UnityEngine.Color lightsTint))
                    {
                        TintUtils.TintLights(egg, lightsTint, true);
                    }
                    if (ColorUtils.TryParseColor(data.Item.ParticlesTintRgb, out UnityEngine.Color particlesTint))
                    {
                        TintUtils.TintParticleSystems(egg, particlesTint, true);
                    }
                    if (data.Item.DisableParticles)
                    {
                        foreach (var r in egg.GetComponentsInChildren<UnityEngine.ParticleSystemRenderer>(true))
                        {
                            UnityEngine.Object.DestroyImmediate(r);
                            //r.enabled = false;
                        }
                    }

                    // scale lights
                    var lightsScale = data.Item.LightsScale;
                    foreach (var l in egg.GetComponentsInChildren<UnityEngine.Light>(true))
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
            else if (data.Components.Item == ComponentBehavior.Remove)
            {
                // ignore, cannot be removed
            }

            if (data.Components.EggGrow == ComponentBehavior.Patch)
            {
                var eggEggGrow = PrefabRegistry.Instance.GetOrAddComponent<EggGrow>(eggName, egg);

                if (data.EggGrow != null)
                {
                    Plugin.LogServerDebug($"{model}.{nameof(data.EggGrow)}: Setting values");

                    if (data.EggGrow.GrowTime.HasValue)
                    {
                        eggEggGrow.m_growTime = data.EggGrow.GrowTime.Value;
                    }




                    if (data.EggGrow.UpdateInterval != null) eggEggGrow.m_updateInterval = (float)data.EggGrow.UpdateInterval;
                    if (data.EggGrow.RequireNearbyFire != null) eggEggGrow.m_requireNearbyFire = (bool)data.EggGrow.RequireNearbyFire;
                    else eggEggGrow.m_requireNearbyFire = false;
                    if (data.EggGrow.RequireUnderRoof != null) eggEggGrow.m_requireUnderRoof = (bool)data.EggGrow.RequireUnderRoof;
                    else eggEggGrow.m_requireUnderRoof = false;
                    if (data.EggGrow.RequireCoverPercentige != null) eggEggGrow.m_requireCoverPercentige = (float)data.EggGrow.RequireCoverPercentige;
                    else eggEggGrow.m_requireCoverPercentige = 0;






                    if (data.EggGrow.RequireAnyBiome != null)
                    {
                        Heightmap.Biome mask = Heightmap.Biome.None;
                        foreach(var biome in data.EggGrow.RequireAnyBiome)
                        {
                            mask |= biome;
                        }
                        eggComponent.m_requireBiome = mask;
                    }

                    if (data.EggGrow.RequireLiquid.HasValue)
                    {
                        eggComponent.m_requireLiquid = data.EggGrow.RequireLiquid.Value;
                        if (data.EggGrow.RequireLiquidDepth.HasValue)
                        {
                            eggComponent.m_requireLiquidDepth = data.EggGrow.RequireLiquidDepth.Value;
                        }
                    }

                    if (data.EggGrow.Grown != null)
                    {
                        eggComponent.SetCustomGrownList(data.EggGrow.Grown);
                    }

                }

                Plugin.LogServerDebug($"{model}: Setting effects");
                if (eggEggGrow.m_hatchEffect == null || eggEggGrow.m_hatchEffect.m_effectPrefabs.Length == 0)
                {
                    eggEggGrow.m_hatchEffect = new EffectList
                    {
                        m_effectPrefabs = Utils.PrefabUtils.CreateEffectList(new string[] {
                        "fx_chicken_birth",
                    })
                    };
                }

                // will be set seperatly
                eggEggGrow.m_tamed = true;
                eggEggGrow.m_grownPrefab = null;

            }
            else if (data.Components.EggGrow == ComponentBehavior.Remove)
            {
                PrefabRegistry.Instance.DestroyComponentIfExists<EggGrow>(eggName, egg);
            }

            if (data.Components.Floating == ComponentBehavior.Patch)
            {
                var eggFloating = PrefabRegistry.Instance.GetOrAddComponent<Floating>(eggName, egg);

                Plugin.LogServerDebug($"{model}.{nameof(data.Floating)}: Setting values");

                eggFloating.m_waterLevelOffset = data.Floating.WaterLevelOffset;
            }
            else if (data.Components.Floating == ComponentBehavior.Remove)
            {
                PrefabRegistry.Instance.DestroyComponentIfExists<Floating>(eggName, egg);
            }

            StaticContext.EggDataContext.RegisterEggSharedName(egg);
        }

        private void SaveIcon(UnityEngine.Sprite sprite, string name)
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
                    Utils.SpriteUtils.ExportSpriteToPng(sprite, pngFile);
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

        private readonly Dictionary<string, UnityEngine.Sprite[]> originalIcons = new Dictionary<string, UnityEngine.Sprite[]>();

        public override void RestorePrefab(string eggName)
        {
            if (originalIcons.TryGetValue(eggName, out UnityEngine.Sprite[] icons))
            {
                // for some reasons we need to set the original icon back before restoring the prefab
                // this seems unneccessary but I tell you it is!
                // otherwiese the icons could look distorted if we rejoin the world
                var itemDrop = PrefabRegistry.Instance.GetOriginalPrefab(eggName).GetComponent<ItemDrop>();
                foreach (var s in itemDrop.m_itemData.m_shared.m_icons)
                {
                    if (s.texture) UnityEngine.Object.DestroyImmediate(s.texture);
                    UnityEngine.Object.DestroyImmediate(s);
                }
                itemDrop.m_itemData.m_shared.m_icons = icons;
                // now the next time we (re)enter a world the icons of cloned prefabs still look nice <3
            }

            PrefabRegistry.Instance.RestorePrefab(eggName, (current, backup) => {
                PrefabUtils.RestoreComponent<ParticleSystemRenderer>(current, backup);
                PrefabUtils.RestoreChildRenderers(current, backup);
                PrefabUtils.RestoreChildLights(current, backup);
                PrefabUtils.RestoreChildParticleRenderers(current, backup);
                PrefabUtils.RestoreChildLightFlickerBaseColor(current, backup);
                // we dont need to use this anymore (or do we? it doesnt seem to be the case)
                //RestoreHelper.RestoreItemIcons(current, backup);
            });

            if (PrefabRegistry.IsCustomPrefab(eggName))
            {
                ItemManager.Instance.RemoveItem(eggName);
            }
        }

        //------------------------------------------------
        // CLEANUP
        //------------------------------------------------

        public override void CleanupProcess()
        {
            originalIcons.Clear();
        }

    }

}
