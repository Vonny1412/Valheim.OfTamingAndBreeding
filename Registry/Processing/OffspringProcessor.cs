using Jotunn.Managers;
using OfTamingAndBreeding.Components;
using OfTamingAndBreeding.Components.Base;
using OfTamingAndBreeding.Components.Traits;
using OfTamingAndBreeding.Data.Models;
using OfTamingAndBreeding.Data.Models.SubData;
using OfTamingAndBreeding.OTABUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Core;

namespace OfTamingAndBreeding.Registry.Processing
{
    internal class OffspringProcessor : Base.DataProcessor<OffspringData>
    {

        public override string DirectoryName => OffspringData.DirectoryName;

        public override string PrefabTypeName => "creature";

        public override string GetDataKey(string filePath) => null;

        public override bool LoadFromFile(string filePath) => LoadFromYamlFile(filePath);

        //------------------------------------------------
        // PREPARE
        //------------------------------------------------

        public override void PrepareProcess()
        {

        }

        //------------------------------------------------
        // VALIDATE DATA
        //------------------------------------------------

        public override bool ValidateData(string offspringName, OffspringData data)
        {
            var model = $"{nameof(OffspringData)}.{offspringName}";
            var error = false;

            if (data.Clone != null)
            {
                if (data.Clone.Name == null)
                {
                    Plugin.LogError($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.Name)}: Missing field");
                    error = true;
                }
                if (data.Clone.MaxHealthFactor.HasValue)
                {
                    if (data.Clone.MaxHealthFactor.Value <= 0)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.MaxHealthFactor)}: Zero or negative values not allowed");
                        error = true;
                    }
                    else if (data.Clone.MaxHealthFactor.Value > 1)
                    {
                        Plugin.LogServerWarning($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.MaxHealthFactor)}: Offspring is getting increased max health");
                    }
                }
                if (data.Clone.RemoveEffects != null && data.Clone.RemoveEffects.Length == 0)
                {
                    Plugin.LogServerWarning($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.RemoveEffects)}: List is empty and will be set to null");
                    data.Clone.RemoveEffects = null;
                }
                if (data.Clone.Scale.HasValue)
                {
                    if (data.Clone.Scale.Value <= 0)
                    {
                        Plugin.LogServerWarning($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.Scale)}: Negative or zero value not allowed - setting to null");
                        data.Clone.Scale = null;
                    }
                    else if (data.Clone.Scale.Value == 1)
                    {
                        Plugin.LogServerInfo($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.Scale)}: Value of 1 will do nothing - setting to null");
                        data.Clone.Scale = null;
                    }
                }
                if (data.Clone.MaxHealthFactor.HasValue)
                {
                    if (data.Clone.MaxHealthFactor.Value < 0)
                    {
                        // maybe 0% could be fun? just allow it for now
                        Plugin.LogServerWarning($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.MaxHealthFactor)}: Negative value not allowed - setting to null");
                        data.Clone.MaxHealthFactor = null;
                    }
                    else if (data.Clone.MaxHealthFactor.Value == 1)
                    {
                        Plugin.LogServerInfo($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.MaxHealthFactor)}: Value of 1 will do nothing - setting to null");
                        data.Clone.MaxHealthFactor = null;
                    }
                }
            }

            switch (data.Components.Growup)
            {
                case ComponentBehavior.Remove:
                    // can be removed
                    break;
                case ComponentBehavior.Patch:
                    if (data.Growup == null)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Components)}.{nameof(data.Components.Growup)}({nameof(ComponentBehavior.Patch)}): Missing component data");
                        error = true;
                    }
                    break;
                case ComponentBehavior.Inherit:
                    if (data.Growup != null)
                    {
                        Plugin.LogServerWarning($"{model}.{nameof(data.Components)}.{nameof(data.Components.Growup)}({nameof(ComponentBehavior.Inherit)}): Component data will be ignored");
                    }
                    break;
            }

            if (data.Growup != null && data.Components.Growup == ComponentBehavior.Patch)
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

        public override bool ReservePrefab(string offspringName, OffspringData data)
        {
            var model = $"{nameof(OffspringData)}.{offspringName}";

            var offspring = PrefabRegistry.Instance.GetReservedPrefab(offspringName);
            if (offspring == null)
            {
                var custom = PrefabRegistry.Instance.GetCustomPrefab(offspringName);
                offspring = PrefabRegistry.Instance.GetOriginalPrefab(offspringName);
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

                    if (PrefabRegistry.IsCustomPrefab(data.Clone.From))
                    {
                        Plugin.LogError($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.From)}: Cannot clone from cloned prefab '{data.Clone.From}'");
                        return false;
                    }

                    var cloneFrom = PrefabRegistry.Instance.GetOriginalPrefab(data.Clone.From);
                    if (!cloneFrom)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.From)}: Prefab '{data.Clone.From}' not found");
                        return false;
                    }

                    if (custom == null)
                    {
                        // not cloned yet
                        offspring = PrefabRegistry.Instance.CreateCustomPrefab(offspringName, cloneFrom.name);
                    }
                    else
                    {
                        // previously cloned - reactivate
                        Plugin.LogServerDebug($"{model}.{nameof(data.Clone)}: Reactivating cloned prefab for '{cloneFrom.name}'");
                        offspring = PrefabRegistry.Instance.ReactivateCustomPrefab(offspringName, cloneFrom.name);
                    }
                }
                else
                {
                    PrefabRegistry.Instance.MakeOriginalBackup(offspringName);
                }

                PrefabRegistry.Instance.ReservePrefab(offspringName, offspring);
            }

            return true;
        }

        //------------------------------------------------
        // VALIDATE PREFAB
        //------------------------------------------------

        public override bool ValidatePrefab(string offspringName, OffspringData data)
        {
            var model = $"{nameof(OffspringData)}.{offspringName}";
            var error = false;

            var offspring = PrefabRegistry.Instance.GetReservedPrefab(offspringName);
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

            if (data.Growup != null && data.Components.Growup == ComponentBehavior.Patch)
            {
                // we already validated data.Growup.Grown != null
                foreach (var (grownData, i) in data.Growup.Grown.Select((value, i) => (value, i)))
                {
                    if (!PrefabRegistry.Instance.PrefabExists(grownData.Prefab))
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

        public override void RegisterPrefab(string offspringName, OffspringData data)
        {
            var model = $"{nameof(OffspringData)}.{offspringName}";

            if (PrefabRegistry.IsCustomPrefab(offspringName))
            {
                Plugin.LogServerDebug($"{model}: Registering prefab");
                var offspring = PrefabRegistry.Instance.GetReservedPrefab(offspringName);
                PrefabManager.Instance.RegisterToZNetScene(offspring);
            }
        }

        //------------------------------------------------
        // EDIT PREFAB
        //------------------------------------------------

        public override void EditPrefab(string offspringName, OffspringData data)
        {
            var model = $"{nameof(OffspringData)}.{offspringName}";

            var offspring = PrefabRegistry.Instance.GetReservedPrefab(offspringName);

            if (PrefabRegistry.IsCustomPrefab(offspringName))
            {
                PrepareClone(offspringName, data, offspring);
            }

            //
            // Growup
            //

            if (data.Components.Growup == ComponentBehavior.Patch)
            {
                if (data.Growup != null)
                {

                    var offspringGrowup = PrefabRegistry.Instance.GetOrAddComponent<Growup>(offspringName, offspring);
                    Plugin.LogServerDebug($"{model}.{nameof(data.Growup)}: Setting Growup values");

                    if (data.Growup.GrowTime != null) offspringGrowup.m_growTime = (float)data.Growup.GrowTime;
                    if (data.Growup.InheritTame != null) offspringGrowup.m_inheritTame = (bool)data.Growup.InheritTame;
                    
                    offspringGrowup.m_grownPrefab = null; // never use explicite prefab, always use list - stick to the system
                    offspringGrowup.m_altGrownPrefabs = new List<Growup.GrownEntry>();
                    foreach (var grownData in data.Growup.Grown)
                    {
                        offspringGrowup.m_altGrownPrefabs.Add(new Growup.GrownEntry
                        {
                            m_prefab = PrefabRegistry.Instance.GetOriginalPrefab(grownData.Prefab),
                            m_weight = grownData.Weight,
                        });
                    }

                }
            }
            else if (data.Components.Growup == ComponentBehavior.Remove)
            {
                Plugin.LogServerDebug($"{model}.{nameof(Growup)}: Removing Growup component (if exist)");
                PrefabRegistry.Instance.DestroyComponentIfExists<Growup>(offspringName, offspring);
            }

        }

        private void PrepareClone(string offspringName, OffspringData data, UnityEngine.GameObject offspring)
        {
            var model = $"{nameof(OffspringData)}.{offspringName}";

            PrefabRegistry.Instance.DestroyComponentIfExists<Procreation>(offspringName, offspring); // offsprings do not procreate
            PrefabRegistry.Instance.DestroyComponentIfExists<Tameable>(offspringName, offspring); // offsprings cannot be explicite tamed

            //PrefabRegistry.Instance.DestroyComponentIfExists<CharacterDrop>(offspringName, offspring);
            if (offspring.TryGetComponent<CharacterDrop>(out var charDrop))
            {
                foreach(var drop in charDrop.m_drops)
                {
                    var isTrophy = drop.m_prefab.name.StartsWith("trophy", StringComparison.OrdinalIgnoreCase);
                    var isSpecial = drop.m_onePerPlayer || isTrophy;
                    if (isSpecial)
                    {
                        drop.m_amountMin = 0;
                        drop.m_amountMax = 0;
                        drop.m_chance = 0;
                    }
                    else
                    {
                        drop.m_amountMin = 0;
                        drop.m_chance /= 2;
                        drop.m_levelMultiplier = false;
                        drop.m_dontScale = true;
                        if (drop.m_amountMax > 1)
                        {
                            drop.m_amountMax = (int)(((float)drop.m_amountMax / 2) + 0.5f);
                        }
                    }
                }
            }






            if (offspring.TryGetComponent<MonsterAI>(out var monsterAI))
            {
                // BaseAI fields
                var baseAISnapshot = new Common.FieldsSnapshot<BaseAI>(monsterAI);

                // MonsterAI fields
                var m_avoidLand = monsterAI.m_avoidLand;
                var m_fleeInLava = monsterAI.m_fleeInLava;

                PrefabRegistry.Instance.DestroyComponentIfExists<MonsterAI>(offspringName, offspring);
                var animalAI = PrefabRegistry.Instance.GetOrAddComponent<AnimalAI>(offspringName, offspring);

                // BaseAI fields
                baseAISnapshot.ApplyTo(animalAI);

                // MonsterAI fields
                var animalAITrait = AnimalAITrait.GetOrAddComponent(offspring);
                animalAITrait.m_avoidLand = m_avoidLand;
                animalAITrait.m_fleeInLava = m_fleeInLava;

            }

            //
            // display higher level creatures always as level 1 creature
            //

            var levelFx = offspring.GetComponentInChildren<LevelEffects>(true);
            if (levelFx != null)
            {
                UnityEngine.Object.Destroy(levelFx);
            }


            //
            // debug/remove effects
            //

            var removedEffects = new List<string>();

            var debugEffects = data.Clone.DebugEffects == true && ZNet.instance.IsServer();
            if (debugEffects) Plugin.LogServerMessage($"{model}: DebugEffects");

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
                        Plugin.LogServerMessage($"  {nameof(FootStep)} ({effect.m_motionType})");

                    if (removeSet == null)
                    {
                        if (debugEffects)
                        {
                            foreach (var p in effect.m_effectPrefabs)
                                Plugin.LogServerMessage($"  - {p.name}");
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
                            Plugin.LogServerMessage(remove
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


            //
            // character
            //

            Plugin.LogServerDebug($"{model}.{nameof(data.Clone)}: Setting Character values");
            var offspringCharacter = offspring.GetComponent<Character>();

            offspringCharacter.m_boss = false;
            offspringCharacter.m_bossEvent = "";
            offspringCharacter.m_name = data.Clone.Name;
            if (data.Clone.MaxHealthFactor.HasValue)
            {
                offspringCharacter.m_health = offspringCharacter.m_health * data.Clone.MaxHealthFactor.Value;
            }

            //
            // scaling
            //

            if (data.Clone.Scale.HasValue)
            {
                var setScale = data.Clone.Scale.Value;

                Plugin.LogServerDebug($"{model}.{nameof(data.Clone)}: Setting custom scaling to {setScale}");

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

                //offspringCharacter.m_turnSpeed /= setScale; // dont use this
                //offspringCharacter.m_runTurnSpeed /= setScale; // dont use this
                //offspringCharacter.m_swimTurnSpeed /= setScale; // dont use this
                //offspringCharacter.m_flyTurnSpeed /= setScale; // dont use this

                var col = offspring.GetComponent<UnityEngine.CapsuleCollider>();
                if (col)
                {
                    //col.height *= setScale; // dont use this because the height will already get scaled. additional scaling will shrink the collision-box for hover-text
                    //col.radius *= setScale; // dont use this because the radius will already get scaled. additional scaling will shrink the collision-box for hover-text
                    col.center *= setScale;
                }

                Plugin.LogServerDebug($"{model}.{nameof(data.Clone)}: Setting vfx scaling");
                OTABUtils.VfxUtils.ScaleVfx(offspring, setScale); // scale model particles

                Plugin.LogServerDebug($"{model}.{nameof(data.Clone)}: Setting effects scaling");

                var scaler = PrefabRegistry.Instance.GetOrAddComponent<ScaledCreature>(offspringName, offspring);
                scaler.m_effectScale = setScale * setScale; // for some reasons setScale*setScale is required
                scaler.m_animationScale = 1 / setScale; // because we are using it as a multiplier
                foreach (var eff in offspringCharacter.m_deathEffects.m_effectPrefabs)
                {
                    eff.m_scale = true; // important
                    eff.m_inheritParentScale = true; // important
                    eff.m_multiplyParentVisualScale = false;
                }

            }
        }

        //------------------------------------------------
        // FINALIZE
        //------------------------------------------------

        public override void FinalizeProcess()
        {
        }

        //------------------------------------------------
        // UNREGISTER PREFAB
        //------------------------------------------------

        public override void RestorePrefab(string offspringName)
        {
            PrefabRegistry.Instance.RestorePrefab(offspringName, (current, backup) => {
                //
            });
        }

        //------------------------------------------------
        // CLEANUP
        //------------------------------------------------

        public override void CleanupProcess()
        {
        }

    }

}
