using Jotunn.Entities;
using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using OfTamingAndBreeding.Helpers;
using OfTamingAndBreeding.Data.Models;

namespace OfTamingAndBreeding.Data.Handling
{

    internal class EggModelHandler : ModelHandler<Egg>
    {

        public override string DirectoryName => Egg.DirectoryName;

        //------------------------------------------------
        // VALIDATE DATA
        //------------------------------------------------

        public override bool ValidateData(ModelHandlerContext ctx, string eggName, Egg data)
        {
            var model = $"{nameof(Egg)}.{eggName}";
            var error = false;

            if (data.Item == null)
            {
                Plugin.LogError($"{model}.{nameof(data.Item)} missing");
                error = true;
            }

            if (data.EggGrow == null)
            {
                Plugin.LogError($"{model}.{nameof(data.EggGrow)} missing");
                error = true;
            }

            if (data.EggGrow.Grown == null || data.EggGrow.Grown.Length == 0)
            {
                Plugin.LogError($"{model}.{nameof(data.EggGrow)}.{nameof(data.EggGrow.Grown)} is null or empty");
                error = true;
            }

            foreach (var (grownData, i) in data.EggGrow.Grown.Select((value, i) => (value, i)))
            {
                grownData.Weight = Math.Max(0f, grownData.Weight);
                if (grownData.Prefab == null)
                {
                    Plugin.LogError($"{model}.{nameof(data.EggGrow)}.{nameof(data.EggGrow.Grown)}.{i}.{nameof(grownData.Prefab)} is empty");
                    error = true;
                }
            }

            return error == false;
        }

        //------------------------------------------------
        // PREPARE PREFAB
        //------------------------------------------------

        public override bool PreparePrefab(ModelHandlerContext ctx, string eggName, Egg data)
        {
            var model = $"{nameof(Egg)}.{eggName}";

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
                //var cloneFrom = PrefabManager.Instance.GetPrefab(data.Clone.from);
                if (!cloneFrom)
                {
                    Plugin.LogError($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.From)} '{data.Clone.From}' not found");
                    return false;
                }

                Plugin.LogDebug($"{model}: Cloning prefab from '{cloneFrom.name}'");
                CustomItem eggCustom = new CustomItem(eggName, cloneFrom.name);
                ItemManager.Instance.AddItem(eggCustom);
                egg = eggCustom.ItemPrefab;
            }

