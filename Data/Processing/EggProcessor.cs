using Jotunn.Managers;
using OfTamingAndBreeding.Data.Models;
using OfTamingAndBreeding.Helpers;
using System;
using System.Collections.Generic;
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
        // PREPARE PREFAB
        //------------------------------------------------

        public override bool PreparePrefab(Base.DataProcessorContext ctx, string eggName, Models.Egg data)
        {
            var model = $"{nameof(Models.Egg)}.{eggName}";

            //var egg = PrefabManager.Instance.GetPrefab(eggName);
            var egg = ctx.GetPrefab(eggName);
            if (egg == null)
            {
                if (data.Clone == null)
                {
                    Plugin.LogError($"{model}.{nameof(data.Clone)} missing");
                    return false;
                }
                if (data.Clone.From == null)
                {
                    Plugin.LogError($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.From)} missing");
                    return false;
                }

                var cloneFrom = ctx.GetPrefab(data.Clone.From);
                if (!cloneFrom)
                {
                    Plugin.LogError($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.From)} '{data.Clone.From}' not found");
                    return false;
                }

                Plugin.LogDebug($"{model}: Cloning prefab from '{cloneFrom.name}'");
                ctx.MakeBackup(eggName, cloneFrom);
                egg = ctx.CreateClonedItemPrefab(eggName, cloneFrom.name);
            }
            else
            {
                ctx.MakeBackup(eggName, egg);
            }

            ctx.CachePrefab(eggName, egg);
            return true;
        }

        //------------------------------------------------
        // VALIDATE PREFAB
        //------------------------------------------------

        public override bool ValidatePrefab(Base.DataProcessorContext ctx, string eggName, Models.Egg data)
        {
            var model = $"{nameof(Models.Egg)}.{eggName}";
            var error = false;

            var egg = ctx.GetPrefab(eggName);
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

            var egg = ctx.GetPrefab(eggName);
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


                    GameObject eggItemPrefab = ObjectDB.instance.GetItemPrefab(eggName);
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
                    if (TintHelper.TryParseTint(data.Item.ItemTintRgb, out Color itemTint))
                    {
                        TintHelper.TintPrefab(eggItemPrefab, itemTint, true);

                        Plugin.LogDebug($"{model}: Tinting icon");
                        var itemDrop = eggItemPrefab.GetComponent<ItemDrop>();
                        var baseIcon = itemDrop.m_itemData.m_shared.m_icons?.FirstOrDefault();
                        if (baseIcon != null)
                        {
                            Plugin.LogMessage($"{baseIcon.name}");
                            originalIcons[eggName] = baseIcon;

                            var tinted = Helpers.TintHelper.CreateTintedSprite(baseIcon, itemTint);
                            itemDrop.m_itemData.m_shared.m_icons = new[] { tinted };
                            //itemDrop.m_itemData.m_shared.m_icons = new[] { baseIcon }; // this was just for debugging
                        }
                        else
                        {
                            // just ignore
                        }
                    }
                    if (TintHelper.TryParseTint(data.Item.LightsTintRgb, out Color lightsTint))
                    {
                        TintHelper.TintLights(eggItemPrefab, lightsTint, true);
                    }
                    if (TintHelper.TryParseTint(data.Item.ParticlesTintRgb, out Color particlesTint))
                    {
                        TintHelper.TintParticleSystems(eggItemPrefab, particlesTint, true);
                    }
                    if (data.Item.DisableParticles)
                    {
                        foreach (var r in egg.GetComponentsInChildren<ParticleSystemRenderer>(true))
                        {
                            UnityEngine.Object.DestroyImmediate(r);
                            //r.enabled = false;
                        }
                    }

                    // scale lights
                    var lightsScale = data.Item.LightsScale;
                    foreach (var l in eggItemPrefab.GetComponentsInChildren<Light>(true))
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

        private Dictionary<string, Sprite> originalIcons = new Dictionary<string, Sprite>();

        public override void RestorePrefab(Base.DataProcessorContext ctx, string eggName, Models.Egg data)
        {
            if (originalIcons.TryGetValue(eggName, out Sprite originalIcon))
            {
                // for some reasons we need to set the original icon back before restoring the prefab
                // this seems unneccessary but I tell you it is!
                // otherwiese the icons could look distorted if we rejoin the world
                Plugin.LogMessage($"{originalIcon.name}");
                var itemDrop = ctx.GetPrefab(eggName).GetComponent<ItemDrop>();
                itemDrop.m_itemData.m_shared.m_icons = new[] { originalIcon };
                // now the next time we (re)enter a world the icons of cloned prefabs still look nice <3
            }

            ctx.Restore(eggName, (GameObject backup, GameObject current) => {

                RestoreHelper.RestoreComponent<ItemDrop>(backup, current);
                RestoreHelper.RestoreComponent<EggGrow>(backup, current);
                RestoreHelper.RestoreComponent<ParticleSystemRenderer>(backup, current);

                RestoreHelper.RestoreChildRenderers(backup, current);
                RestoreHelper.RestoreChildLights(backup, current);
                RestoreHelper.RestoreChildParticleRenderers(backup, current);
                RestoreHelper.RestoreChildLightFlickerBaseColor(backup, current);
                
                // we dont need to use this naymore (or do we? it doesnt seem to be the case)
                //RestoreHelper.RestoreItemIcons(backup, current);
            });
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
