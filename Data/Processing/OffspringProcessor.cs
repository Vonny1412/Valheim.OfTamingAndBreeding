using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Jotunn.Managers;

using OfTamingAndBreeding.Helpers;
namespace OfTamingAndBreeding.Data.Processing
{
    internal class OffspringProcessor : Base.DataProcessor<Models.Offspring>
    {

        public override string DirectoryName => Models.Offspring.DirectoryName;

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

        public override bool ValidateData(Base.DataProcessorContext ctx, string offspringName, Models.Offspring data)
        {
            var model = $"{nameof(Models.Offspring)}.{offspringName}";
            var error = false;

            switch (data.Components.Character)
            {
                case Models.SubData.ComponentBehavior.Remove:
                    Plugin.LogDebug($"{model}.{nameof(data.Components)}.{nameof(data.Components.Character)}({nameof(Models.SubData.ComponentBehavior.Remove)}): Component cannot be removed");
                    break;
                case Models.SubData.ComponentBehavior.Patch:
                    if (data.Character == null)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Components)}.{nameof(data.Components.Character)}({nameof(Models.SubData.ComponentBehavior.Patch)}): Missing component data");
                        error = true;
                    }
                    break;
                case Models.SubData.ComponentBehavior.Inherit:
                    if (data.Character != null)
                    {
                        Plugin.LogDebug($"{model}.{nameof(data.Components)}.{nameof(data.Components.Character)}({nameof(Models.SubData.ComponentBehavior.Inherit)}): Component data will be ignored");
                    }
                    break;
            }

            switch (data.Components.Growup)
            {
                case Models.SubData.ComponentBehavior.Remove:
                    // can be removed
                    break;
                case Models.SubData.ComponentBehavior.Patch:
                    if (data.Growup == null)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Components)}.{nameof(data.Components.Growup)}({nameof(Models.SubData.ComponentBehavior.Patch)}): Missing component data");
                        error = true;
                    }
                    break;
                case Models.SubData.ComponentBehavior.Inherit:
                    if (data.Growup != null)
                    {
                        Plugin.LogDebug($"{model}.{nameof(data.Components)}.{nameof(data.Components.Growup)}({nameof(Models.SubData.ComponentBehavior.Inherit)}): Component data will be ignored");
                    }
                    break;
            }

            if (data.Growup != null && data.Components.Growup == Models.SubData.ComponentBehavior.Patch)
            {
                if (data.Growup.Grown == null || data.Growup.Grown.Length == 0)
                {
                    Plugin.LogError($"{model}.{nameof(data.Growup)}.{nameof(data.Growup.Grown)}: List is null or empty");
                    error = true;
                }
                else
                {
                    foreach (var (grownData, i) in data.Growup.Grown.Select((value, i) => (value, i)))
                    {
                        grownData.Weight = Math.Max(0f, grownData.Weight);
                        if (grownData.Prefab == null)
                        {
                            Plugin.LogError($"{model}.{nameof(data.Growup)}.{nameof(data.Growup.Grown)}.{i}.{nameof(grownData.Prefab)}: Field is empty");
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

        public override bool PreparePrefab(Base.DataProcessorContext ctx, string offspringName, Models.Offspring data)
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

        public override bool ValidatePrefab(Base.DataProcessorContext ctx, string offspringName, Models.Offspring data)
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

            if (data.Growup != null && data.Components.Growup == Models.SubData.ComponentBehavior.Patch)
            {
                // we already validated data.Growup.Grown != null
                foreach (var (grownData, i) in data.Growup.Grown.Select((value, i) => (value, i)))
                {
                    if (!ctx.PrefabExists(grownData.Prefab))
                    {
                        Plugin.LogError($"{model}.{nameof(data.Growup)}.{nameof(data.Growup.Grown)}.{i}.{nameof(grownData.Prefab)}: '{grownData.Prefab}' not found");
                        error = true;
                    }
                }
            }

            return error == false;
        }

        //------------------------------------------------
        // REGISTER PREFAB
        //------------------------------------------------

        public override void RegisterPrefab(Base.DataProcessorContext ctx, string offspringName, Models.Offspring data)
        {
            var model = $"{nameof(Models.Offspring)}.{offspringName}";

            var offspring = ctx.GetPrefab(offspringName);
            PrefabManager.Instance.RegisterToZNetScene(offspring);

            ctx.DestroyComponentIfExists<ItemDrop>(offspringName, offspring); // offsprings are not items (wait, why am i even doing this? dont remember.. just keep it)
            ctx.DestroyComponentIfExists<Procreation>(offspringName, offspring); // offsprings do nor procreate
            ctx.DestroyComponentIfExists<Tameable>(offspringName, offspring); // offsprings cannot be explicite tamed
            ctx.DestroyComponentIfExists<CharacterDrop>(offspringName, offspring); // offsprings do not drop items
            ctx.DestroyComponentIfExists<MonsterAI>(offspringName, offspring); // offsprings do not attack
            ctx.GetOrAddComponent<AnimalAI>(offspringName, offspring); // offsprings do act like passive animals

            var footStep = offspring.GetComponent<FootStep>();
            if ((bool)footStep)
            {
                footStep.m_effects.Clear();
            }



            if (data.Components.Character == Models.SubData.ComponentBehavior.Patch)
            {
                if (data.Character != null)
                {
                    var offspringCharacter = offspring.GetComponent<Character>();
                    offspringCharacter.m_boss = false; // offsprings are not bosses
                    offspringCharacter.m_bossEvent = ""; // offsprings are not bosses

                    Plugin.LogDebug($"{model}.{nameof(data.Character)}: Setting Character values");

                    if (data.Character.Name != null) offspringCharacter.m_name = data.Character.Name;
                    if (data.Character.Group != null) offspringCharacter.m_group = data.Character.Group;
                    if (data.Character.TamesStickToFaction) Runtime.Character.SetSticksToFaction(offspringName);
                    
                    if (data.Character.Scale != 1 && data.Character.Scale != 0)
                    {
                        var setScale = data.Character.Scale;

                        Plugin.LogDebug($"{model}.{nameof(data.Character)}: Setting custom scaling to {setScale}");

                        offspring.transform.localScale = UnityEngine.Vector3.one * setScale;
                        offspringCharacter.m_eye.position = new UnityEngine.Vector3(
                            offspringCharacter.m_eye.position.x * setScale,
                            offspringCharacter.m_eye.position.y * setScale,
                            offspringCharacter.m_eye.position.z * setScale
                            );

                        offspringCharacter.m_speed *= setScale;

                        offspringCharacter.m_crouchSpeed *= setScale;
                        offspringCharacter.m_walkSpeed *= setScale;
                        offspringCharacter.m_runSpeed *= setScale;
                        offspringCharacter.m_swimSpeed *= setScale;
                        offspringCharacter.m_flySlowSpeed *= setScale;
                        offspringCharacter.m_flyFastSpeed *= setScale;

                        //offspringCharacter.m_turnSpeed /= setScale;
                        //offspringCharacter.m_runTurnSpeed /= setScale;
                        //offspringCharacter.m_swimTurnSpeed /= setScale;
                        //offspringCharacter.m_flyTurnSpeed /= setScale;

                        var col = offspring.GetComponent<CapsuleCollider>();
                        if (col)
                        {
                            col.height *= setScale;
                            col.radius *= setScale;
                            col.center *= setScale;
                        }

                        Plugin.LogDebug($"{model}.{nameof(data.Character)}: Setting vfx scaling");
                        Helpers.VfxHelper.ScaleVfx(offspring, setScale); // scale model particles
                        Plugin.LogDebug($"{model}.{nameof(data.Character)}: Setting animation scaling");
                        Runtime.Character.SetAnimationScaling(offspringName, 1/setScale); // scale animations

                        // we need to clone the effect prefab to make it scaleable independently from its original effect prefab
                        // but we need to make sure that the original effect prefab only gets cloned once 
                        Plugin.LogDebug($"{model}.{nameof(data.Character)}: Setting effects scaling");
                        var prefix = $"OTAB_{offspringName}_";
                        foreach (var eff in offspringCharacter.m_deathEffects.m_effectPrefabs)
                        {
                            var originalPrefabName = eff.m_prefab.name;
                            if (!originalPrefabName.StartsWith(prefix))
                            {
                                // not cloned yet, try to get it from any cache
                                var clonedName = $"{prefix}{originalPrefabName}";
                                var cloned = ctx.GetPrefab(clonedName);
                                if (!cloned)
                                {
                                    // no cache? create clone
                                    cloned = PrefabManager.Instance.CreateClonedPrefab(clonedName, eff.m_prefab);
                                }
                                eff.m_prefab = cloned;
                            }
                            eff.m_prefab.transform.localScale = UnityEngine.Vector3.one * (setScale/2); // i dunno why but /2 is doing the trick!
                            eff.m_scale = false;
                            eff.m_inheritParentScale = false;
                        }

                    }

                }
            }
            else if (data.Components.Character == Models.SubData.ComponentBehavior.Remove)
            {
                // ignore, cannot be removed
            }

            if (data.Components.Growup == Models.SubData.ComponentBehavior.Patch)
            {
                if (data.Growup != null)
                {

                    var offspringGrowup = ctx.GetOrAddComponent<Growup>(offspringName, offspring);
                    Plugin.LogDebug($"{model}.{nameof(data.Growup)}: Setting Growup values");

                    if (data.Growup.GrowTime != null) offspringGrowup.m_growTime = (float)data.Growup.GrowTime;
                    if (data.Growup.InheritTame != null) offspringGrowup.m_inheritTame = (bool)data.Growup.InheritTame;
                    
                    offspringGrowup.m_grownPrefab = null; // WeakReference never use explicite prefab, always use list - stick to the system
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
            }
            else if (data.Components.Growup == Models.SubData.ComponentBehavior.Remove)
            {
                Plugin.LogDebug($"{model}.{nameof(Growup)}: Removing Growup component (if exist)");
                ctx.DestroyComponentIfExists<Growup>(offspringName, offspring);
            }

        }

        //------------------------------------------------
        // CLEANUP
        //------------------------------------------------

        public override void Cleanup(Base.DataProcessorContext ctx)
        {

        }

        //------------------------------------------------
        // UNREGISTER PREFAB
        //------------------------------------------------

        public override void RestorePrefab(Base.DataProcessorContext ctx, string offspringName, Models.Offspring data)
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
