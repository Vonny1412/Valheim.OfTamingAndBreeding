using Jotunn.Managers;
using OfTamingAndBreeding.Helpers;
using OfTamingAndBreeding.ValheimAPI.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OfTamingAndBreeding.Data.Processing
{
    internal class OffspringProcessor : Base.DataProcessor<Models.Offspring>
    {

        public override string DirectoryName => Models.Offspring.DirectoryName;

        public override string PrefabTypeName => "creature";

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

        public override bool ValidateData(Base.PrefabRegistry reg, string offspringName, Models.Offspring data)
        {
            var model = $"{nameof(Models.Offspring)}.{offspringName}";
            var error = false;

            if (data.Clone != null)
            {
                if (data.Clone.Name == null)
                {
                    Plugin.LogError($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.Name)}: Missing field");
                    error = true;
                }
                if (data.Clone.MaxHealthFactor <= 0)
                {
                    Plugin.LogError($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.MaxHealthFactor)}: Zero or negative values not allowed");
                    error = true;
                }
                if (data.Clone.MaxHealthFactor > 1)
                {
                    Plugin.LogWarning($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.MaxHealthFactor)}: Offspring is getting increased max health");
                }
                if (data.Clone.RemoveEffects != null)
                {
                    if (data.Clone.RemoveEffects.Length == 0)
                    {
                        Plugin.LogWarning($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.RemoveEffects)}: List is empty and will be set to null");
                        data.Clone.RemoveEffects = null;
                    }
                }
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
                        Plugin.LogWarning($"{model}.{nameof(data.Components)}.{nameof(data.Components.Growup)}({nameof(Models.SubData.ComponentBehavior.Inherit)}): Component data will be ignored");
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
        // RESERVE PREFAB
        //------------------------------------------------

        public override bool ReservePrefab(Base.PrefabRegistry reg, string offspringName, Models.Offspring data)
        {
            var model = $"{nameof(Models.Offspring)}.{offspringName}";

            var offspring = reg.GetReservedPrefab(offspringName);
            if (offspring == null)
            {
                var custom = reg.GetCustomPrefab(offspringName);
                offspring = reg.GetOriginalPrefab(offspringName);
                if (offspring == null || custom != null) // need clone (not cloned yet / previously cloned, reactivate)
                {

                    if (data.Clone == null)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Clone)}: Field missing");
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
                        offspring = reg.CreateClonedPrefab(offspringName, cloneFrom.name);

                        //ctx.SetCustomPrefabUsingBackup(offspringName, backup);
                    }
                    else
                    {
                        // previously cloned - reactivate
                        // hint: offspring==custom

                        Plugin.LogDebug($"{model}.{nameof(data.Clone)}: Reactivating cloned prefab for '{cloneFrom.name}'");
                        var backup = reg.GetUnusedCustomPrefabBackup(cloneFrom);
                        RestorePrefabFromBackup(backup, offspring);

                        reg.SetCustomPrefabUsingBackup(offspringName, backup);
                    }

                    PrepareClone(reg, offspringName, data, offspring);
                }
                else
                {
                    reg.MakeOriginalBackup(offspring);
                }

                reg.ReservePrefab(offspringName, offspring);
            }

            return true;
        }

        private void PrepareClone(Base.PrefabRegistry reg, string offspringName, Models.Offspring data, UnityEngine.GameObject offspring)
        {
            var model = $"{nameof(Models.Offspring)}.{offspringName}";

            {
                //
                // EFFECTS
                //

                var removedEffects = new List<string>();

                var debugEffects = data.Clone.DebugEffects == true && ZNet.instance.IsServer();
                if (debugEffects) Plugin.LogMessage($"{model}: DebugEffects");

                var removeSet = data.Clone.RemoveEffects != null
                    ? new HashSet<string>(data.Clone.RemoveEffects, StringComparer.OrdinalIgnoreCase)
                    : null;

                var footStep = offspring.GetComponent<FootStep>();
                if ((bool)footStep)
                {
                    var m_effects = new List<FootStep.StepEffect>();

                    foreach (var effect in footStep.m_effects)
                    {
                        if (debugEffects)
                            Plugin.LogMessage($"  {nameof(FootStep)} ({effect.m_motionType})");

                        if (removeSet == null)
                        {
                            if (debugEffects)
                            {
                                foreach (var p in effect.m_effectPrefabs)
                                    Plugin.LogMessage($"  - {p.name}");
                            }
                            continue;
                        }

                        var kept = new List<UnityEngine.GameObject>(effect.m_effectPrefabs.Length);

                        foreach (var p in effect.m_effectPrefabs)
                        {
                            var remove = removeSet.Contains(p.name);

                            if (remove)
                                removedEffects.Add(p.name);

                            if (debugEffects)
                                Plugin.LogMessage(remove
                                    ? $"  - {p.name} (removed)"
                                    : $"  - {p.name}");

                            if (!remove)
                                kept.Add(p);
                        }

                        if (kept.Count != 0)
                        {
                            m_effects.Add(new FootStep.StepEffect
                            {
                                m_name = effect.m_name,
                                m_motionType = effect.m_motionType,
                                m_material = effect.m_material,
                                m_effectPrefabs = kept.ToArray()
                            });
                        }
                    }

                    if (data.Clone.RemoveEffects != null)
                    {
                        footStep.m_effects = m_effects;
                    }
                }

            }

            {
                //
                // CHARACTER
                //

                var offspringCharacter = offspring.GetComponent<Character>();
                offspringCharacter.m_boss = false; // offsprings are not bosses
                offspringCharacter.m_bossEvent = ""; // offsprings are not bosses

                Plugin.LogDebug($"{model}.{nameof(data.Clone)}: Setting Character values");

                offspringCharacter.m_name = data.Clone.Name;
                offspringCharacter.m_health = offspringCharacter.m_health * data.Clone.MaxHealthFactor;

                if (data.Clone.Scale != 1 && data.Clone.Scale != 0)
                {
                    var setScale = data.Clone.Scale;

                    Plugin.LogDebug($"{model}.{nameof(data.Clone)}: Setting custom scaling to {setScale}");

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

                    var col = offspring.GetComponent<UnityEngine.CapsuleCollider>();
                    if (col)
                    {
                        col.height *= setScale;
                        col.radius *= setScale;
                        col.center *= setScale;
                    }

                    Plugin.LogDebug($"{model}.{nameof(data.Clone)}: Setting vfx scaling");
                    Helpers.VfxHelper.ScaleVfx(offspring, setScale); // scale model particles

                    Plugin.LogDebug($"{model}.{nameof(data.Clone)}: Setting effects scaling");

                    var scaler = reg.GetOrAddComponent<OTAB_ScaledCreature>(offspringName, offspring);
                    scaler.m_customEffectScale = setScale * setScale; // for some reasons setScale*setScale is required
                    scaler.m_customAnimationScale = 1/setScale; // because we are using it as a multiplier

                    foreach (var eff in offspringCharacter.m_deathEffects.m_effectPrefabs)
                    {
                        eff.m_scale = true; // important
                        eff.m_inheritParentScale = true; // important
                        eff.m_multiplyParentVisualScale = false;
                    }

                }

            }

        }

        //------------------------------------------------
        // VALIDATE PREFAB
        //------------------------------------------------

        public override bool ValidatePrefab(Base.PrefabRegistry reg, string offspringName, Models.Offspring data)
        {
            var model = $"{nameof(Models.Offspring)}.{offspringName}";
            var error = false;

            var offspring = reg.GetReservedPrefab(offspringName);
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
                    if (!reg.PrefabExists(grownData.Prefab))
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

        public override void RegisterPrefab(Base.PrefabRegistry reg, string offspringName, Models.Offspring data)
        {
            var model = $"{nameof(Models.Offspring)}.{offspringName}";

            var offspring = reg.GetReservedPrefab(offspringName);
            if (reg.IsCustomPrefab(offspringName))
            {
                PrefabManager.Instance.RegisterToZNetScene(offspring);
            }

            reg.DestroyComponentIfExists<ItemDrop>(offspringName, offspring); // offsprings are not items (wait, why am i even doing this? dont remember.. just keep it)
            reg.DestroyComponentIfExists<Procreation>(offspringName, offspring); // offsprings do not procreate
            reg.DestroyComponentIfExists<Tameable>(offspringName, offspring); // offsprings cannot be explicite tamed
            reg.DestroyComponentIfExists<CharacterDrop>(offspringName, offspring); // offsprings do not drop items
            reg.DestroyComponentIfExists<MonsterAI>(offspringName, offspring); // offsprings do not attack
            reg.GetOrAddComponent<AnimalAI>(offspringName, offspring); // offsprings do act like passive animals

            if (data.Components.Growup == Models.SubData.ComponentBehavior.Patch)
            {
                if (data.Growup != null)
                {

                    var offspringGrowup = reg.GetOrAddComponent<Growup>(offspringName, offspring);
                    Plugin.LogDebug($"{model}.{nameof(data.Growup)}: Setting Growup values");

                    if (data.Growup.GrowTime != null) offspringGrowup.m_growTime = (float)data.Growup.GrowTime;
                    if (data.Growup.InheritTame != null) offspringGrowup.m_inheritTame = (bool)data.Growup.InheritTame;
                    
                    offspringGrowup.m_grownPrefab = null; // WeakReference never use explicite prefab, always use list - stick to the system
                    offspringGrowup.m_altGrownPrefabs = new List<Growup.GrownEntry>();
                    foreach (var grownData in data.Growup.Grown)
                    {
                        offspringGrowup.m_altGrownPrefabs.Add(new Growup.GrownEntry
                        {
                            m_prefab = reg.GetOriginalPrefab(grownData.Prefab),
                            m_weight = grownData.Weight,
                        });
                    }

                }
            }
            else if (data.Components.Growup == Models.SubData.ComponentBehavior.Remove)
            {
                Plugin.LogDebug($"{model}.{nameof(Growup)}: Removing Growup component (if exist)");
                reg.DestroyComponentIfExists<Growup>(offspringName, offspring);
            }

        }

        //------------------------------------------------
        // FINALIZE
        //------------------------------------------------

        public override void Finalize(Base.PrefabRegistry reg)
        {
        }

        //------------------------------------------------
        // UNREGISTER PREFAB
        //------------------------------------------------

        public override void RestorePrefab(Base.PrefabRegistry reg, string offspringName)
        {
            reg.Restore(offspringName, RestorePrefabFromBackup);
        }

        private void RestorePrefabFromBackup(UnityEngine.GameObject backup, UnityEngine.GameObject current)
        {
            RestoreHelper.RestoreComponent<Character>(backup, current);
            RestoreHelper.RestoreComponent<AnimalAI>(backup, current);
            RestoreHelper.RestoreComponent<ItemDrop>(backup, current);
            RestoreHelper.RestoreComponent<Procreation>(backup, current);
            RestoreHelper.RestoreComponent<Tameable>(backup, current);
            RestoreHelper.RestoreComponent<CharacterDrop>(backup, current);
            RestoreHelper.RestoreComponent<MonsterAI>(backup, current);
            RestoreHelper.RestoreComponent<Growup>(backup, current);
        }
        
        //------------------------------------------------
        // CLEANUP
        //------------------------------------------------

        public override void Cleanup(Base.PrefabRegistry reg)
        {
        }

    }

}
