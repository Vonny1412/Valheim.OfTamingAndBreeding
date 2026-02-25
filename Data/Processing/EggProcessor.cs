using Jotunn.Managers;
using OfTamingAndBreeding.Data.Processing.Base;
using OfTamingAndBreeding.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OfTamingAndBreeding.Data.Processing
{

    internal class EggProcessor : Base.DataProcessor<Models.Egg>
    {

        public override string DirectoryName => Models.Egg.DirectoryName;

        public override string PrefabTypeName => "item";

        public override string GetDataKey(string filePath) => null;

        //------------------------------------------------
        // PREPARE
        //------------------------------------------------

        public override void Prepare(Base.PrefabRegistry reg)
        {

        }

        //------------------------------------------------
        // VALIDATE DATA
        //------------------------------------------------

        public override bool ValidateData(Base.PrefabRegistry reg, string eggName, Models.Egg data)
        {
            var model = $"{nameof(Models.Egg)}.{eggName}";
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
            }

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

        public override bool ReservePrefab(Base.PrefabRegistry reg, string eggName, Models.Egg data)
        {
            var model = $"{nameof(Models.Egg)}.{eggName}";

            var egg = reg.GetReservedPrefab(eggName);
            if (egg == null)
            {

                var custom = reg.GetCustomPrefab(eggName);
                egg = reg.GetOriginalPrefab(eggName);
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

                    if (reg.IsCustomPrefab(data.Clone.From))
                    {
                        Plugin.LogError($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.From)}: Cannot clone from cloned prefab '{data.Clone.From}'");
                        return false;
                    }

                    var cloneFrom = reg.GetOriginalPrefab(data.Clone.From);
                    if (!cloneFrom)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.From)}: Prefab '{data.Clone.From}' not found");
                        return false;
                    }

                    if (custom == null)
                    {
                        // not cloned yet

                        var backup = reg.MakeCustomBackup(cloneFrom);
                        egg = reg.CreateClonedItemPrefab(eggName, cloneFrom.name);

                        //ctx.SetCustomPrefabUsingBackup(eggName, backup);
                    }
                    else
                    {
                        // previously cloned - reactivate
                        // hint: egg==custom

                        Plugin.LogDebug($"{model}.{{nameof(data.Clone): Reactivating cloned prefab for '{cloneFrom.name}'");
                        var backup = reg.GetUnusedCustomPrefabBackup(cloneFrom);
                        RestorePrefabFromBackup(backup, egg);

                        reg.SetCustomPrefabUsingBackup(eggName, backup);
                    }

                    ItemManager.Instance.AddItem(new Jotunn.Entities.CustomItem(egg, fixReference: false));
                    PrepareClone(reg, eggName, data, egg);
                }
                else
                {
                    reg.MakeOriginalBackup(egg);
                }

                reg.ReservePrefab(eggName, egg);
            }

            return true;
        }

        private void PrepareClone(Base.PrefabRegistry reg, string eggName, Models.Egg data, UnityEngine.GameObject egg)
        {
            var model = $"{nameof(Models.Egg)}.{eggName}";

            var eggItemDrop = egg.GetComponent<ItemDrop>(); // required (checked in ValidatePrefab)
            Plugin.LogDebug($"{model}.{nameof(data.Clone)}: Setting ItemDrop.ItemData values");

            var eggItemData = eggItemDrop.m_itemData;
            var eggItemDataShared = eggItemData.m_shared;

            if (data.Clone.Name != null) eggItemDataShared.m_name = data.Clone.Name;
            if (data.Clone.Description != null) eggItemDataShared.m_description = data.Clone.Description;
            if (data.Clone.Scale != null) Runtime.ItemData.SetCustomScale(eggName, (float)data.Clone.Scale);
        }

        //------------------------------------------------
        // VALIDATE PREFAB
        //------------------------------------------------

        public override bool ValidatePrefab(Base.PrefabRegistry reg, string eggName, Models.Egg data)
        {
            var model = $"{nameof(Models.Egg)}.{eggName}";
            var error = false;

            var egg = reg.GetReservedPrefab(eggName);
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
                        if (!reg.PrefabExists(grownData.Prefab))
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

        public override void RegisterPrefab(Base.PrefabRegistry reg, string eggName, Models.Egg data)
        {
            var model = $"{nameof(Models.Egg)}.{eggName}";

            var egg = reg.GetReservedPrefab(eggName);
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
                var eggEggGrow = reg.GetOrAddComponent<EggGrow>(eggName, egg);

                if (data.EggGrow != null)
                {
                    Plugin.LogDebug($"{model}.{nameof(data.EggGrow)}: Setting values");

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
                        m_effectPrefabs = Helpers.EffectsHelper.CreateEffectList(new string[] {
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
                reg.DestroyComponentIfExists<EggGrow>(eggName, egg);
            }

            if (data.Components.Floating == Models.SubData.ComponentBehavior.Patch)
            {
                var eggFloating = reg.GetOrAddComponent<Floating>(eggName, egg);

                Plugin.LogDebug($"{model}.{nameof(data.Floating)}: Setting values");

                eggFloating.m_waterLevelOffset = data.Floating.WaterLevelOffset;
            }
            else if (data.Components.Floating == Models.SubData.ComponentBehavior.Remove)
            {
                reg.DestroyComponentIfExists<Floating>(eggName, egg);
            }

            Data.Runtime.ItemData.RegisterEggBySharedName(egg);
        }

        //------------------------------------------------
        // CLEANUP
        //------------------------------------------------

        public override void Finalize(Base.PrefabRegistry reg)
        {

        }

        //------------------------------------------------
        // UNREGISTER PREFAB
        //------------------------------------------------

        private readonly Dictionary<string, UnityEngine.Sprite[]> originalIcons = new Dictionary<string, UnityEngine.Sprite[]>();

        public override void RestorePrefab(Base.PrefabRegistry reg, string eggName)
        {
            if (originalIcons.TryGetValue(eggName, out UnityEngine.Sprite[] icons))
            {
                // for some reasons we need to set the original icon back before restoring the prefab
                // this seems unneccessary but I tell you it is!
                // otherwiese the icons could look distorted if we rejoin the world
                var itemDrop = reg.GetOriginalPrefab(eggName).GetComponent<ItemDrop>();
                foreach (var s in itemDrop.m_itemData.m_shared.m_icons)
                {
                    if (s.texture) UnityEngine.Object.DestroyImmediate(s.texture);
                    UnityEngine.Object.DestroyImmediate(s);
                }
                itemDrop.m_itemData.m_shared.m_icons = icons;
                // now the next time we (re)enter a world the icons of cloned prefabs still look nice <3
            }

            reg.Restore(eggName, RestorePrefabFromBackup);
            if (reg.IsCustomPrefab(eggName))
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

        public override void Cleanup(Base.PrefabRegistry reg)
        {
            originalIcons.Clear();
        }

    }

}
