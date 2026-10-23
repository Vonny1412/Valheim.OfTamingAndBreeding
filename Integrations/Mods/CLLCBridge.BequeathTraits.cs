using System;
using System.Collections.Generic;
using UnityEngine;

namespace OfTamingAndBreeding.Integrations.Mods
{
    internal static partial class CllCBridge
    {

        public static void PassTraits(ZDO fromZdo, GameObject to)
        {
            var infusion = fromZdo.GetInt(Plugin.ZDOVars.z_CLLC_Infusion, 0);
            var effect = fromZdo.GetInt(Plugin.ZDOVars.z_CLLC_Effect, 0);
            ApplyTraits(infusion, effect, to);
        }

        public static void BequeathTraits(Character directParentCharacter, GameObject spawned)
        {
            // Safety / feature gating
            if (!IsRegistered || !IsEnabled())
                return;

            if (!spawned)
                return;

            // Decide which traits should be inherited (as int enum values; 0 = None)
            int infusion = DecideInfusion(directParentCharacter, spawned.transform.position);
            int effect = DecideExtraEffect(directParentCharacter, spawned.transform.position);
            ApplyTraits(infusion, effect, spawned);
        }

        private static void ApplyTraits(int infusion, int effect, GameObject to)
        {
            var spawnedGrowup = to.GetComponent<Growup>();
            var spawnedItemDrop = to.GetComponent<ItemDrop>();
            var spawnedCharacter = to.GetComponent<Character>();
            if ((bool)spawnedGrowup || (bool)spawnedItemDrop)
            {
                var zdo = to.GetComponent<ZNetView>().GetZDO();
                if (infusion != 0) zdo.Set(Plugin.ZDOVars.z_CLLC_Infusion, infusion);
                if (effect != 0) zdo.Set(Plugin.ZDOVars.z_CLLC_Effect, effect);
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
            public int Value; // 0=None, >0=Trait, -1=Random
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

        private static int DecideInfusion(Character parentCharacter, Vector3 center)
        {
            if (!IsInfusionEnabled())
            {
                return 0;
            }

            int parentValue = GetInfusion(parentCharacter);

            // Options: Count weights only if Value != 0 (for 1–3)
            var opts = new List<WeightedTraitOption>(5)
            {
                new WeightedTraitOption
                {
                    Path = TraitPath.DirectParent,
                    Value = parentValue,
                    Weight = parentValue != 0 ? Plugin.Configs.CLLC_Infusion_WeightParent.Value : 0f
                },
                new WeightedTraitOption
                {
                    Path = TraitPath.RandomNew,
                    Value = -1,
                    Weight = Plugin.Configs.CLLC_Infusion_WeightRandom.Value
                },
                new WeightedTraitOption
                {
                    Path = TraitPath.None,
                    Value = 0,
                    Weight = Plugin.Configs.CLLC_Infusion_WeightNone.Value
                }
            };

            // If all weights are 0: fallback -> None
            // (e.g., if RandomNew and None in config are 0 and all traits were 0)

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

        private static int DecideExtraEffect(Character parentCharacter, Vector3 center)
        {
            if (!IsExtraEffectEnabled())
            {
                return 0;
            }

            int parentValue = GetExtraEffect(parentCharacter);
            
            var opts = new List<WeightedTraitOption>(5)
            {
                new WeightedTraitOption
                {
                    Path = TraitPath.DirectParent,
                    Value = parentValue,
                    Weight = parentValue != 0 ? Plugin.Configs.CLLC_Effect_WeightParent.Value : 0f
                },
                new WeightedTraitOption
                {
                    Path = TraitPath.RandomNew,
                    Value = -1,
                    Weight = Plugin.Configs.CLLC_Effect_WeightRandom.Value
                },
                new WeightedTraitOption
                {
                    Path = TraitPath.None,
                    Value = 0,
                    Weight = Plugin.Configs.CLLC_Effect_WeightNone.Value
                }
            };

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

    }
}
