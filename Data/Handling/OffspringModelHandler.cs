using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Jotunn.Managers;
using OfTamingAndBreeding.Data.Models;
using YamlDotNet.Core;

namespace OfTamingAndBreeding.Data.Handling
{
    internal class OffspringModelHandler : ModelHandler<Data.Models.Offspring>
    {

        public override string GetDirectoryName() => Data.Models.Offspring.GetDirectoryName();

        //------------------------------------------------
        // VALIDATE DATA
        //------------------------------------------------

        public override bool ValidateData(ModelHandlerContext ctx, string offspringName, Data.Models.Offspring data)
        {
            var model = $"{nameof(Models.Offspring)}.{offspringName}";
            var error = false;

            if (data.Growup != null)
            {
                if (data.Growup.grown == null)
                {
                    data.Growup.grown = new Models.Offspring.GrowupGrownData[] { };
                }

                foreach (var (grownData, i) in data.Growup.grown.Select((value, i) => (value, i)))
                {
                    grownData.weight = Math.Max(0f, grownData.weight);
                    if (grownData.prefab == null)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Growup)}.{nameof(data.Growup.grown)}.{i}.{nameof(grownData.prefab)} is empty");
                        error = true;
                    }
                }
            }

            return error == false;
        }

        //------------------------------------------------
        // PREPARE PREFAB
        //------------------------------------------------

        public override bool PreparePrefab(ModelHandlerContext ctx, string offspringName, Data.Models.Offspring data)
        {
            var model = $"{nameof(Models.Offspring)}.{offspringName}";

            //var offspring = PrefabManager.Instance.GetPrefab(offspringName);
            var offspring = ctx.GetPrefab(offspringName);
            if (offspring == null)
            {
                if (data.Clone == null)
                {
                    Plugin.LogError($"{model}.{nameof(data.Clone)} missing");
                    return false;
                }
                if (data.Clone.from == null)
                {
                    Plugin.LogError($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.from)} missing");
                    return false;
                }

                //var cloneFrom = PrefabManager.Instance.GetPrefab(data.Clone.from);
                var cloneFrom = ctx.GetPrefab(data.Clone.from);
                if (!cloneFrom)
                {
                    Plugin.LogError($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.from)} '{data.Clone.from}' not found");
                    return false;
                }

                Plugin.LogDebug($"{model}: Cloning prefab from '{cloneFrom.name}'");
                offspring = PrefabManager.Instance.CreateClonedPrefab(offspringName, cloneFrom.name);
            }

            ctx.CachePrefab(offspringName, offspring);
            return true;
        }

        //------------------------------------------------
        // VALIDATE PREFAB
        //------------------------------------------------

        public override bool ValidatePrefab(ModelHandlerContext ctx, string offspringName, Data.Models.Offspring data)
        {
            var model = $"{nameof(Models.Offspring)}.{offspringName}";
            var error = false;

            var offspring = ctx.GetPrefab(offspringName);
            if (!offspring)
            {
                Plugin.LogError($"{model}: Prefab not found");
                error = true;
            }
            else
            {
                if (!offspring.GetComponent<Character>())
                {
                    Plugin.LogError($"{model}: Prefab has no Character");
                    error = true;
                }
            }

            if (data.Growup != null)
            {
                foreach (var (grownData, i) in data.Growup.grown.Select((value, i) => (value, i)))
                {
                    if (!ctx.PrefabExists(grownData.prefab))
                    {
                        Plugin.LogFatal($"{model}.{nameof(data.Growup)}.{nameof(data.Growup.grown)}.{i}.{nameof(grownData.prefab)} '{grownData.prefab}' not found");
                        error = true;
                    }
                }
            }

            return error == false;
        }

        //------------------------------------------------
        // REGISTER PREFAB
        //------------------------------------------------

        public override void RegisterPrefab(ModelHandlerContext ctx, string offspringName, Data.Models.Offspring data)
        {
            var model = $"{nameof(Models.Offspring)}.{offspringName}";

            var offspring = PrefabManager.Instance.GetPrefab(offspringName);
            PrefabManager.Instance.RegisterToZNetScene(offspring);

            var offspringCharacter = offspring.GetComponent<Character>();
            var offspringGrowup = offspring.GetComponent<Growup>();

            Helpers.PrefabHelper.DestroyComponentIfExists<ItemDrop>(offspring);
            Helpers.PrefabHelper.DestroyComponentIfExists<Procreation>(offspring);
            Helpers.PrefabHelper.DestroyComponentIfExists<Tameable>(offspring);
            Helpers.PrefabHelper.DestroyComponentIfExists<CharacterDrop>(offspring);

            Helpers.PrefabHelper.DestroyComponentIfExists<MonsterAI>(offspring);
            if (offspring.GetComponent<AnimalAI>() == null)
            {
                Plugin.LogDebug($"{model}.{nameof(data.Growup)}: Adding AnimalAI component");
                offspring.AddComponent<AnimalAI>();
            }

            if (data.Character != null)
            {
                Plugin.LogDebug($"{model}.{nameof(data.Character)}: Setting Character values");

                offspringCharacter.m_group = data.Character.group;
                offspringCharacter.m_name = data.Character.name;
                if (offspringCharacter.m_name == null)
                {
                    offspringCharacter.m_name = "Baby";
                }
                Plugin.LogDebug($"{model}.{nameof(data.Character)}: Name: {offspringCharacter.m_name}");

                if (data.Character.scale != 1)
                {
                    Plugin.LogDebug($"{model}.{nameof(data.Character)}: Setting custom scaling to {data.Character.scale}");
                    offspring.transform.localScale = UnityEngine.Vector3.one * data.Character.scale;

                    // we need to clone the effect prefab to make it scaleable independently from its original effect prefab
                    // but we need to make sure that the original effect prefab only gets cloned once 
                    var prefx = $"{offspringName}_";
                    foreach (var eff in offspringCharacter.m_deathEffects.m_effectPrefabs)
                    {
                        var originalPrefabName = eff.m_prefab.name;
                        if (!originalPrefabName.StartsWith(prefx))
                        {
                            // not cloned yet, try to get it from any cache
                            var clonedName = $"{prefx}{originalPrefabName}";
                            var cloned = ctx.GetPrefab(clonedName);
                            if (!cloned)
                            {
                                // no cache? create clone
                                cloned = PrefabManager.Instance.CreateClonedPrefab(clonedName, eff.m_prefab);
                            }
                            eff.m_prefab = cloned;
                        }
                        eff.m_prefab.transform.localScale = UnityEngine.Vector3.one * data.Character.scale;
                        eff.m_scale = false;
                        eff.m_inheritParentScale = false;
                    }
                }

                if (data.Character.stickToFaction)
                {
                    Patches.Contexts.IsEnemyContext.prefabSticksToFaction.Add(offspringName.GetStableHashCode());
                }

            }

            if (data.Growup != null)
            {
                if (offspringGrowup == null)
                {
                    Plugin.LogDebug($"{model}.{nameof(data.Growup)}: Adding Growup component");
                    offspringGrowup = offspring.AddComponent<Growup>();
                }

                Plugin.LogDebug($"{model}.{nameof(data.Growup)}: Setting Growup values");
                offspringGrowup.m_growTime = data.Growup.growTime;
                offspringGrowup.m_inheritTame = data.Growup.inheritTame;

                if (data.Growup.grown.Length == 1)
                {
                    offspringGrowup.m_grownPrefab = ctx.GetPrefab(data.Growup.grown[0].prefab);
                    offspringGrowup.m_altGrownPrefabs = null;
                }
                else
                {
                    offspringGrowup.m_grownPrefab = null;
                    offspringGrowup.m_altGrownPrefabs = new List<Growup.GrownEntry>();
                    foreach (var grownData in data.Growup.grown)
                    {
                        offspringGrowup.m_altGrownPrefabs.Add(new Growup.GrownEntry
                        {
                            m_prefab = ctx.GetPrefab(grownData.prefab),
                            m_weight = grownData.weight,
                        });
                    }
                }

            }

        }

    }

}
