using Jotunn.Managers;
using OfTamingAndBreeding.Components;
using OfTamingAndBreeding.Components.Core;
using OfTamingAndBreeding.Components.Traits;
using OfTamingAndBreeding.Data.Models;
using OfTamingAndBreeding.Data.Models.SubData;
using OfTamingAndBreeding.Utilities;
using OfTamingAndBreeding.Processing.Core;
using OfTamingAndBreeding.ValheimAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using YamlDotNet.Core;

namespace OfTamingAndBreeding.Registry.Processing
{
    internal class OffspringProcessor : DataProcessor<OffspringFile>
    {

        public override string DirectoryName => OffspringFile.DirectoryName;

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

        public override bool ValidateData(string offspringName, OffspringFile data)
        {
            var model = $"{nameof(OffspringFile)}.{offspringName}";
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
                        Plugin.LogWarning($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.MaxHealthFactor)}: Offspring is getting increased max health");
                    }
                }
                if (data.Clone.RemoveEffects != null && data.Clone.RemoveEffects.Length == 0)
                {
                    Plugin.LogWarning($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.RemoveEffects)}: List is empty and will be set to null");
                    data.Clone.RemoveEffects = null;
                }
                if (data.Clone.Scale.HasValue)
                {
                    if (data.Clone.Scale.Value <= 0)
                    {
                        Plugin.LogWarning($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.Scale)}: Negative or zero value not allowed - setting to null");
                        data.Clone.Scale = null;
                    }
                    else if (data.Clone.Scale.Value == 1)
                    {
                        Plugin.LogWarning($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.Scale)}: Value of 1 will do nothing - setting to null");
                        data.Clone.Scale = null;
                    }
                }
                if (data.Clone.MaxHealthFactor.HasValue)
                {
                    if (data.Clone.MaxHealthFactor.Value < 0)
                    {
                        // maybe 0% could be fun? just allow it for now
                        Plugin.LogWarning($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.MaxHealthFactor)}: Negative value not allowed - setting to null");
                        data.Clone.MaxHealthFactor = null;
                    }
                    else if (data.Clone.MaxHealthFactor.Value == 1)
                    {
                        Plugin.LogWarning($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.MaxHealthFactor)}: Value of 1 will do nothing - setting to null");
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
                        Plugin.LogWarning($"{model}.{nameof(data.Components)}.{nameof(data.Components.Growup)}({nameof(ComponentBehavior.Inherit)}): Component data will be ignored");
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

        public override bool ReservePrefab(string offspringName, OffspringFile data)
        {
            var model = $"{nameof(OffspringFile)}.{offspringName}";

            var offspring = OTABPrefabRegistry.Instance.GetReservedPrefab(offspringName);
            if (offspring == null)
            {
                var custom = OTABPrefabRegistry.Instance.GetCustomPrefab(offspringName);
                offspring = OTABPrefabRegistry.Instance.GetOriginalPrefab(offspringName);
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

                    if (OTABPrefabRegistry.IsCustomPrefab(data.Clone.From))
                    {
                        Plugin.LogError($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.From)}: Cannot clone from cloned prefab '{data.Clone.From}'");
                        return false;
                    }

                    var cloneFrom = OTABPrefabRegistry.Instance.GetOriginalPrefab(data.Clone.From);
                    if (!cloneFrom)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.From)}: Prefab '{data.Clone.From}' not found");
                        return false;
                    }

                    if (custom == null)
                    {
                        // not cloned yet
                        offspring = OTABPrefabRegistry.Instance.CreateCustomPrefab(offspringName, cloneFrom.name);
                    }
                    else
                    {
                        // previously cloned - reactivate
                        Plugin.LogDebug($"{model}.{nameof(data.Clone)}: Reactivating cloned prefab for '{cloneFrom.name}'");
                        offspring = OTABPrefabRegistry.Instance.ReactivateCustomPrefab(offspringName, cloneFrom.name);
                    }
                }
                else
                {
                    OTABPrefabRegistry.Instance.MakeOriginalBackup(offspringName);
                }

                OTABPrefabRegistry.Instance.ReservePrefab(offspringName, offspring);
            }

            return true;
        }

        //------------------------------------------------
        // VALIDATE PREFAB
        //------------------------------------------------

        public override bool ValidatePrefab(string offspringName, OffspringFile data)
        {
            var model = $"{nameof(OffspringFile)}.{offspringName}";
            var error = false;

            var offspring = OTABPrefabRegistry.Instance.GetReservedPrefab(offspringName);
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
                    if (!OTABPrefabRegistry.Instance.PrefabExists(grownData.Prefab))
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

        public override void RegisterPrefab(string offspringName, OffspringFile data)
        {
            var model = $"{nameof(OffspringFile)}.{offspringName}";

            if (OTABPrefabRegistry.IsCustomPrefab(offspringName))
            {
                Plugin.LogDebug($"{model}: Registering prefab");
                var offspring = OTABPrefabRegistry.Instance.GetReservedPrefab(offspringName);
                PrefabManager.Instance.RegisterToZNetScene(offspring);
            }
        }

        //------------------------------------------------
        // EDIT PREFAB
        //------------------------------------------------

        public override void EditPrefab(string offspringName, OffspringFile data)
        {
            var model = $"{nameof(OffspringFile)}.{offspringName}";

            var offspring = OTABPrefabRegistry.Instance.GetReservedPrefab(offspringName);

            if (OTABPrefabRegistry.IsCustomPrefab(offspringName))
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

                    var offspringGrowup = OTABPrefabRegistry.Instance.GetOrAddComponent<Growup>(offspringName, offspring);
                    Plugin.LogDebug($"{model}.{nameof(data.Growup)}: Setting Growup values");

                    if (data.Growup.GrowTime != null) offspringGrowup.m_growTime = (float)data.Growup.GrowTime;
                    if (data.Growup.InheritTame != null) offspringGrowup.m_inheritTame = (bool)data.Growup.InheritTame;

                    offspringGrowup.m_grownPrefab = null; // never use explicite prefab, always use list - stick to the system
                    offspringGrowup.m_altGrownPrefabs = new List<Growup.GrownEntry>();
                    foreach (var grownData in data.Growup.Grown)
                    {
                        offspringGrowup.m_altGrownPrefabs.Add(new Growup.GrownEntry
                        {
                            m_prefab = OTABPrefabRegistry.Instance.GetOriginalPrefab(grownData.Prefab),
                            m_weight = grownData.Weight,
                        });
                    }

                }
            }
            else if (data.Components.Growup == ComponentBehavior.Remove)
            {
                Plugin.LogDebug($"{model}.{nameof(Growup)}: Removing Growup component (if exist)");
                OTABPrefabRegistry.Instance.DestroyComponentIfExists<Growup>(offspringName, offspring);
            }

        }

        private void PrepareClone(string offspringName, OffspringFile data, UnityEngine.GameObject offspring)
        {
            var model = $"{nameof(OffspringFile)}.{offspringName}";

            OTABPrefabRegistry.Instance.DestroyComponentIfExists<Procreation>(offspringName, offspring); // offsprings do not procreate
            OTABPrefabRegistry.Instance.DestroyComponentIfExists<Tameable>(offspringName, offspring); // offsprings cannot be explicite tamed

            //PrefabRegistry.Instance.DestroyComponentIfExists<CharacterDrop>(offspringName, offspring);
            if (offspring.TryGetComponent<CharacterDrop>(out var charDrop))
            {
                foreach (var drop in charDrop.m_drops)
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

                OTABPrefabRegistry.Instance.DestroyComponentIfExists<MonsterAI>(offspringName, offspring);
                var animalAI = OTABPrefabRegistry.Instance.GetOrAddComponent<AnimalAI>(offspringName, offspring);

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
                    if (debugEffects) Plugin.LogMessage($"  {nameof(FootStep)} ({effect.m_motionType})");

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


            //
            // character
            //

            Plugin.LogDebug($"{model}.{nameof(data.Clone)}: Setting Character values");
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

                //offspringCharacter.m_turnSpeed /= setScale; // dont use this
                //offspringCharacter.m_runTurnSpeed /= setScale; // dont use this
                //offspringCharacter.m_swimTurnSpeed /= setScale; // dont use this
                //offspringCharacter.m_flyTurnSpeed /= setScale; // dont use this

                Plugin.LogDebug($"{model}.{nameof(data.Clone)}: Setting vfx scaling");
                Utilities.VfxUtils.ScaleVfx(offspring, setScale); // scale model particles

                var col = offspring.GetComponent<UnityEngine.CapsuleCollider>();
                if (col)
                {
                    // not used anymore
                    // delete, if unneccessary
                    //col.height *= setScale; // dont use this because the height will already get scaled. additional scaling will shrink the collision-box for hover-text
                    //col.radius *= setScale; // dont use this because the radius will already get scaled. additional scaling will shrink the collision-box for hover-text
                    //col.center *= setScale;
                }

                Plugin.LogDebug($"{model}.{nameof(data.Clone)}: Setting effects scaling");
                foreach (var eff in offspringCharacter.m_deathEffects.m_effectPrefabs)
                {

                    var clonedEffectName = $"{offspringName}_{eff.m_prefab.gameObject.name}";
                    var clonedEffect = PrefabManager.Instance.GetPrefab(clonedEffectName);
                    if (clonedEffect == null)
                    {
                        clonedEffect = PrefabManager.Instance.CreateClonedPrefab(clonedEffectName, eff.m_prefab.gameObject.name);
                    }
                    eff.m_prefab = clonedEffect;

                    var ragdoll = eff.m_prefab.GetComponent<Ragdoll>();
                    if (ragdoll)
                    {
                        ragdoll.transform.localScale *= setScale;
                        foreach (var eff2 in ragdoll.m_removeEffect.m_effectPrefabs)
                        {
                            Utilities.VfxUtils.ScaleVfx(eff2.m_prefab, setScale);
                        }
                    }
                    else
                    {
                        Utilities.VfxUtils.ScaleVfx(eff.m_prefab, setScale);
                    }
                }

                var scaler = OTABPrefabRegistry.Instance.GetOrAddComponent<ScaledCreature>(offspringName, offspring);
                scaler.m_animationScale = 1 / setScale;

                // do not hide the creature if we just go some steps away
                var lodGroup = offspringCharacter.transform.Find("Visual")?.GetComponent<LODGroup>();
                if (lodGroup)
                {
                    // todo: is this beeing restored correctly?
                    lodGroup.size /= setScale;
                }
            }



            /*
             * testing to make a creature pickable
             * 
            if (offspringName == "OTAB_Bat_pup")
            {
                var itemDrop = PrefabRegistry.Instance.GetOrAddComponent<ItemDrop>(
                    offspringName,
                    offspring
                );

                itemDrop.m_itemData = new ItemDrop.ItemData();
                itemDrop.m_itemData.m_shared = new ItemDrop.ItemData.SharedData
                {
                    m_name = "$OTAB_enemy_bat_pup",
                    m_description = ",
                    m_itemType = ItemDrop.ItemData.ItemType.Misc,
                    m_maxStackSize = 1,
                    m_maxQuality = 1,
                    m_weight = 2f,
                    m_teleportable = true,
                };

                itemDrop.m_itemData.m_stack = 1;
                itemDrop.m_itemData.m_quality = 1;
                itemDrop.m_itemData.m_variant = 0;
            }
            */

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
            OTABPrefabRegistry.Instance.RestorePrefab(offspringName, (current, backup) => {
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




