using Jotunn.Entities;
using Jotunn.Managers;
using OfTamingAndBreeding.Data.Handling.Base;
using OfTamingAndBreeding.Data.Models;
using OfTamingAndBreeding.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OfTamingAndBreeding.Data.Handling
{

    internal class EggHandler : DataHandler<Models.Egg>
    {

        public override string DirectoryName => Models.Egg.DirectoryName;

        public override string GetDataKey(string filePath) => null;


        //------------------------------------------------
        // PREPARE
        //------------------------------------------------

        public override void Prepare(DataHandlerContext ctx)
        {

        }

        //------------------------------------------------
        // VALIDATE DATA
        //------------------------------------------------

        public override bool ValidateData(DataHandlerContext ctx, string eggName, Models.Egg data)
        {
            var model = $"{nameof(Models.Egg)}.{eggName}";
            var error = false;

            if (data.Item == null)
            {
                //Plugin.LogError($"{model}.{nameof(data.Item)} missing");
                //error = true;
            }

            if (data.EggGrow == null)
            {
                //Plugin.LogError($"{model}.{nameof(data.EggGrow)} missing");
                //error = true;
            }
            else
            {

                if (data.EggGrow.Grown == null || data.EggGrow.Grown.Length == 0)
                {
                    //Plugin.LogError($"{model}.{nameof(data.EggGrow)}.{nameof(data.EggGrow.Grown)} is null or empty");
                    //error = true;
                }
                else
                {

                    foreach (var (grownData, i) in data.EggGrow.Grown.Select((value, i) => (value, i)))
                    {
                        grownData.Weight = Math.Max(0f, grownData.Weight);
                        if (grownData.Prefab == null)
                        {
                            Plugin.LogError($"{model}.{nameof(data.EggGrow)}.{nameof(data.EggGrow.Grown)}.{i}.{nameof(grownData.Prefab)} is empty");
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

        public override bool PreparePrefab(DataHandlerContext ctx, string eggName, Models.Egg data)
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

        public override bool ValidatePrefab(DataHandlerContext ctx, string eggName, Models.Egg data)
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
                    Plugin.LogFatal($"{model}: Prefab has no ItemDrop");
                    error = true;
                }
            }

            foreach (var (grownData, i) in data.EggGrow.Grown.Select((value, i) => (value, i)))
            {
                if (!ctx.PrefabExists(grownData.Prefab))
                {
                    Plugin.LogFatal($"{model}.{nameof(data.EggGrow)}.{nameof(data.EggGrow.Grown)}.{i}.{nameof(grownData.Prefab)}: '{grownData.Prefab}' not found");
                    error = true;
                }
            }

            return error == false;
        }

        //------------------------------------------------
        // REGISTER PREFAB
        //------------------------------------------------

        public override void RegisterPrefab(DataHandlerContext ctx, string eggName, Models.Egg data)
        {
            var model = $"{nameof(Models.Egg)}.{eggName}";

            var egg = PrefabManager.Instance.GetPrefab(eggName);
            ItemManager.Instance.RegisterItemInObjectDB(egg);

            var eggItemDrop = egg.GetComponent<ItemDrop>(); // required (checked in ValidatePrefab)
            var eggEggGrow = ctx.GetOrAddComponent<EggGrow>(eggName, egg);

            /*
            Plugin.Log.LogMessage($"{eggName}");
            foreach (var c in egg.GetComponents<Component>())
            {
                Plugin.Log.LogInfo($" Component: {c.GetType().FullName}");
            }
            */

            //
            // set values
            //

            if (data.Item != null) {

                Plugin.LogDebug($"{model}.{nameof(data.Item)}: Setting values");

                var eggItemData = eggItemDrop.m_itemData;
                var eggItemDataShared = eggItemData.m_shared;

                if (data.Item.Name != null)                 eggItemDataShared.m_name = data.Item.Name;
                if (data.Item.Description != null)          eggItemDataShared.m_description = data.Item.Description;

                if (data.Item.Weight != null)               eggItemDataShared.m_weight = (float)data.Item.Weight;
                if (data.Item.ScaleByQuality != null)       eggItemDataShared.m_scaleByQuality = (float)data.Item.ScaleByQuality;
                if (data.Item.Value != null)                eggItemDataShared.m_value = (int)data.Item.Value;

                if (data.Item.Teleportable != null)         eggItemDataShared.m_teleportable = (bool)data.Item.Teleportable;
                if (data.Item.MaxStackSize != null)         eggItemDataShared.m_maxStackSize = (int)data.Item.MaxStackSize;
                if (data.Item.MaxQuality != null)           eggItemDataShared.m_maxQuality = (int)data.Item.MaxQuality;

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

                string prefabName = egg.gameObject.name;
                GameObject eggItemPrefab = ObjectDB.instance.GetItemPrefab(prefabName);
                if (eggItemPrefab == null)
                {
                    // not found? maybe its custom item
                    var customItem = ItemManager.Instance.GetItem(prefabName);
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
                        var tinted = Helpers.TintHelper.CreateTintedSprite(baseIcon, itemTint);
                        itemDrop.m_itemData.m_shared.m_icons = new[] { tinted };
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
                        r.enabled = false;
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
            else
            {
                Plugin.LogDebug($"{model}.{nameof(data.Item)}: Skipped");
            }

            if (data.EggGrow != null)
            {
                Plugin.LogDebug($"{model}.{nameof(data.EggGrow)}: Setting values");
                eggEggGrow.m_growTime = data.EggGrow.GrowTime;
                eggEggGrow.m_updateInterval = data.EggGrow.UpdateInterval;
                eggEggGrow.m_requireNearbyFire = data.EggGrow.RequireNearbyFire;
                eggEggGrow.m_requireUnderRoof = data.EggGrow.RequireUnderRoof;
                eggEggGrow.m_requireCoverPercentige = data.EggGrow.RequireCoverPercentige;
            }
            else
            {
                Plugin.LogDebug($"{model}.{nameof(data.EggGrow)}: Skipped");
            }

            Plugin.LogDebug($"{model}: Setting effects");
            if (eggEggGrow.m_hatchEffect == null || eggEggGrow.m_hatchEffect.m_effectPrefabs.Length == 0)
            {
                eggEggGrow.m_hatchEffect = new EffectList
                {
                    m_effectPrefabs = Helpers.PrefabHelper.GetEffects(new string[] {
                        "fx_chicken_birth",
                    })
                };
            }

            // will be set seperatly
            eggEggGrow.m_tamed = true;
            eggEggGrow.m_grownPrefab = null;

        }

        //------------------------------------------------
        // CLEANUP
        //------------------------------------------------

        public override void Cleanup(DataHandlerContext ctx)
        {

        }

        //------------------------------------------------
        // UNREGISTER PREFAB
        //------------------------------------------------

        public override void RestorePrefab(DataHandlerContext ctx, string eggName, Models.Egg data)
        {
            ctx.Restore(eggName, (GameObject backup, GameObject current) => {

                RestoreHelper.RestoreComponent<ItemDrop>(backup, current);
                RestoreHelper.RestoreComponent<EggGrow>(backup, current);
                RestoreHelper.RestoreComponent<ParticleSystemRenderer>(backup, current);

                RestoreHelper.RestoreChildRenderers(backup, current);
                RestoreHelper.RestoreChildLights(backup, current);
                RestoreHelper.RestoreChildParticleRenderers(backup, current);
                RestoreHelper.RestoreChildLightFlickerBaseColor(backup, current);
                RestoreHelper.RestoreItemIcons(backup, current);

            });
        }

    }

}
