using Jotunn.Managers;
using OfTamingAndBreeding.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace OfTamingAndBreeding.Data.Processing
{

    internal class EggProcessor : Base.DataProcessor<Models.Egg>
    {

        public override string DirectoryName => Models.Egg.DirectoryName;

        public override string GetDataKey(string filePath) => null;


        //------------------------------------------------
        // PREPARE
        //------------------------------------------------

        public override void Prepare(Base.DataProcessorContext ctx)
        {

        }

        //------------------------------------------------
        // VALIDATE DATA
        //------------------------------------------------

        public override bool ValidateData(Base.DataProcessorContext ctx, string eggName, Models.Egg data)
        {
            var model = $"{nameof(Models.Egg)}.{eggName}";
            var error = false;

            switch (data.Components.Item)
            {
                case Models.SubData.ComponentBehavior.Remove:
                    Plugin.LogWarning($"{model}.{nameof(data.Components)}.{nameof(data.Components.Item)}({nameof(Models.SubData.ComponentBehavior.Remove)}): Component cannot be removed");
                    break;
                case Models.SubData.ComponentBehavior.Patch:
                    if (data.Item == null)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Components)}.{nameof(data.Components.Item)}({nameof(Models.SubData.ComponentBehavior.Patch)}): Missing component data");
                        error = true;
                    }
                    break;
                case Models.SubData.ComponentBehavior.Inherit:
                    if (data.Item != null)
                    {
                        Plugin.LogWarning($"{model}.{nameof(data.Components)}.{nameof(data.Components.Item)}({nameof(Models.SubData.ComponentBehavior.Inherit)}): Component data will be ignored");
                    }
                    break;
            }

            switch (data.Components.EggGrow)
            {
                case Models.SubData.ComponentBehavior.Remove:
                    //Plugin.LogDebug($"{model}.{nameof(data.Components)}.{nameof(data.Components.EggGrow)}({nameof(Models.SubData.ComponentBehavior.Remove)}): Component cannot be removed");
                    break;
                case Models.SubData.ComponentBehavior.Patch:
                    if (data.EggGrow == null)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Components)}.{nameof(data.Components.EggGrow)}({nameof(Models.SubData.ComponentBehavior.Patch)}): Missing component data");
                        error = true;
                    }
                    break;
                case Models.SubData.ComponentBehavior.Inherit:
                    if (data.EggGrow != null)
                    {
                        Plugin.LogWarning($"{model}.{nameof(data.Components)}.{nameof(data.Components.EggGrow)}({nameof(Models.SubData.ComponentBehavior.Inherit)}): Component data will be ignored");
                    }
                    break;
            }

            switch (data.Components.Floating)
            {
                case Models.SubData.ComponentBehavior.Remove:
                    // can be removed
                    break;
                case Models.SubData.ComponentBehavior.Patch:
                    if (data.Floating == null)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Components)}.{nameof(data.Components.Floating)}({nameof(Models.SubData.ComponentBehavior.Patch)}): Missing component data");
                        error = true;
                    }
                    break;
                case Models.SubData.ComponentBehavior.Inherit:
                    if (data.Floating != null)
                    {
                        Plugin.LogWarning($"{model}.{nameof(data.Components)}.{nameof(data.Components.Floating)}({nameof(Models.SubData.ComponentBehavior.Inherit)}): Component data will be ignored");
                    }
                    break;
            }

            if (data.Item != null && data.Components.Item == Models.SubData.ComponentBehavior.Patch)
            {
                // nothing to validate here
            }

            if (data.EggGrow != null && data.Components.EggGrow == Models.SubData.ComponentBehavior.Patch)
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

        private readonly HashSet<string> reservedPrefabNames = new HashSet<string>();

        public override bool ReservePrefab(Base.DataProcessorContext ctx, string eggName, Models.Egg data)
        {
            var model = $"{nameof(Models.Egg)}.{eggName}";

            if (reservedPrefabNames.Contains(eggName))
            {
                Plugin.LogError($"{model}: Prefab already reserved");
                return false;
            }
            reservedPrefabNames.Add(eggName);

            var egg = ctx.GetReservedPrefab(eggName);
            if (egg == null)
            {

                var cloned = ctx.GetClonedPrefab(eggName);
                egg = ctx.GetOriginalPrefab(eggName);
                if (egg == null || cloned != null) // need clone (not cloned yet / previously cloned, reactivate)
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

                    if (ctx.IsCustomPrefab(data.Clone.From))
                    {
                        Plugin.LogError($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.From)}: Cannot clone from cloned prefab '{data.Clone.From}'");
                        return false;
                    }

                    var cloneFrom = ctx.GetOriginalPrefab(data.Clone.From);
                    if (!cloneFrom)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.From)}: Prefab '{data.Clone.From}' not found");
                        return false;
                    }

                    if (cloned == null)
                    {
                        // not cloned yet

                        var backup = ctx.MakeCloneBackup(cloneFrom);
                        egg = ctx.CreateClonedItemPrefab(eggName, cloneFrom.name);

                        ctx.SetCustomPrefabUsingBackup(eggName, backup);
                    }
                    else
                    {
                        // previously cloned - reactivate

                        Plugin.LogDebug($"{model}.{{nameof(data.Clone): Reactivating cloned prefab for '{cloneFrom.name}'");
                        var backup = ctx.GetUnusedClonedPrefabBackup(cloneFrom);
                        RestorePrefabFromBackup(backup, egg);

                        ctx.SetCustomPrefabUsingBackup(eggName, backup);
                    }

                    ItemManager.Instance.AddItem(new Jotunn.Entities.CustomItem(egg, fixReference: false));
                }
                else
                {
                    ctx.MakeOriginalBackup(egg);
                }

                ctx.ReservePrefab(eggName, egg);
            }

            return true;
        }

        //------------------------------------------------
        // VALIDATE PREFAB
        //------------------------------------------------

        public override bool ValidatePrefab(Base.DataProcessorContext ctx, string eggName, Models.Egg data)
        {
            var model = $"{nameof(Models.Egg)}.{eggName}";
            var error = false;

            var egg = ctx.GetOriginalPrefab(eggName);
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

            if (data.Components.EggGrow == Models.SubData.ComponentBehavior.Patch)
            {
                if (data.EggGrow.Grown != null)
                {
                    foreach (var (grownData, i) in data.EggGrow.Grown.Select((value, i) => (value, i)))
                    {
                        if (!ctx.PrefabExists(grownData.Prefab))
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

        public override void RegisterPrefab(Base.DataProcessorContext ctx, string eggName, Models.Egg data)
        {
            var model = $"{nameof(Models.Egg)}.{eggName}";

            var egg = ctx.GetOriginalPrefab(eggName);
            ItemManager.Instance.RegisterItemInObjectDB(egg);

            //
            // set values
            //

            if (data.Components.Item == Models.SubData.ComponentBehavior.Patch)
            {
                if (data.Item != null)
                {
                    var eggItemDrop = egg.GetComponent<ItemDrop>(); // required (checked in ValidatePrefab)
                    Plugin.LogDebug($"{model}.{nameof(data.Item)}: Setting values");

                    var eggItemData = eggItemDrop.m_itemData;
                    var eggItemDataShared = eggItemData.m_shared;

                    if (data.Item.Name != null) eggItemDataShared.m_name = data.Item.Name;
                    if (data.Item.Description != null) eggItemDataShared.m_description = data.Item.Description;

                    if (data.Item.Weight != null) eggItemDataShared.m_weight = (float)data.Item.Weight;
                    if (data.Item.Scale != null) Runtime.ItemData.SetCustomScale(eggName, (float)data.Item.Scale);
                    if (data.Item.ScaleByQuality != null) eggItemDataShared.m_scaleByQuality = (float)data.Item.ScaleByQuality;
                    if (data.Item.ScaleWeightByQuality != null) eggItemDataShared.m_scaleWeightByQuality = (float)data.Item.ScaleWeightByQuality;
                    if (data.Item.Value != null) eggItemDataShared.m_value = (int)data.Item.Value;

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
                    
                    // defaults
                    eggItemDataShared.m_autoStack = false;
                    eggItemDrop.m_autoPickup = false;

                    var eggItemPrefab = ObjectDB.instance.GetItemPrefab(eggName);
                    if (eggItemPrefab == null)
                    {
                        // not found? maybe its custom item
                        var customItem = ItemManager.Instance.GetItem(eggName);
                        if (customItem != null)
                        {
                            eggItemPrefab = customItem.ItemPrefab;
                        }
                    }

                    Plugin.LogDebug($"{model}: Tinting prefab, lights and particles");

                    // multiply = true by default to preserve texture contrast;
                    // kept as parameter for possible future advanced tint modes
                    if (TintHelper.TryParseTint(data.Item.ItemTintRgb, out UnityEngine.Color itemTint))
                    {
                        TintHelper.TintPrefab(eggItemPrefab, itemTint, true);

                        Plugin.LogDebug($"{model}: Tinting icon");
                        var itemDrop = eggItemPrefab.GetComponent<ItemDrop>();
                        var baseIcon = itemDrop.m_itemData.m_shared.m_icons?.FirstOrDefault();
                        if (baseIcon != null)
                        {
                            originalIcons[eggName] = itemDrop.m_itemData.m_shared.m_icons;

                            var tinted = Helpers.TintHelper.CreateTintedSprite(baseIcon, itemTint);
                            itemDrop.m_itemData.m_shared.m_icons = new[] { tinted };
                            //itemDrop.m_itemData.m_shared.m_icons = new[] { baseIcon }; // this was just for debugging
                        }
                        else
                        {
                            // just ignore
                        }
                    }
                    if (TintHelper.TryParseTint(data.Item.LightsTintRgb, out UnityEngine.Color lightsTint))
                    {
                        TintHelper.TintLights(eggItemPrefab, lightsTint, true);
                    }
                    if (TintHelper.TryParseTint(data.Item.ParticlesTintRgb, out UnityEngine.Color particlesTint))
                    {
                        TintHelper.TintParticleSystems(eggItemPrefab, particlesTint, true);
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
                    foreach (var l in eggItemPrefab.GetComponentsInChildren<UnityEngine.Light>(true))
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
            else if (data.Components.Item == Models.SubData.ComponentBehavior.Remove)
            {
                // ignore, cannot be removed
            }

            if (data.Components.EggGrow == Models.SubData.ComponentBehavior.Patch)
            {
                var eggEggGrow = ctx.GetOrAddComponent<EggGrow>(eggName, egg);

                if (data.EggGrow != null)
                {
                    Plugin.LogDebug($"{model}.{nameof(data.EggGrow)}: Setting values");

                    if (data.EggGrow.GrowTime != null) eggEggGrow.m_growTime = (float)data.EggGrow.GrowTime;
                    if (data.EggGrow.UpdateInterval != null) eggEggGrow.m_updateInterval = (float)data.EggGrow.UpdateInterval;
                    if (data.EggGrow.RequireNearbyFire != null) eggEggGrow.m_requireNearbyFire = (bool)data.EggGrow.RequireNearbyFire;
                    if (data.EggGrow.RequireUnderRoof != null) eggEggGrow.m_requireUnderRoof = (bool)data.EggGrow.RequireUnderRoof;
                    if (data.EggGrow.RequireCoverPercentige != null) eggEggGrow.m_requireCoverPercentige = (float)data.EggGrow.RequireCoverPercentige;

                    if (data.EggGrow.RequireAnyBiome != null) Runtime.EggGrow.SetEggNeedsAnyBiome(eggName, data.EggGrow.RequireAnyBiome);
                    if (data.EggGrow.RequireLiquid != null)
                    {
                        var lType = (EnvironmentHelper.LiquidTypeEx)data.EggGrow.RequireLiquid;
                        var lDepth = data.EggGrow.RequireLiquidDepth ?? 0;
                        Runtime.EggGrow.SetEggNeedsLiquid(eggName, lType, lDepth);
                    }

                }

                Plugin.LogDebug($"{model}: Setting effects");
                if (eggEggGrow.m_hatchEffect == null || eggEggGrow.m_hatchEffect.m_effectPrefabs.Length == 0)
                {
                    eggEggGrow.m_hatchEffect = new EffectList
                    {
                        m_effectPrefabs = Helpers.EffectsHelper.GetEffectList(new string[] {
                        "fx_chicken_birth",
                    })
                    };
                }

                // will be set seperatly
                eggEggGrow.m_tamed = true;
                eggEggGrow.m_grownPrefab = null;

            }
            else if (data.Components.EggGrow == Models.SubData.ComponentBehavior.Remove)
            {
                ctx.DestroyComponentIfExists<EggGrow>(eggName, egg);
            }

            if (data.Components.Floating == Models.SubData.ComponentBehavior.Patch)
            {
                var eggFloating = ctx.GetOrAddComponent<Floating>(eggName, egg);

                Plugin.LogDebug($"{model}.{nameof(data.Floating)}: Setting values");

                eggFloating.m_waterLevelOffset = data.Floating.WaterLevelOffset;
            }
            else if (data.Components.Floating == Models.SubData.ComponentBehavior.Remove)
            {
                ctx.DestroyComponentIfExists<Floating>(eggName, egg);
            }

            Data.Runtime.ItemData.RegisterEggBySharedName(egg);
        }

        //------------------------------------------------
        // CLEANUP
        //------------------------------------------------

        public override void Finalize(Base.DataProcessorContext ctx)
        {

        }

        //------------------------------------------------
        // UNREGISTER PREFAB
        //------------------------------------------------

        private readonly Dictionary<string, UnityEngine.Sprite[]> originalIcons = new Dictionary<string, UnityEngine.Sprite[]>();

        public override void RestorePrefab(Base.DataProcessorContext ctx, string eggName)
        {
            if (originalIcons.TryGetValue(eggName, out UnityEngine.Sprite[] icons))
            {
                // for some reasons we need to set the original icon back before restoring the prefab
                // this seems unneccessary but I tell you it is!
                // otherwiese the icons could look distorted if we rejoin the world
                var itemDrop = ctx.GetOriginalPrefab(eggName).GetComponent<ItemDrop>();
                foreach (var s in itemDrop.m_itemData.m_shared.m_icons)
                {
                    if (s.texture) UnityEngine.Object.DestroyImmediate(s.texture);
                    UnityEngine.Object.DestroyImmediate(s);
                }
                itemDrop.m_itemData.m_shared.m_icons = icons;
                // now the next time we (re)enter a world the icons of cloned prefabs still look nice <3
            }

            ctx.Restore(eggName, RestorePrefabFromBackup);
            if (ctx.IsCustomPrefab(eggName))
            {
                ItemManager.Instance.RemoveItem(eggName);
            }
        }

        private void RestorePrefabFromBackup(UnityEngine.GameObject backup, UnityEngine.GameObject current)
        {
            RestoreHelper.RestoreComponent<Floating>(backup, current);
            RestoreHelper.RestoreComponent<ItemDrop>(backup, current);
            RestoreHelper.RestoreComponent<EggGrow>(backup, current);
            RestoreHelper.RestoreComponent<UnityEngine.ParticleSystemRenderer>(backup, current);
            RestoreHelper.RestoreChildRenderers(backup, current);
            RestoreHelper.RestoreChildLights(backup, current);
            RestoreHelper.RestoreChildParticleRenderers(backup, current);
            RestoreHelper.RestoreChildLightFlickerBaseColor(backup, current);

            // we dont need to use this anymore (or do we? it doesnt seem to be the case)
            //RestoreHelper.RestoreItemIcons(backup, current);
        }

        //------------------------------------------------
        // CLEANUP
        //------------------------------------------------

        public override void Cleanup(Base.DataProcessorContext ctx)
        {
            originalIcons.Clear();
        }

    }

}
