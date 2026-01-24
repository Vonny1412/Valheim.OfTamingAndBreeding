using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OfTamingAndBreeding.ThirdParty.Mods
{
    internal static partial class CllCBridge
    {

        private enum TraitPath
        {
            DirectParent,
            CurrentPartner,
            AnyNearby,
            RandomNew,
            None,
        }

        public static void PassTraits(ZDO fromZdo, GameObject to)
        {
            var infusion = fromZdo.GetInt(Plugin.ZDOVars.z_CLLC_Infusion, 0);
            var effect = fromZdo.GetInt(Plugin.ZDOVars.z_CLLC_Effect, 0);
            ApplyTraits(infusion, effect, to);
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

        private static int DecideInfusion(Character directParent, GameObject partnerPrefab, Vector3 center)
        {
            if (!IsInfusionEnabled())
                return 0;

            // Build path list from weights (only non-negative weights)
            var paths = BuildWeightedPaths(
                wDirect: Plugin.Configs.CLLC_Infusion_WeightDirectParent.Value,
                wPartner: Plugin.Configs.CLLC_Infusion_WeightCurrentPartner.Value,
                wAny: Plugin.Configs.CLLC_Infusion_WeightAnyNearby.Value,
                wRandom: Plugin.Configs.CLLC_Infusion_WeightRandomNew.Value,
                wNone: Plugin.Configs.CLLC_Infusion_WeightNone.Value
            );

            float range = Plugin.Configs.CLLC_Infusion_SearchRange.Value;

            // Progressive fallback: once a path fails, remove it and try again using only remaining (which are “lower priority”)
            while (paths.Count > 0)
            {
                var chosen = PickWeighted(paths);
                if (TryResolveInfusion(chosen, directParent, partnerPrefab, center, range, out int infusion))
                    return infusion;

                RemovePath(paths, chosen);
            }

            return 0;
        }

        private static int DecideExtraEffect(Character directParent, GameObject partnerPrefab, Vector3 center)
        {
            if (!IsExtraEffectEnabled())
                return 0;

            var paths = BuildWeightedPaths(
                wDirect: Plugin.Configs.CLLC_Effect_WeightDirectParent.Value,
                wPartner: Plugin.Configs.CLLC_Effect_WeightCurrentPartner.Value,
                wAny: Plugin.Configs.CLLC_Effect_WeightAnyNearby.Value,
                wRandom: Plugin.Configs.CLLC_Effect_WeightRandomNew.Value,
                wNone: Plugin.Configs.CLLC_Effect_WeightNone.Value
            );

            float range = Plugin.Configs.CLLC_Effect_SearchRange.Value;

            while (paths.Count > 0)
            {
                var chosen = PickWeighted(paths);
                if (TryResolveExtraEffect(chosen, directParent, partnerPrefab, center, range, out int effect))
                    return effect;

                RemovePath(paths, chosen);
            }

            return 0;
        }

        // ---------- Resolve paths ----------

        private static bool TryResolveInfusion(
            TraitPath path,
            Character directParent,
            GameObject partnerPrefab,
            Vector3 center,
            float range,
            out int infusion)
        {
            infusion = 0;

            switch (path)
            {
                case TraitPath.DirectParent:
                    if (!directParent) return false;
                    infusion = GetInfusion(directParent);
                    return infusion != 0;

                case TraitPath.CurrentPartner:
                    {
                        var ch = FindNearbyCharacterWithTrait(prefabMustMatch: partnerPrefab, center, range, exclude: directParent, traitGetter: GetInfusion);
                        if (!ch) return false;
                        infusion = GetInfusion(ch);
                        return infusion != 0;
                    }

                case TraitPath.AnyNearby:
                    {
                        var ch = FindNearbyCharacterWithTrait(prefabMustMatch: null, center, range, exclude: directParent, traitGetter: GetInfusion);
                        if (!ch) return false;
                        infusion = GetInfusion(ch);
                        return infusion != 0;
                    }

                case TraitPath.RandomNew:
                    // Let CLLC pick a random infusion, but we can only apply it to a Character.
                    // Here we just return a marker that caller can apply later by SetInfusionRandom if you want.
                    // Since your pipeline stores "pending int", we need an actual value. So:
                    // - We generate it on a temporary dummy? No.
                    // - Better: we apply random later when adult spawns.
                    // For now: choose 'None' here to keep system deterministic unless you want a different approach.
                    // If you DO want random now as int, you'd need to re-implement CLLC’s selection logic or read ZDO after calling SetInfusionRandom on a temp.
                    // Practical compromise: use a special sentinel value in ZDO and interpret it later.
                    infusion = -1;
                    return true;

                case TraitPath.None:
                    infusion = 0;
                    return true;
            }

            return false;
        }

        private static bool TryResolveExtraEffect(
            TraitPath path,
            Character directParent,
            GameObject partnerPrefab,
            Vector3 center,
            float range,
            out int effect)
        {
            effect = 0;

            switch (path)
            {
                case TraitPath.DirectParent:
                    if (!directParent) return false;
                    effect = GetExtraEffect(directParent);
                    return effect != 0;

                case TraitPath.CurrentPartner:
                    {
                        var ch = FindNearbyCharacterWithTrait(prefabMustMatch: partnerPrefab, center, range, exclude: directParent, traitGetter: GetExtraEffect);
                        if (!ch) return false;
                        effect = GetExtraEffect(ch);
                        return effect != 0;
                    }

                case TraitPath.AnyNearby:
                    {
                        var ch = FindNearbyCharacterWithTrait(prefabMustMatch: null, center, range, exclude: directParent, traitGetter: GetExtraEffect);
                        if (!ch) return false;
                        effect = GetExtraEffect(ch);
                        return effect != 0;
                    }

                case TraitPath.RandomNew:
                    effect = -1;
                    return true;

                case TraitPath.None:
                    effect = 0;
                    return true;
            }

            return false;
        }

        // ---------- Nearby search ----------

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

        // ---------- Weighted path helpers ----------

        private sealed class WeightedPath
        {
            public TraitPath Path;
            public float Weight;
        }

        private static List<WeightedPath> BuildWeightedPaths(float wDirect, float wPartner, float wAny, float wRandom, float wNone)
        {
            var list = new List<WeightedPath>(5);

            void Add(TraitPath p, float w)
            {
                if (w <= 0f) return;
                list.Add(new WeightedPath { Path = p, Weight = w });
            }

            // Priority order is implicit in the fallback elimination; but we keep all initially.
            Add(TraitPath.DirectParent, wDirect);
            Add(TraitPath.CurrentPartner, wPartner);
            Add(TraitPath.AnyNearby, wAny);
            Add(TraitPath.RandomNew, wRandom);
            Add(TraitPath.None, wNone);

            // If everything is <= 0, behave as None
            if (list.Count == 0)
                list.Add(new WeightedPath { Path = TraitPath.None, Weight = 1f });

            return list;
        }

        private static TraitPath PickWeighted(List<WeightedPath> list)
        {
            float sum = 0f;
            for (int i = 0; i < list.Count; i++)
                sum += Mathf.Max(0f, list[i].Weight);

            if (sum <= 0f)
                return TraitPath.None;

            float r = UnityEngine.Random.value * sum;
            float acc = 0f;
            for (int i = 0; i < list.Count; i++)
            {
                acc += Mathf.Max(0f, list[i].Weight);
                if (r <= acc)
                    return list[i].Path;
            }

            return list[list.Count - 1].Path;
        }

        private static void RemovePath(List<WeightedPath> list, TraitPath path)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i].Path == path)
                    list.RemoveAt(i);
            }
        }
    }
}
