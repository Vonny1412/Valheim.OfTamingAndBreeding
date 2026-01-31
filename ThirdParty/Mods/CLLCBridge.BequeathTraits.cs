using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OfTamingAndBreeding.ThirdParty.Mods
{
    internal static partial class CllCBridge
    {

        public static void PassTraits(ZDO fromZdo, GameObject to)
        {
            var infusion = fromZdo.GetInt(Plugin.ZDOVars.z_CLLC_Infusion, 0);
            var effect = fromZdo.GetInt(Plugin.ZDOVars.z_CLLC_Effect, 0);
            ApplyTraits(infusion, effect, to);
        }

        public static void BequeathTraits(Character directParentCharacter, GameObject currentPartnerPrefab, GameObject spawned)
        {
            // Safety / feature gating
            if (!IsRegistered || !IsEnabled())
                return;

            if (!spawned)
                return;

            // If no partner (self-breeding), treat parent as partner prefab
            if (!currentPartnerPrefab)
                currentPartnerPrefab = directParentCharacter ? directParentCharacter.gameObject : null;

            // Decide which traits should be inherited (as int enum values; 0 = None)
            int infusion = DecideInfusion(directParentCharacter, currentPartnerPrefab, spawned.transform.position);
            int effect = DecideExtraEffect(directParentCharacter, currentPartnerPrefab, spawned.transform.position);
            ApplyTraits(infusion, effect, spawned);
        }

        private static void ApplyTraits(int infusion, int effect, GameObject to)
        {
            var spawnedGrowup = to.GetComponent<Growup>();
            var spawnedItemDrop = to.GetComponent<ItemDrop>();
            var spawnedCharacter = to.GetComponent<Character>();
            if ((bool)spawnedGrowup || (bool)spawnedItemDrop)
            {
                if (Helpers.ZNetHelper.TryGetZDO(to, out ZDO zdo))
                {
                    if (infusion != 0) zdo.Set(Plugin.ZDOVars.z_CLLC_Infusion, infusion);
                    if (effect != 0) zdo.Set(Plugin.ZDOVars.z_CLLC_Effect, effect);
                }
            }
            else if ((bool)spawnedCharacter)
            {
                // adult spawned -> apply now
                if (infusion != 0)
                {
                    if (infusion > 0)
                    {
                        SetInfusion(spawnedCharacter, infusion);
                    }
                    else
                    {
                        // use a switch, maybe we gonna add more features
                        // if so, create an enum for additional features in Plugin.ZDOVars
                        switch (infusion)
                        {
                            case -1:
                                SetInfusionRandom(spawnedCharacter);
                                break;
                        }
                    }
                }
                if (effect != 0)
                {
                    if (effect > 0)
                    {
                        SetExtraEffect(spawnedCharacter, effect);
                    }
                    else
                    {
                        // use a switch, maybe we gonna add more features
                        // if so, create an enum for additional features in Plugin.ZDOVars
                        switch (effect)
                        {
                            case -1:
                                SetExtraEffectRandom(spawnedCharacter);
                                break;
                        }
                    }
                }
            }
        }

        // ----------------- helpers -------------------

        private sealed class WeightedTraitOption
        {
            public TraitPath Path;
            public float Weight;
            public int Value; // 0=None, >0=Trait, -1=RandomNew sentinel
        }

        private enum TraitPath
        {
            DirectParent,
            CurrentPartner,
            AnyNearby,
            RandomNew,
            None,
        }

        private static TraitPath PickWeightedOption(List<WeightedTraitOption> options)
        {
            float sum = 0f;
            for (int i = 0; i < options.Count; ++i)
                sum += Mathf.Max(0f, options[i].Weight);

            if (sum <= 0f)
                return TraitPath.None;

            float r = UnityEngine.Random.value * sum;
            float acc = 0f;

            for (int i = 0; i < options.Count; ++i)
            {
                acc += Mathf.Max(0f, options[i].Weight);
                if (r <= acc)
                    return options[i].Path;
            }

            return options[options.Count - 1].Path;
        }

        private static int DecideInfusion(Character directParent, GameObject partnerPrefab, Vector3 center)
        {
            if (!IsInfusionEnabled())
                return 0;

            float range = Plugin.Configs.CLLC_Infusion_SearchRange.Value;

            int directValue = 0;
            if (directParent)
                directValue = GetInfusion(directParent);

            int partnerValue = 0;
            {
                var ch = FindNearbyCharacterWithTrait(prefabMustMatch: partnerPrefab, center, range, exclude: directParent, traitGetter: GetInfusion);
                if (ch) partnerValue = GetInfusion(ch);
            }

            int anyValue = 0;
            {
                var ch = FindNearbyCharacterWithTrait(prefabMustMatch: null, center, range, exclude: directParent, traitGetter: GetInfusion);
                if (ch) anyValue = GetInfusion(ch);
            }

            // Options: Count weights only if Value != 0 (for 1–3)
            var opts = new List<WeightedTraitOption>(5)
            {
                new WeightedTraitOption
                {
                    Path = TraitPath.DirectParent,
                    Value = directValue,
                    Weight = directValue != 0 ? Plugin.Configs.CLLC_Infusion_WeightDirectParent.Value : 0f
                },
                new WeightedTraitOption
                {
                    Path = TraitPath.CurrentPartner,
                    Value = partnerValue,
                    Weight = partnerValue != 0 ? Plugin.Configs.CLLC_Infusion_WeightCurrentPartner.Value : 0f
                },
                new WeightedTraitOption
                {
                    Path = TraitPath.AnyNearby,
                    Value = anyValue,
                    Weight = anyValue != 0 ? Plugin.Configs.CLLC_Infusion_WeightAnyNearby.Value : 0f
                },
                new WeightedTraitOption
                {
                    Path = TraitPath.RandomNew,
                    Value = -1,
                    Weight = Plugin.Configs.CLLC_Infusion_WeightRandomNew.Value
                },
                new WeightedTraitOption
                {
                    Path = TraitPath.None,
                    Value = 0,
                    Weight = Plugin.Configs.CLLC_Infusion_WeightNone.Value
                }
            };

            foreach (var o in opts)
            {
                Plugin.LogDebug($"{o.Path}: value={o.Value}, weight={o.Weight}");
            }

            // If all weights are 0: fallback -> None
            // (e.g., if RandomNew and None in config are 0 and all traits were 0)
            bool anyWeight = false;
            for (int i = 0; i < opts.Count; ++i)
                anyWeight |= opts[i].Weight > 0f;

            if (!anyWeight)
                return 0;

            var chosen = PickWeightedOption(opts);

            // Value zurückgeben passend zum chosen path
            for (int i = 0; i < opts.Count; ++i)
                if (opts[i].Path == chosen)
                    return opts[i].Value;

            return 0;
        }

        private static int DecideExtraEffect(Character directParent, GameObject partnerPrefab, Vector3 center)
        {
            if (!IsExtraEffectEnabled())
                return 0;

            float range = Plugin.Configs.CLLC_Effect_SearchRange.Value;

            int directValue = 0;
            if (directParent)
                directValue = GetExtraEffect(directParent);

            int partnerValue = 0;
            {
                var ch = FindNearbyCharacterWithTrait(prefabMustMatch: partnerPrefab, center, range, exclude: directParent, traitGetter: GetExtraEffect);
                if (ch) partnerValue = GetExtraEffect(ch);
            }

            int anyValue = 0;
            {
                var ch = FindNearbyCharacterWithTrait(prefabMustMatch: null, center, range, exclude: directParent, traitGetter: GetExtraEffect);
                if (ch) anyValue = GetExtraEffect(ch);
            }

            var opts = new List<WeightedTraitOption>(5)
            {
                new WeightedTraitOption
                {
                    Path = TraitPath.DirectParent,
                    Value = directValue,
                    Weight = directValue != 0 ? Plugin.Configs.CLLC_Effect_WeightDirectParent.Value : 0f
                },
                new WeightedTraitOption
                {
                    Path = TraitPath.CurrentPartner,
                    Value = partnerValue,
                    Weight = partnerValue != 0 ? Plugin.Configs.CLLC_Effect_WeightCurrentPartner.Value : 0f
                },
                new WeightedTraitOption
                {
                    Path = TraitPath.AnyNearby,
                    Value = anyValue,
                    Weight = anyValue != 0 ? Plugin.Configs.CLLC_Effect_WeightAnyNearby.Value : 0f
                },
                new WeightedTraitOption
                {
                    Path = TraitPath.RandomNew,
                    Value = -1,
                    Weight = Plugin.Configs.CLLC_Effect_WeightRandomNew.Value
                },
                new WeightedTraitOption
                {
                    Path = TraitPath.None,
                    Value = 0,
                    Weight = Plugin.Configs.CLLC_Effect_WeightNone.Value
                }
            };

            foreach (var o in opts)
            {
                Plugin.LogDebug($"{o.Path}: value={o.Value}, weight={o.Weight}");
            }

            bool anyWeight = false;
            for (int i = 0; i < opts.Count; ++i)
                anyWeight |= opts[i].Weight > 0f;

            if (!anyWeight)
                return 0;

            var chosen = PickWeightedOption(opts);

            for (int i = 0; i < opts.Count; ++i)
                if (opts[i].Path == chosen)
                    return opts[i].Value;

            return 0;
        }

        private static Character FindNearbyCharacterWithTrait(
            GameObject prefabMustMatch,
            Vector3 center,
            float range,
            Character exclude,
            Func<Character, int> traitGetter)
        {
            // Use BaseAI instances (fast, common Valheim pattern)
            var list = BaseAI.BaseAIInstances;

            Character best = null;
            float bestDist = float.MaxValue;

            foreach (var ai in list)
            {
                if (!ai) continue;

                var go = ai.gameObject;
                if (!go) continue;

                if (exclude && go == exclude.gameObject)
                    continue;

                // Range filter
                float dist = Vector3.Distance(center, go.transform.position);
                if (range > 0f && dist > range)
                    continue;

                // Prefab filter (match by prefab name, because instances are "(Clone)")
                if (prefabMustMatch)
                {
                    string want = Utils.GetPrefabName(prefabMustMatch.name);
                    string have = Utils.GetPrefabName(go.name);
                    if (!string.Equals(want, have, StringComparison.Ordinal))
                        continue;
                }

                var ch = go.GetComponent<Character>();
                if (!ch) continue;

                // Must have trait (traitGetter returns 0 for None)
                if (traitGetter(ch) == 0)
                    continue;

                // Choose closest (predictable). Could also randomize later if you want.
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = ch;
                }
            }

            return best;
        }

    }
}
