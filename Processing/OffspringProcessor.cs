using Jotunn.Managers;
using OfTamingAndBreeding.Components;
using OfTamingAndBreeding.Components.Traits;
using OfTamingAndBreeding.Data.Models;
using OfTamingAndBreeding.Data.Models.SubData;
using OfTamingAndBreeding.Processing.Core;
using OfTamingAndBreeding.Registry;
using OfTamingAndBreeding.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OfTamingAndBreeding.Processing
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
            var valid = true;

            if (data.Clone != null)
            {
                if (data.Clone.Name == null)
                {
                    Plugin.LogError($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.Name)}: Missing field");
                    valid = false;
                }
                if (data.Clone.MaxHealthFactor.HasValue)
                {
                    if (data.Clone.MaxHealthFactor.Value <= 0)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.MaxHealthFactor)}: Zero or negative values not allowed");
                        valid = false;
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
                        valid = false;
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
                    valid = false;
                }
                else
                {
                    foreach (var (grownData, i) in data.Growup.Grown.Select((value, i) => (value, i)))
                    {
                        grownData.Weight = Math.Max(0f, grownData.Weight);
                        if (grownData.Prefab == null)
                        {
                            Plugin.LogError($"{model}.{nameof(data.Growup)}.{nameof(data.Growup.Grown)}.{i}.{nameof(grownData.Prefab)}: Field is empty");
                            valid = false;
                        }
                    }
                }
            }

            return valid;
        }

        //------------------------------------------------
        // RESERVE PREFAB
        //------------------------------------------------

        public override bool ReservePrefab(string offspringName, OffspringFile data)
        {
            var model = $"{nameof(OffspringFile)}.{offspringName}";
            var valid = true;

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
                        valid = false;
                    }

                    if (data.Clone.From == null)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.From)}: Field missing");
                        valid = false;
                    }

                    if (OTABPrefabRegistry.IsCustomPrefab(data.Clone.From))
                    {
                        Plugin.LogError($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.From)}: Cannot clone from cloned prefab '{data.Clone.From}'");
                        valid = false;
                    }

                    var cloneFrom = OTABPrefabRegistry.Instance.GetOriginalPrefab(data.Clone.From);
                    if (!cloneFrom)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.From)}: Prefab '{data.Clone.From}' not found");
                        valid = false;
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
                    if (!offspring)
                    {
                        return false;
                    }
                }
                else
                {
                    OTABPrefabRegistry.Instance.MakeOriginalBackup(offspringName);
                }

                if (valid)
                {
                    OTABPrefabRegistry.Instance.ReservePrefab(offspringName, offspring);
                }
            }

            return valid;
        }

        //------------------------------------------------
        // VALIDATE PREFAB
        //------------------------------------------------

        public override bool ValidatePrefab(string offspringName, OffspringFile data)
        {
            var model = $"{nameof(OffspringFile)}.{offspringName}";
            var valid = true;

            var offspring = OTABPrefabRegistry.Instance.GetReservedPrefab(offspringName);
            if (!offspring)
            {
                Plugin.LogError($"{model}: Prefab not found");
                valid = false;
            }
            else
            {
                if (!offspring.GetComponent<Character>())
                {
                    Plugin.LogError($"{model}: Prefab has no Character");
                    valid = false;
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
                        valid = false;
                    }
                }
            }

            return valid;
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

        public override bool ProcessPrefab(string offspringName, OffspringFile data)
        {
            var model = $"{nameof(OffspringFile)}.{offspringName}";
            var valid = true;

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

            return valid;
        }

        private void PrepareClone(string offspringName, OffspringFile data, UnityEngine.GameObject offspring)
        {
            var model = $"{nameof(OffspringFile)}.{offspringName}";

            OTABPrefabRegistry.Instance.DestroyComponentIfExists<Procreation>(offspringName, offspring); // offsprings do not procreate
            OTABPrefabRegistry.Instance.DestroyComponentIfExists<Tameable>(offspringName, offspring); // offsprings cannot be explicite tamed, can be readded by using Creature Processing

            if (offspring.TryGetComponent<CharacterDrop>(out var charDrop))
            {
                charDrop.m_drops = charDrop.m_drops
                    .Select(drop => new CharacterDrop.Drop
                    {
                        m_prefab = drop.m_prefab,
                        m_amountMin = drop.m_amountMin,
                        m_amountMax = drop.m_amountMax,
                        m_chance = drop.m_chance,
                        m_onePerPlayer = drop.m_onePerPlayer,
                        m_levelMultiplier = drop.m_levelMultiplier,
                        m_dontScale = drop.m_dontScale,
                    })
                    .ToList();

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






            if (offspring.TryGetComponent<MonsterAI>(out var monsterAI)) // todo: maybe add yaml option to allow offsprings with monster ai
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
                levelFx.enabled = false;
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
            // character / baseai
            //

            Plugin.LogDebug($"{model}.{nameof(data.Clone)}: Setting Character values");
            var offspringCharacter = offspring.GetComponent<Character>();
            var offspringBaseAI = offspring.GetComponent<BaseAI>();

            offspringCharacter.m_boss = false;
            offspringCharacter.m_bossEvent = "";
            offspringBaseAI.m_spawnMessage = "";
            offspringBaseAI.m_deathMessage = "";

            var comp1 = offspring.GetComponent<MovementDamage>();
            if (comp1)
            {
                // disable faders walk damage
                comp1.enabled = false;
                if (comp1.m_runDamageObject)
                {
                    comp1.m_runDamageObject.SetActive(false);
                }
            }




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

                offspring.transform.localScale *= setScale;




                offspringCharacter.m_speed *= setScale;

                offspringCharacter.m_crouchSpeed *= setScale;
                offspringCharacter.m_walkSpeed *= setScale;
                offspringCharacter.m_runSpeed *= setScale;
                offspringCharacter.m_swimSpeed *= setScale;
                offspringCharacter.m_flySlowSpeed *= setScale;
                offspringCharacter.m_flyFastSpeed *= setScale;

                Plugin.LogDebug($"{model}.{nameof(data.Clone)}: Setting vfx scaling");
                VfxUtils.ScaleVfx(offspring, setScale); // scale model particles





                Plugin.LogDebug($"{model}.{nameof(data.Clone)}: Setting death effects scaling");
                offspringCharacter.m_deathEffects = EffectUtils.CloneEffectList(offspringCharacter.m_deathEffects);
                foreach (var eff in offspringCharacter.m_deathEffects.m_effectPrefabs)
                {
                    var originalEffect = eff.m_prefab;

                    var clonedEffectName = $"{offspringName}_{originalEffect.name}";
                    var clonedEffect = PrefabManager.Instance.GetPrefab(clonedEffectName);

                    if (clonedEffect == null)
                    {
                        clonedEffect = PrefabManager.Instance.CreateClonedPrefab(
                            clonedEffectName,
                            originalEffect.name
                        );
                    }
                    else
                    {
                        VfxUtils.RestoreVfx(clonedEffect, originalEffect);
                    }
                    VfxUtils.ScaleVfx(clonedEffect, setScale);
                    eff.m_prefab = clonedEffect;

                    // ragdoll

                    var ragdoll = clonedEffect.GetComponent<Ragdoll>();
                    var originalRagdoll = originalEffect.GetComponent<Ragdoll>();

                    if (ragdoll && originalRagdoll)
                    {
                        ragdoll.transform.localScale = originalRagdoll.transform.localScale * setScale;
                        ragdoll.m_removeEffect = EffectUtils.CloneEffectList(originalRagdoll.m_removeEffect);

                        foreach (var eff2 in ragdoll.m_removeEffect.m_effectPrefabs)
                        {
                            var originalRemoveEffect = eff2.m_prefab;

                            var clonedRemoveEffectName = $"{offspringName}_{originalEffect.name}_{originalRemoveEffect.name}";
                            var clonedRemoveEffect = PrefabManager.Instance.GetPrefab(clonedRemoveEffectName);
                            if (clonedRemoveEffect == null)
                            {
                                clonedRemoveEffect = PrefabManager.Instance.CreateClonedPrefab(clonedRemoveEffectName, originalRemoveEffect.name);
                            }
                            else
                            {
                                VfxUtils.RestoreVfx(clonedRemoveEffect, originalRemoveEffect
                                );
                            }

                            VfxUtils.ScaleVfx(clonedRemoveEffect, setScale);
                            eff2.m_prefab = clonedRemoveEffect;
                        }
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
             * just testing to make a creature pickable
             * 
             * do not remove, do not uncomment
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

                current.transform.localScale = backup.transform.localScale;

                var currentLodGroup = current.transform.Find("Visual")?.GetComponent<LODGroup>();
                var backupLodGroup = backup.transform.Find("Visual")?.GetComponent<LODGroup>();
                if (currentLodGroup && backupLodGroup)
                {
                    currentLodGroup.size = backupLodGroup.size;
                }



                var currentGrowup = current.GetComponent<Growup>();
                var backupGrowup = backup.GetComponent<Growup>();
                var currentFootStep = current.GetComponent<FootStep>();
                var backupFootStep = backup.GetComponent<FootStep>();
                var currentCharacterDrop = current.GetComponent<CharacterDrop>();
                var backupCharacterDrop = backup.GetComponent<CharacterDrop>();
                var currentMonsterAI = current.GetComponent<MonsterAI>();
                var backupMonsterAI = backup.GetComponent<MonsterAI>();
                var currentCharacter = current.GetComponent<Character>();
                var backupCharacter = backup.GetComponent<Character>();

                if (currentGrowup && backupGrowup)
                {
                    currentGrowup.m_grownPrefab = backupGrowup.m_grownPrefab;
                    currentGrowup.m_altGrownPrefabs = backupGrowup.m_altGrownPrefabs;
                }

                if (currentFootStep && backupFootStep)
                {
                    currentFootStep.m_effects = backupFootStep.m_effects;
                }

                if (currentCharacterDrop && backupCharacterDrop)
                {
                    currentCharacterDrop.m_drops = backupCharacterDrop.m_drops;
                }

                if (currentMonsterAI && backupMonsterAI)
                {
                    // BaseAI references
                    currentMonsterAI.m_onBecameAggravated = backupMonsterAI.m_onBecameAggravated;
                    currentMonsterAI.m_alertedEffects = backupMonsterAI.m_alertedEffects;
                    currentMonsterAI.m_idleSound = backupMonsterAI.m_idleSound;

                    // MonsterAI references
                    currentMonsterAI.m_onConsumedItem = backupMonsterAI.m_onConsumedItem;
                    currentMonsterAI.m_wakeupEffects = backupMonsterAI.m_wakeupEffects;
                    currentMonsterAI.m_sleepEffects = backupMonsterAI.m_sleepEffects;
                    currentMonsterAI.m_consumeItems = backupMonsterAI.m_consumeItems;
                }

                if (currentCharacter && backupCharacter)
                {
                    currentCharacter.m_deathEffects = backupCharacter.m_deathEffects;
                }

                var currentLevelFx = current.GetComponentInChildren<LevelEffects>(true);
                var backupLevelFx = backup.GetComponentInChildren<LevelEffects>(true);
                if (currentLevelFx && backupLevelFx)
                {
                    currentLevelFx.enabled = backupLevelFx.enabled;
                }





                var currentMovementDamage = current.GetComponent<MovementDamage>();
                var backupMovementDamage = backup.GetComponent<MovementDamage>();
                if (currentMovementDamage && backupMovementDamage)
                {
                    currentMovementDamage.enabled = backupMovementDamage.enabled;
                    if (currentMovementDamage.m_runDamageObject && backupMovementDamage.m_runDamageObject)
                    {
                        currentMovementDamage.m_runDamageObject.SetActive(backupMovementDamage.m_runDamageObject.activeSelf);
                    }
                }





                VfxUtils.RestoreVfx(current, backup);

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




