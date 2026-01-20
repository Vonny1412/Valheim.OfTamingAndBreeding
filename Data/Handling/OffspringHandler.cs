using Jotunn.Managers;
using OfTamingAndBreeding.Data.Handling.Base;
using OfTamingAndBreeding.Data.Models;
using OfTamingAndBreeding.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OfTamingAndBreeding.Data.Handling
{
    internal class OffspringHandler : DataHandler<Models.Offspring>
    {

        public override string DirectoryName => Models.Offspring.DirectoryName;

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

        public override bool ValidateData(DataHandlerContext ctx, string offspringName, Models.Offspring data)
        {
            var model = $"{nameof(Models.Offspring)}.{offspringName}";
            var error = false;

            if (data.Growup != null)
            {
                if (data.Growup.Grown == null || data.Growup.Grown.Length == 0)
                {
                    Plugin.LogError($"{model}.{nameof(data.Growup)}.{nameof(data.Growup.Grown)} is null or empty");
                    error = true;
                }
                else
                {
                    foreach (var (grownData, i) in data.Growup.Grown.Select((value, i) => (value, i)))
                    {
                        grownData.Weight = Math.Max(0f, grownData.Weight);
                        if (grownData.Prefab == null)
                        {
                            Plugin.LogError($"{model}.{nameof(data.Growup)}.{nameof(data.Growup.Grown)}.{i}.{nameof(grownData.Prefab)} is empty");
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

        public override bool PreparePrefab(DataHandlerContext ctx, string offspringName, Models.Offspring data)
        {
            var model = $"{nameof(Models.Offspring)}.{offspringName}";

            var offspring = ctx.GetPrefab(offspringName);
            if (offspring == null)
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
                offspring = ctx.CreateClonedPrefab(offspringName, cloneFrom.name);
            }
            else
            {
                ctx.MakeBackup(offspringName, offspring);
            }

            ctx.CachePrefab(offspringName, offspring);
            return true;
        }

        //------------------------------------------------
        // VALIDATE PREFAB
        //------------------------------------------------

        public override bool ValidatePrefab(DataHandlerContext ctx, string offspringName, Models.Offspring data)
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
                foreach (var (grownData, i) in data.Growup.Grown.Select((value, i) => (value, i)))
                {
                    if (!ctx.PrefabExists(grownData.Prefab))
                    {
                        Plugin.LogFatal($"{model}.{nameof(data.Growup)}.{nameof(data.Growup.Grown)}.{i}.{nameof(grownData.Prefab)}: '{grownData.Prefab}' not found");
                        error = true;
                    }
                }
            }

            return error == false;
        }

        //------------------------------------------------
        // REGISTER PREFAB
        //------------------------------------------------

        public override void RegisterPrefab(DataHandlerContext ctx, string offspringName, Models.Offspring data)
        {
            var model = $"{nameof(Models.Offspring)}.{offspringName}";

            var offspring = PrefabManager.Instance.GetPrefab(offspringName);
            PrefabManager.Instance.RegisterToZNetScene(offspring);

            var offspringCharacter = offspring.GetComponent<Character>();
            offspringCharacter.m_boss = false;
            offspringCharacter.m_bossEvent = "";

            ctx.GetOrAddComponent<AnimalAI>(offspringName, offspring);

            ctx.DestroyComponentIfExists<ItemDrop>(offspringName, offspring);
            ctx.DestroyComponentIfExists<Procreation>(offspringName, offspring);
            ctx.DestroyComponentIfExists<Tameable>(offspringName, offspring);
            ctx.DestroyComponentIfExists<CharacterDrop>(offspringName, offspring);
            ctx.DestroyComponentIfExists<MonsterAI>(offspringName, offspring);

            if (data.Character != null)
            {
                Plugin.LogDebug($"{model}.{nameof(data.Character)}: Setting Character values");

                if (data.Character.Name != null)    offspringCharacter.m_name = data.Character.Name;
                if (data.Character.Group != null)   offspringCharacter.m_group = data.Character.Group;

                if (data.Character.Scale != 1)
                {
                    var setScale = data.Character.Scale;

                    Plugin.LogDebug($"{model}.{nameof(data.Character)}: Setting custom scaling to {setScale}");
                    offspring.transform.localScale = UnityEngine.Vector3.one * setScale;

                    Helpers.VfxHelper.ScaleVfx(offspring, setScale); // scale model particles
                    Patches.Contexts.DataContext.SetObjectAnimationScaling(offspringName, 1f/setScale); // scale animations

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
                        eff.m_prefab.transform.localScale = UnityEngine.Vector3.one * setScale;
                        eff.m_scale = false;
                        eff.m_inheritParentScale = false;
                    }

                }

                if (data.Character.StickToFaction)
                {
                    Patches.Contexts.DataContext.SetObjectSticksToFaction(offspringName);
                }

            }

            if (data.Growup != null)
            {
                var offspringGrowup = ctx.GetOrAddComponent<Growup>(offspringName, offspring);
                Plugin.LogDebug($"{model}.{nameof(data.Growup)}: Setting Growup values");

                offspringGrowup.m_growTime = data.Growup.GrowTime;
                offspringGrowup.m_inheritTame = data.Growup.InheritTame;

                offspringGrowup.m_grownPrefab = null;
                offspringGrowup.m_altGrownPrefabs = new List<Growup.GrownEntry>();
                foreach (var grownData in data.Growup.Grown)
                {
                    offspringGrowup.m_altGrownPrefabs.Add(new Growup.GrownEntry
                    {
                        m_prefab = ctx.GetPrefab(grownData.Prefab),
                        m_weight = grownData.Weight,
                    });
                }

            }
            else
            {
                Plugin.LogDebug($"{model}.{nameof(Growup)}: Removing Growup component (if exist)");
                ctx.DestroyComponentIfExists<Growup>(offspringName, offspring);
            }

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

        public override void RestorePrefab(DataHandlerContext ctx, string offspringName, Models.Offspring data)
        {
            ctx.Restore(offspringName, (GameObject backup, GameObject current) => {

                RestoreHelper.RestoreComponent<Character>(backup, current);
                RestoreHelper.RestoreComponent<AnimalAI>(backup, current);
                RestoreHelper.RestoreComponent<ItemDrop>(backup, current);
                RestoreHelper.RestoreComponent<Procreation>(backup, current);
                RestoreHelper.RestoreComponent<Tameable>(backup, current);
                RestoreHelper.RestoreComponent<CharacterDrop>(backup, current);
                RestoreHelper.RestoreComponent<MonsterAI>(backup, current);
                RestoreHelper.RestoreComponent<Growup>(backup, current);

            });
        }

    }

}