            ctx.CachePrefab(eggName, egg);
            return true;
        }

        //------------------------------------------------
        // VALIDATE PREFAB
        //------------------------------------------------

        public override bool ValidatePrefab(ModelHandlerContext ctx, string eggName, Egg data)
        {
            var model = $"{nameof(Egg)}.{eggName}";
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
                    Plugin.LogFatal($"{model}.{nameof(data.EggGrow)}.{nameof(data.EggGrow.Grown)}.{i}.{nameof(grownData.Prefab)} '{grownData.Prefab}' not found");
                    error = true;
                }
            }

            return error == false;
        }

        //------------------------------------------------
        // REGISTER PREFAB
        //------------------------------------------------

        public override void RegisterPrefab(ModelHandlerContext ctx, string eggName, Egg data)
        {
            var model = $"{nameof(Egg)}.{eggName}";

            var egg = PrefabManager.Instance.GetPrefab(eggName);
            PrefabManager.Instance.RegisterToZNetScene(egg);

            var eggItemDrop = egg.GetComponent<ItemDrop>();
            var eggEggGrow = egg.GetComponent<EggGrow>();

            if (!eggEggGrow)
            {
                Plugin.LogDebug($"{model}: Adding EggGrow component");
                eggEggGrow = egg.AddComponent<EggGrow>();
            }

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

            var eggItemData = eggItemDrop.m_itemData;
            var eggItemDataShared = eggItemData.m_shared;

            eggItemDataShared.m_name = data.Item.Name;
            if (eggItemDataShared.m_name == null)
            {
                eggItemDataShared.m_name = "Egg";
            }
            Plugin.LogDebug($"{model}: Item name: {eggItemDataShared.m_name}");

            eggItemDataShared.m_description = data.Item.Description;
            if (eggItemDataShared.m_description == null)
            {
                eggItemDataShared.m_description = "It's an egg";
            }
            Plugin.LogDebug($"{model}: Item description: {eggItemDataShared.m_description}");

            eggItemDataShared.m_weight = data.Item.Weight;
            eggItemDataShared.m_scaleByQuality = data.Item.ScaleByQuality;
            eggItemDataShared.m_scaleWeightByQuality = data.Item.ScaleWeightByQuality;
            eggItemDataShared.m_value = data.Item.Value;
            eggItemDataShared.m_teleportable = data.Item.Teleportable;
            eggItemDataShared.m_maxStackSize = data.Item.MaxStackSize;

            // defaults
            eggItemDataShared.m_itemType = ItemDrop.ItemData.ItemType.Misc;
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
            if (TryParseTint(data.Item.ItemTintRgb, out Color itemTint))
            {
                TintPrefab(eggItemPrefab, itemTint, true);
            }
            if (TryParseTint(data.Item.LightsTintRgb, out Color lightsTint))
            {
                TintLights(eggItemPrefab, lightsTint, true);
            }
            if (TryParseTint(data.Item.ParticlesTintRgb, out Color particlesTint))
            {
                TintParticleSystems(eggItemPrefab, particlesTint, true);
            }
            ScaleLights(eggItemPrefab, data.Item.LightsScale);

            if (TryParseTint(data.Item.ItemTintRgb, out Color iconTint))
            {
                Plugin.LogDebug($"{model}: Tinting icon");
                var itemDrop = eggItemPrefab.GetComponent<ItemDrop>();
                var baseIcon = itemDrop.m_itemData.m_shared.m_icons?.FirstOrDefault();
                if (baseIcon != null)
                {
                    var tinted = IconTint.CreateTintedSprite(baseIcon, iconTint);
                    itemDrop.m_itemData.m_shared.m_icons = new[] { tinted };
                }
                else
                {
                    // fallback (falls kein icon vorhanden)
                }
            }

            Plugin.LogDebug($"{model}: Setting effects");
            if (eggEggGrow.m_hatchEffect == null || eggEggGrow.m_hatchEffect.m_effectPrefabs.Length == 0)
            {
                eggEggGrow.m_hatchEffect = new EffectList
                {
                    m_effectPrefabs = PrefabHelper.GetEffects(new string[] {
                        "fx_chicken_birth",
                    })
                };
            }
            if (data.Item.DisableParticles)
            {
                foreach (var r in egg.GetComponentsInChildren<ParticleSystemRenderer>(true))
                {
                    r.enabled = false;
                }
            }

            Plugin.LogDebug($"{model}.{nameof(data.EggGrow)}: Setting EggGrow values");
            eggEggGrow.m_growTime = data.EggGrow.GrowTime;
            eggEggGrow.m_updateInterval = data.EggGrow.UpdateInterval;
            eggEggGrow.m_requireNearbyFire = data.EggGrow.RequireNearbyFire;
            eggEggGrow.m_requireUnderRoof = data.EggGrow.RequireUnderRoof;
            eggEggGrow.m_requireCoverPercentige = data.EggGrow.RequireCoverPercentige;

            // will be set seperatly
            eggEggGrow.m_tamed = true;
            eggEggGrow.m_grownPrefab = null;

        }

        //------------------------------------------------
        // HELPERS
        //------------------------------------------------

        static bool TryParseTint(int[] src, out Color color)
        {
            color = default;

            if (src == null || (src.Length != 3 && src.Length != 4))
                return false;

            color = new Color(
                Mathf.Clamp01(src[0] / 255f),
                Mathf.Clamp01(src[1] / 255f),
                Mathf.Clamp01(src[2] / 255f),
                src.Length == 4 ? Mathf.Clamp01(src[3] / 255f) : 1f
            );

            return true;
        }

        static void TintPrefab(GameObject prefab, Color tint, bool multiply)
        {
            if (!prefab) return;

            foreach (var r in prefab.GetComponentsInChildren<Renderer>(true))
            {
                var mats = r.sharedMaterials;
                if (mats == null) continue;

                for (int i = 0; i < mats.Length; i++)
                {
                    if (!mats[i]) continue;
                    var m2 = UnityEngine.Object.Instantiate(mats[i]);
                    ApplyTintToMaterial(m2, tint, multiply);
                    mats[i] = m2;
                }

                r.sharedMaterials = mats;
            }
        }

        static Color Multiply(Color baseCol, Color tint)
        {
            // komponentenweise Multiplikation, Alpha typischerweise beibehalten
            return new Color(baseCol.r * tint.r, baseCol.g * tint.g, baseCol.b * tint.b, baseCol.a);
        }

        static ParticleSystem.MinMaxGradient Multiply(ParticleSystem.MinMaxGradient g, Color tint)
        {
            // Es gibt 4 Modi: Color, TwoColors, Gradient, TwoGradients
            switch (g.mode)
            {
                case ParticleSystemGradientMode.Color:
                    g.color = Multiply(g.color, tint);
                    break;

                case ParticleSystemGradientMode.TwoColors:
                    g.colorMin = Multiply(g.colorMin, tint);
                    g.colorMax = Multiply(g.colorMax, tint);
                    break;

                case ParticleSystemGradientMode.Gradient:
                    g.gradient = Multiply(g.gradient, tint);
                    break;

                case ParticleSystemGradientMode.TwoGradients:
                    g.gradientMin = Multiply(g.gradientMin, tint);
                    g.gradientMax = Multiply(g.gradientMax, tint);
                    break;
            }
            return g;
        }

        static Gradient Multiply(Gradient grad, Color tint)
        {
            var outG = new Gradient();
            var cks = grad.colorKeys;
            for (int i = 0; i < cks.Length; ++i)
                cks[i].color = Multiply(cks[i].color, tint);

            outG.SetKeys(cks, grad.alphaKeys); // AlphaKeys unverändert
            return outG;
        }

        static void TintParticleSystems(GameObject go, Color tint, bool multiply)
        {
            foreach (var ps in go.GetComponentsInChildren<ParticleSystem>(true))
            {
                var main = ps.main;
                if (multiply)
                    main.startColor = Multiply(main.startColor, tint);
                else
                    main.startColor = new ParticleSystem.MinMaxGradient(tint);
                var col = ps.colorOverLifetime;
                if (col.enabled)
                {
                    if (multiply) col.color = Multiply(col.color, tint);
                    else col.color = new ParticleSystem.MinMaxGradient(tint);
                }
                var bySpeed = ps.colorBySpeed;
                if (bySpeed.enabled)
                {
                    if (multiply) bySpeed.color = Multiply(bySpeed.color, tint);
                    else bySpeed.color = new ParticleSystem.MinMaxGradient(tint);
                }
            }
            // optional: Material-tinting on ParticleSystemRenderern
            foreach (var r in go.GetComponentsInChildren<ParticleSystemRenderer>(true))
            {
                var mat = r.sharedMaterial;
                if (!mat) continue;

                ApplyTintToMaterial(mat, tint, multiply);
            }
        }

        static void TintLights(GameObject go, Color tint, bool multiply)
        {
            foreach (var l in go.GetComponentsInChildren<Light>(true))
            {
                l.color = multiply ? Multiply(l.color, tint) : tint;
                // keep intensity
            }
            foreach (var mb in go.GetComponentsInChildren<MonoBehaviour>(true))
            {
                var t = mb.GetType();
                if (t.Name != "LightFlicker") continue;

                var baseColorField = t.GetField("m_baseColor",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);

                if (baseColorField != null && baseColorField.FieldType == typeof(Color))
                {
                    var baseCol = (Color)baseColorField.GetValue(mb);
                    baseColorField.SetValue(mb, multiply ? Multiply(baseCol, tint) : tint);
                }
            }
        }

        static void ScaleLights(GameObject prefabOrInstance, float scale)
        {
            foreach (var l in prefabOrInstance.GetComponentsInChildren<Light>(true))
            {
                if (scale <= 0f)
                {
                    l.enabled = false;
                    l.range = 0;
                    continue;
                }

                l.enabled = true;
                l.range *= scale;
            }
        }

        static void ApplyTintToMaterial(Material mat, Color tint, bool multiply)
        {
            if (mat.HasProperty("_Color"))
            {
                var baseCol = mat.GetColor("_Color");
                mat.SetColor("_Color", multiply ? Multiply(baseCol, tint) : tint);
            }
            if (mat.HasProperty("_TintColor"))
            {
                var baseCol = mat.GetColor("_TintColor");
                mat.SetColor("_TintColor", multiply ? Multiply(baseCol, tint) : tint);
            }
            // if (mat.HasProperty("_EmissionColor")) ...
        }

    }

}
