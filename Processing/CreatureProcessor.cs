using Jotunn.Managers;
using OfTamingAndBreeding.Components;
using OfTamingAndBreeding.Components.Extensions;
using OfTamingAndBreeding.Components.Traits;
using OfTamingAndBreeding.Data.Models;
using OfTamingAndBreeding.Processing.Core;
using OfTamingAndBreeding.Processing.Registry;
using OfTamingAndBreeding.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OfTamingAndBreeding.Processing
{
    internal partial class CreatureProcessor : DataProcessor<CreatureFile>
    {

        public override string DirectoryName => CreatureFile.DirectoryName;

        public override string PrefabTypeName => "creature";

        public override string GetDataKey(string filePath) => null;

        public override bool LoadFromFile(string filePath) => LoadFromYamlFile(filePath);

        //------------------------------------------------

        public override bool PrepareProcess()
        {
            return true;
        }

        public override bool ReservePrefabName(string creatureName)
        {
            if (!OTABPrefabRegistry.Instance.ReservePrefabName(creatureName))
            {
                var model = $"{nameof(CreatureFile)}.{creatureName}";
                Plugin.LogError($"{model}: Prefab is already reserved");
                return false;
            }
            return true;
        }

        public override bool ValidateData(string creatureName, CreatureFile data)
        {
            var model = $"{nameof(CreatureFile)}.{creatureName}";
            var valid = true;

            // ---------------------------
            // Clone
            // ---------------------------

            //var registeredPrefab = OTABPrefabRegistry.Instance.GetRegisteredPrefab(creatureName);
            var isOriginalPrefab = OTABPrefabRegistry.IsCustomPrefab(creatureName) == false;

            if (data.Clone == null)
            {
                // we dont want to clone
                // but we need to check if original exists
                if (!isOriginalPrefab)
                {
                    Plugin.LogError($"{model}: Prefab not found - Field '{nameof(data.Clone)}' missing?");
                    valid = false;
                }
            }
            else
            {
                if (isOriginalPrefab)
                {
                    Plugin.LogError($"{model}.{nameof(data.Clone)}: Cannot create cloned prefab with name '{creatureName}' because it already exists.");
                    valid = false;
                }
                
                if (string.IsNullOrEmpty(data.Clone.From))
                {
                    Plugin.LogError($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.From)}: Missing field");
                    valid = false;
                }
                else
                {
                    if (OTABPrefabRegistry.IsCustomPrefab(data.Clone.From))
                    {
                        Plugin.LogError($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.From)}: Source '{data.Clone.From}' needs to be valid original prefab");
                        valid = false;
                    }
                }

                if (data.Clone.Scale.HasValue)
                {
                    if (data.Clone.Scale.Value <= 0)
                    {
                        Plugin.LogWarning($"{model}.{nameof(data.Clone)}.{nameof(data.Clone.Scale)}: Negative or zero value not allowed");
                        valid = false;
                    }
                    else if (data.Clone.Scale.Value == 1)
                    {
                        data.Clone.Scale = null; // just dont scale at all
                    }
                }
            }

            if (!valid)
            {
                // cloning needs to be valid first!
                return false;
            }

            // ---------------------------

            var sourceName = data.Clone?.From ?? creatureName;
            var source = OTABPrefabRegistry.Instance.GetRegisteredPrefab(sourceName);
            if (!source.GetComponent<Character>())
            {
                Plugin.LogError($"{model}: Prefab has no Character");
                return false;
            }

            var hasProcreation = (bool)source.GetComponent<Procreation>();
            var hasTameable = (bool)source.GetComponent<Tameable>();

            var hasMonsterAI = (bool)source.GetComponent<MonsterAI>();
            var hasAnimalAI = (bool)source.GetComponent<AnimalAI>();
            if (hasMonsterAI && data.Clone?.RemoveMonsterAI == true)
            {
                (hasMonsterAI, hasAnimalAI) = (hasAnimalAI, hasMonsterAI);
            }

            // ---------------------------
            // MonsterAI / AnimalAI
            // ---------------------------

            CreatureFile.BaseAIData data_BaseAI = null;
            string data_BaseAI_name = "";

            if (hasMonsterAI)
            {
                data_BaseAI = data.MonsterAI;
                data_BaseAI_name = nameof(data.MonsterAI);
            }
            else if (hasAnimalAI)
            {
                data_BaseAI = data.AnimalAI;
                data_BaseAI_name = nameof(data.AnimalAI);
            }
            else
            {
                Plugin.LogError($"{model}: Prefab has neither MonsterAI nor AnimalAI");
                valid = false;
            }

            if (valid == false)
            {
                return false;
            }

            // ---------------------------
            // Character
            // ---------------------------

            if (data.Character != null)
            {
                // nothing to validate here
            }

            // ---------------------------
            // Tameable
            // ---------------------------

            if (data.Tameable != null)
            {
                if (data.Tameable.FeedingDisabled == true)
                {
                    if (data.Tameable.FedDuration.HasValue)
                    {
                        Plugin.LogWarning($"{model}.{nameof(data.Tameable)}.{nameof(data.Tameable.FedDuration)}: Field will be ignored because {nameof(data.Tameable.FeedingDisabled)} is true");
                    }
                }
                else
                {
                    if (data.Tameable.FedDuration.HasValue && data.Tameable.FedDuration <= 0)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Tameable)}.{nameof(data.Tameable.FedDuration)}: Zero or negative values not allowed");
                        valid = false;
                    }
                }
                if (data.Tameable.TamingDisabled == true)
                {
                    if (data.Tameable.TamingTime.HasValue)
                    {
                        Plugin.LogWarning($"{model}.{nameof(data.Tameable)}.{nameof(data.Tameable.TamingTime)}: Field will be ignored because {nameof(data.Tameable.TamingDisabled)} is true");
                    }
                    if (!string.IsNullOrEmpty(data.Tameable.RequireGlobalKey))
                    {
                        Plugin.LogWarning($"{model}.{nameof(data.Tameable)}.{nameof(data.Tameable.RequireGlobalKey)}: Field will be ignored because {nameof(data.Tameable.TamingDisabled)} is true");
                    }
                }
                else
                {
                    if (data.Tameable.TamingTime.HasValue && data.Tameable.TamingTime < 0)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Tameable)}.{nameof(data.Tameable.TamingTime)}: Negative values not allowed");
                        valid = false;
                    }
                }
            }

            // ---------------------------
            // Growup
            // ---------------------------

            if (data.Growup != null)
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
                        if (grownData.Prefab == null)
                        {
                            Plugin.LogError($"{model}.{nameof(data.Growup)}.{nameof(data.Growup.Grown)}.{i}.{nameof(grownData.Prefab)}: Field is empty");
                            valid = false;
                        }
                        else
                        {
                            if (OTABPrefabRegistry.Instance.PrefabWillExist(grownData.Prefab) == false)
                            {
                                Plugin.LogError($"{model}.{nameof(data.Growup)}.{nameof(data.Growup.Grown)}.{i}.{nameof(grownData.Prefab)}: '{grownData.Prefab}' not found");
                                valid = false;
                            }
                        }
                    }
                }

                if (data.Growup.RequireFeeding != null && !hasTameable && data.Tameable == null)
                {
                    Plugin.LogError($"{model}.{nameof(data.Growup)}.{nameof(data.Growup.RequireFeeding)}: Field requires Tameable component");
                    valid = false;
                }

            }

            // ---------------------------
            // Procreation
            // ---------------------------

            if (data.Procreation != null)
            {
                bool wantProcreationActive = hasProcreation || data.Procreation != null;
                bool wantTameableActive = hasTameable || data.Tameable != null;
                if (wantProcreationActive && !wantTameableActive)
                {
                    Plugin.LogError($"{model}.{nameof(data.Procreation)}: Component requires {nameof(data.Tameable)}");
                    valid = false;
                }

                if (data.Procreation.Partner != null)
                {
                    foreach (var (partnerName, i) in data.Procreation.Partner.Select((value, i) => (value, i)))
                    {
                        if (string.IsNullOrEmpty(partnerName))
                        {
                            Plugin.LogError($"{model}.{nameof(data.Procreation)}.{nameof(data.Procreation.Partner)}.{i}: Field is empty");
                            valid = false;
                        }
                        else
                        {
                            if (!OTABPrefabRegistry.Instance.PrefabWillExist(partnerName))
                            {
                                Plugin.LogError($"{model}.{nameof(data.Procreation)}.{nameof(data.Procreation.Partner)}.{i}: '{partnerName}' not found");
                                valid = false;
                            }
                        }
                    }
                }

                if (data.Procreation.Offspring == null || data.Procreation.Offspring.Length == 0)
                {
                    Plugin.LogError($"{model}.{nameof(data.Procreation)}.{nameof(data.Procreation.Offspring)}: Field is required but null or empty");
                    valid = false;
                }
                else
                {
                    foreach (var (offspringData, i) in data.Procreation.Offspring.Select((value, i) => (value, i)))
                    {
                        offspringData.Weight = Math.Max(0f, offspringData.Weight);
                        if (offspringData.Prefab == null)
                        {
                            Plugin.LogError($"{model}.{nameof(data.Procreation)}.{nameof(data.Procreation.Offspring)}.{i}.{nameof(offspringData.Prefab)}: Field is empty");
                            valid = false;
                        }
                    }
                }

                foreach (var (offspringData, i) in data.Procreation.Offspring.Select((value, i) => (value, i)))
                {

                    if (!OTABPrefabRegistry.Instance.PrefabWillExist(offspringData.Prefab))
                    {
                        Plugin.LogError($"{model}.{nameof(data.Procreation)}.{nameof(data.Procreation.Offspring)}.{i}.{nameof(offspringData.Prefab)}: '{offspringData.Prefab}' not found");
                        valid = false;
                    }

                    if (offspringData.NeedPartner == false && offspringData.NeedPartnerPrefab != null)
                    {
                        // todo: warning
                        offspringData.NeedPartnerPrefab = null;
                    }

                    if (offspringData.NeedPartnerPrefab != null)
                    {
                        if (!OTABPrefabRegistry.Instance.PrefabWillExist(offspringData.NeedPartnerPrefab))
                        {
                            Plugin.LogError($"{model}.{nameof(data.Procreation)}.{nameof(data.Procreation.Offspring)}.{i}.{nameof(offspringData.NeedPartnerPrefab)}: '{offspringData.NeedPartnerPrefab}' not found");
                            valid = false;
                        }
                    }
                }

                if (data.Procreation.MaxCreaturesCountPrefabs != null)
                {
                    foreach (var (prefabName, i) in data.Procreation.MaxCreaturesCountPrefabs.Select((value, i) => (value, i)))
                    {
                        if (!OTABPrefabRegistry.Instance.PrefabWillExist(prefabName))
                        {
                            Plugin.LogError($"{model}.{nameof(data.Procreation)}.{nameof(data.Procreation.MaxCreaturesCountPrefabs)}.{i}: '{prefabName}' not found");
                            valid = false;
                        }
                    }
                }

            }

            return valid;
        }

        //------------------------------------------------

        public override bool RegisterPrefab(string creatureName, CreatureFile data)
        {
            var model = $"{nameof(CreatureFile)}.{creatureName}";

            var creature = OTABPrefabRegistry.Instance.GetRegisteredPrefab(creatureName);
            var custom = OTABPrefabRegistry.Instance.GetCustomPrefab(creatureName);
            if (creature == null || custom != null) // not cloned yet / previously cloned, reactivate
            {

                if (data.Clone == null)
                {
                    // should have been validated already
                    return false;
                }

                if (custom == null)
                {
                    // not cloned yet
                    creature = OTABPrefabRegistry.Instance.CreateCustomPrefab(creatureName, data.Clone.From);
                }
                else
                {
                    // previously cloned - reactivate
                    Plugin.LogDebug($"{model}.{nameof(data.Clone)}: Reactivating cloned prefab for '{data.Clone.From}'");
                    creature = OTABPrefabRegistry.Instance.ReactivateCustomPrefab(creatureName, data.Clone.From);
                }

                if (creature)
                {
                    // its very important that we clone the prefab BEFORE the original gets modified
                    PrepareClone(creatureName, data, creature);

                    Plugin.LogDebug($"{model}: Registering custom prefab");
                    PrefabManager.Instance.RegisterToZNetScene(creature);
                }
            }
            else
            {
                OTABPrefabRegistry.Instance.MakeOriginalBackup(creatureName);
            }

            if (!creature)
            {
                Plugin.LogDebug($"{model}: Prefab '{creatureName}' not found");
                return false;
            }

            return true;
        }

        public override bool ProcessPrefab(string creatureName, CreatureFile data)
        {
            var model = $"{nameof(CreatureFile)}.{creatureName}";
            var valid = true;

            var creature = OTABPrefabRegistry.Instance.GetRegisteredPrefab(creatureName);

            var idleSoundPrefab = EffectUtils.FindEffectPrefab<BaseAI>(creatureName, "m_idleSound", 0);

            // ---------------------------
            // MonsterAI / AnimalAI
            // ---------------------------

            var monsterAI = creature.GetComponent<MonsterAI>();
            var animalAI = creature.GetComponent<AnimalAI>();

            CreatureFile.BaseAIData data_BaseAI = null;
            string data_BaseAI_name = "";

            if (monsterAI)
            {
                data_BaseAI = data.MonsterAI;
                data_BaseAI_name = nameof(data.MonsterAI);
            }
            else if (animalAI)
            {
                data_BaseAI = data.AnimalAI;
                data_BaseAI_name = nameof(data.AnimalAI);
            }
            else
            {
                Plugin.LogError($"{model}: Prefab has neither MonsterAI nor AnimalAI");
                return false;
            }

            if (data_BaseAI != null)
            {
                var baseAITrait = BaseAITrait.GetOrAddComponent(creature);

                BaseAITrait.ConsumeItem[] consumeItems = null;

                if (data_BaseAI.ConsumeItems != null)
                {
                    consumeItems = data_BaseAI.ConsumeItems
                        .OrderByDescending(i => i.FedDurationFactor)
                        .Select((ci, i) =>
                        {
                            var foodItem = OTABPrefabRegistry.Instance.GetCustomPrefab(ci.Prefab)
                                        ?? OTABPrefabRegistry.Instance.GetRegisteredPrefab(ci.Prefab);

                            if (foodItem == null)
                            {
                                Plugin.LogWarning($"{model}.{nameof(data.MonsterAI)}.{nameof(data.MonsterAI.ConsumeItems)}.{i}.{nameof(ci.Prefab)}: '{ci.Prefab}' not found");
                                return null;
                            }

                            var foodItemDrop = foodItem.GetComponent<ItemDrop>();
                            if (foodItemDrop == null)
                            {
                                Plugin.LogError($"{model}.{nameof(data.MonsterAI)}.{nameof(data.MonsterAI.ConsumeItems)}.{i}.{nameof(ci.Prefab)}: '{ci.Prefab}' has no ItemDrop component");
                                valid = false;
                                return null;
                            }

                            return new BaseAITrait.ConsumeItem
                            {
                                itemDrop = foodItemDrop,
                                fedDurationFactor = ci.FedDurationFactor,
                            };
                        })
                        .Where(ci => ci != null)
                        .ToArray();

                    baseAITrait.m_consumeItemsStoreIndex = BaseAITrait.s_consumeItemsStore.Add(consumeItems);
                }
                else
                {
                    if (monsterAI)
                    {
                        consumeItems = monsterAI.m_consumeItems
                            .Select((ci, i) => new BaseAITrait.ConsumeItem { itemDrop = ci, fedDurationFactor = 1f })
                            .ToArray();
                    }
                }

                if (monsterAI != null)
                {
                    Plugin.LogDebug($"{model}.{nameof(data_BaseAI_name)}: Setting MonsterAI values");

                    if (data_BaseAI.ConsumeRange != null) monsterAI.m_consumeRange = (float)data_BaseAI.ConsumeRange;
                    if (data_BaseAI.ConsumeSearchRange != null) monsterAI.m_consumeSearchRange = (float)data_BaseAI.ConsumeSearchRange;
                    if (data_BaseAI.ConsumeSearchInterval != null) monsterAI.m_consumeSearchInterval = (float)data_BaseAI.ConsumeSearchInterval;

                    if (consumeItems != null)
                    {
                        monsterAI.m_consumeItems = new List<ItemDrop>();
                        foreach (var ci in consumeItems)
                        {
                            monsterAI.m_consumeItems.Add(ci.itemDrop);
                        }
                    }
                }
                else
                {
                    // we need to make pseudo MonsterAI
                    if (animalAI != null)
                    {
                        Plugin.LogDebug($"{model}.{nameof(data_BaseAI_name)}: Setting custom AnimalAI values");

                        var exAnimalAI = AnimalAITrait.GetOrAddComponent(creature);

                        if (data_BaseAI.ConsumeRange != null) exAnimalAI.m_consumeRange = (float)data_BaseAI.ConsumeRange;
                        if (data_BaseAI.ConsumeSearchRange != null) exAnimalAI.m_consumeSearchRange = (float)data_BaseAI.ConsumeSearchRange;
                        if (data_BaseAI.ConsumeSearchInterval != null) exAnimalAI.m_consumeSearchInterval = (float)data_BaseAI.ConsumeSearchInterval;

                        if (consumeItems != null)
                        {
                            exAnimalAI.m_consumeItems = new List<ItemDrop>();
                            foreach (var ci in consumeItems)
                            {
                                exAnimalAI.m_consumeItems.Add(ci.itemDrop);
                            }
                        }
                    }
                }


                if (data_BaseAI.TamedIdleNearSpawn.HasValue) baseAITrait.m_tamedIdleNearSpawn = data_BaseAI.TamedIdleNearSpawn.Value;

                if (data_BaseAI.ConsumeAnimation != null)
                {
                    var customAnimation = data_BaseAI.ConsumeAnimation;
                    if (AnimationUtils.AnimationExists(creature, customAnimation, out AnimationClip animClip))
                    {
                        var runner = OTABPrefabRegistry.Instance.GetOrAddComponent<AnimationClipOverlay>(creatureName, creature);
                        runner.m_animClipName = customAnimation;
                    }
                    else
                    {
                        Plugin.LogWarning($"{model}.{nameof(MonsterAI)}.{nameof(data_BaseAI.ConsumeAnimation)}: Animation '{customAnimation}' not found on prefab '{creatureName}'. Custom consume animation ignored.");
                    }
                }

                if (data_BaseAI.ConsumeRandomItem == true)
                {
                    baseAITrait.m_consumeRandomItem = true;
                }

            }

            // ---------------------------
            // Character
            // ---------------------------

            if (data.Character != null)
            {
                var character = creature.GetComponent<Character>();
                var characterTrait = CharacterTrait.GetOrAddComponent(creature);
                Plugin.LogDebug($"{model}.{nameof(data.Character)}: Setting Character values");

                if (data.Character.Group != null)
                {
                    character.m_group = data.Character.Group;
                }

                if (data.Character.GroupWhenTamed != null)
                {
                    characterTrait.m_changeGroupWhenTamed = true;
                    characterTrait.m_changeGroupWhenTamedTo = data.Character.GroupWhenTamed;
                }

                if (data.Character.FactionWhenTamed.HasValue)
                {
                    characterTrait.m_changeFactionWhenTamed = true;
                    characterTrait.m_changeFactionWhenTamedTo = data.Character.FactionWhenTamed.Value;
                }

                if (data.Character.TameSpawnedOnDeath.HasValue)
                {
                    characterTrait.m_tameSpawnedOnDeath = data.Character.TameSpawnedOnDeath.Value;
                }

                characterTrait.m_tamedVersusPlayer = data.Character.TamedVersusPlayer;
                characterTrait.m_tamedVersusGroup = data.Character.TamedVersusGroup;
                characterTrait.m_tamedVersusFaction = data.Character.TamedVersusFaction;
                characterTrait.m_tamedVersusTamed = data.Character.TamedVersusTamed;
                characterTrait.m_tamedVersusWild = data.Character.TamedVersusWild;

            }

            // ---------------------------
            // Tameable
            // ---------------------------

            if (data.Tameable != null)
            {
                var tameable = OTABPrefabRegistry.Instance.GetOrAddComponent<Tameable>(creatureName, creature);
                var tameableTrait = TameableTrait.GetOrAddComponent(creature);
                var pet = OTABPrefabRegistry.Instance.GetOrAddComponent<Pet>(creatureName, creature); // also neccessary

                Plugin.LogDebug($"{model}.{nameof(data.Tameable)}: Setting Tameable values");

                /*
                if (data.Tameable.TamingBoostEnabled.HasValue)
                {
                    if (data.Tameable.TamingBoostEnabled.Value == false)
                    {
                        tameable.m_tamingSpeedMultiplierRange = 0;
                        tameable.m_tamingBoostMultiplier = 1;
                    }
                }
                // todo: add global config for this
                */

                if (data.Tameable.Commandable.HasValue)
                {
                    tameable.m_commandable = data.Tameable.Commandable.Value;
                }

                if (data.Tameable.FeedingDisabled == true)
                {
                    tameableTrait.m_feedingDisabled = true;
                }
                else
                {
                    if (data.Tameable.FedDuration.HasValue)
                    {
                        tameable.m_fedDuration = data.Tameable.FedDuration.Value;
                    }
                    else
                    {
                        tameable.m_fedDuration = 600; // we are using 600 as default, not 60
                    }
                }

                if (data.Tameable.TamingDisabled == true)
                {
                    tameableTrait.m_tamingDisabled = true;
                }
                else
                {
                    if (data.Tameable.TamingTime.HasValue)
                    {
                        tameable.m_tamingTime = data.Tameable.TamingTime.Value;
                    }
                    else
                    {
                        tameable.m_tamingTime = 1800f; // 1800 = default
                    }

                    if (!string.IsNullOrEmpty(data.Tameable.RequireGlobalKey))
                    {
                        tameableTrait.m_requireGlobalKey = EnvironmentUtils.ParseGlobalKey(data.Tameable.RequireGlobalKey);
                    }

                }

                Plugin.LogDebug($"{model}.{nameof(data.Tameable)}: Setting effects");
                if (tameable.m_sootheEffect?.m_effectPrefabs == null || tameable.m_sootheEffect.m_effectPrefabs.Length == 0)
                {
                    tameable.m_sootheEffect = new EffectList
                    {
                        m_effectPrefabs = Utilities.EffectUtils.CreateEffectList(new string[] {
                            "vfx_creature_soothed",
                        })
                    };
                }
                if (tameable.m_tamedEffect?.m_effectPrefabs == null || tameable.m_tamedEffect.m_effectPrefabs.Length == 0)
                {
                    tameable.m_tamedEffect = new EffectList
                    {
                        m_effectPrefabs = Utilities.EffectUtils.CreateEffectList(new string[] {
                            "fx_creature_tamed",
                        })
                    };
                }

                if (tameable.m_petEffect?.m_effectPrefabs == null || tameable.m_petEffect.m_effectPrefabs.Length == 0)
                {
                    var scaled = creature.GetComponent<ScaledCreature>();
                    var collider = creature.GetComponent<CapsuleCollider>();
                    var scale = scaled ? scaled.m_scale : 1f;
                    var effectY = (collider.center.y + collider.height * 0.5f * scale) * 0.5f;

                    var vfx = EffectUtils.GetVisualOnlyEffect("fx_boar_pet", $"{creatureName}_pet_vfx");

                    var particleSystem = vfx.GetComponentInChildren<ParticleSystem>(true);
                    if (particleSystem)
                    {
                        var position = particleSystem.transform.localPosition;
                        position.y = effectY;
                        particleSystem.transform.localPosition = position;

                        var heightFactorToBoar = collider.height / 1.4f;
                        var localScale = (heightFactorToBoar - 1) * 0.25f + 1f;

                        particleSystem.transform.localScale *= localScale;

                        var shape = particleSystem.shape;
                        shape.radius *= heightFactorToBoar / localScale;
                    }

                    tameable.m_petEffect = new EffectList
                    {
                        m_effectPrefabs = EffectUtils.CreateEffectList(new GameObject[]
                        {
                            vfx,
                            idleSoundPrefab,
                        })
                    };
                }
                
                if (data.Tameable.PetAnswerText != null)
                {
                    if (data.Tameable.PetAnswerText.Length == 0)
                    {
                        tameable.m_tameTextGetter = new Tameable.TextGetter(() => " ");
                    }
                    else
                    {
                        tameable.m_tameText = data.Tameable.PetAnswerText;
                    }
                }

                if (data.Tameable.PetCommandText != null)
                {
                    tameableTrait.m_petCommand = data.Tameable.PetCommandText;
                }

            }

            // ---------------------------
            // Growup
            // ---------------------------

            if (data.Growup != null)
            {
                var creatureGrowup = OTABPrefabRegistry.Instance.GetOrAddComponent<Growup>(creatureName, creature);
                Plugin.LogDebug($"{model}.{nameof(data.Growup)}: Setting Growup values");

                if (data.Growup.GrowTime.HasValue) creatureGrowup.m_growTime = data.Growup.GrowTime.Value;
                if (data.Growup.InheritTame.HasValue) creatureGrowup.m_inheritTame = data.Growup.InheritTame.Value;

                creatureGrowup.m_grownPrefab = null;
                
                creatureGrowup.m_altGrownPrefabs = new List<Growup.GrownEntry>();
                foreach (var grownData in data.Growup.Grown)
                {
                    creatureGrowup.m_altGrownPrefabs.Add(new Growup.GrownEntry
                    {
                        m_prefab = OTABPrefabRegistry.Instance.GetRegisteredPrefab(grownData.Prefab),
                        m_weight = grownData.Weight,
                    });
                }

                var growupTrait = GrowupTrait.GetOrAddComponent(creature);
                if (data.Growup.RequireFeeding.HasValue) growupTrait.m_requireFeeding = data.Growup.RequireFeeding.Value;
                if (!string.IsNullOrEmpty(data.Growup.RequireGlobalKey)) growupTrait.m_requireGlobalKey = data.Growup.RequireGlobalKey;

            }

            // ---------------------------
            // Procreation
            // ---------------------------

            if (data.Procreation != null)
            {
                if (data.Procreation != null)
                {
                    var procreation = OTABPrefabRegistry.Instance.GetOrAddComponent<Procreation>(creatureName, creature);
                    var procreationTrait = ProcreationTrait.GetOrAddComponent(creature);
                    Plugin.LogDebug($"{model}.{nameof(data.Procreation)}: Setting Procreation values");

                    procreationTrait.m_procreateWhileSwimming = data.Procreation.ProcreateWhileSwimming; // todo: rename yaml option to: "DisableProcreationWhileSwimming"

                    if (data.Procreation.Partner != null)
                    {
                        var partnerList = data.Procreation.Partner.Select((partnerName) => new ProcreationTrait.ProcreationPartner(
                            prefab: partnerName
                        )).ToArray();
                        procreationTrait.m_partnerListStoreIndex = ProcreationTrait.s_partnerListStore.Add(partnerList);
                    }

                    if (data.Procreation.Offspring != null)
                    {
                        var offspringList = data.Procreation.Offspring.Select((o) => new ProcreationTrait.ProcreationOffspring(
                            prefab: o.Prefab,
                            weight: o.Weight,
                            needPartner: o.NeedPartner,
                            needPartnerPrefab: o.NeedPartnerPrefab,
                            inheritTame: o.InheritTame
                        )).ToArray();
                        procreationTrait.m_offspringListStoreIndex = ProcreationTrait.s_offspringListStore.Add(offspringList);
                    }

                    if (data.Procreation.MaxCreaturesCountPrefabs != null)
                    {
                        // prefabs should have been already validated if they exist
                        GameObject[] prefabs = data.Procreation.MaxCreaturesCountPrefabs.Select(ZNetScene.instance.GetPrefab).ToArray();
                        procreationTrait.m_maxCreaturesPrefabsStoreIndex = ProcreationTrait.s_maxCreaturesPrefabsStore.Add(prefabs);
                    }

                    if (data.Procreation.UpdateInterval.HasValue) procreation.m_updateInterval = data.Procreation.UpdateInterval.Value;
                    if (data.Procreation.TotalCheckRange.HasValue) procreation.m_totalCheckRange = data.Procreation.TotalCheckRange.Value;
                    if (data.Procreation.PartnerCheckRange.HasValue) procreation.m_partnerCheckRange = data.Procreation.PartnerCheckRange.Value;
                    if (data.Procreation.RequiredLovePoints.HasValue) procreation.m_requiredLovePoints = data.Procreation.RequiredLovePoints.Value;
                    if (data.Procreation.PregnancyChance.HasValue) procreation.m_pregnancyChance = data.Procreation.PregnancyChance.Value;
                    if (data.Procreation.PregnancyDuration.HasValue) procreation.m_pregnancyDuration = data.Procreation.PregnancyDuration.Value;
                    if (data.Procreation.SpawnOffset.HasValue) procreation.m_spawnOffset = data.Procreation.SpawnOffset.Value;
                    if (data.Procreation.SpawnOffsetMax.HasValue) procreation.m_spawnOffsetMax = data.Procreation.SpawnOffsetMax.Value;
                    if (data.Procreation.SpawnRandomDirection.HasValue) procreation.m_spawnRandomDirection = data.Procreation.SpawnRandomDirection.Value;
                    if (data.Procreation.MaxCreatures.HasValue) procreation.m_maxCreatures = data.Procreation.MaxCreatures.Value;

                    Plugin.LogDebug($"{model}.{nameof(data.Procreation)}: Setting effects");

                    if (procreation.m_loveEffects?.m_effectPrefabs == null || procreation.m_loveEffects.m_effectPrefabs.Length == 0)
                    {
                        procreation.m_loveEffects = new EffectList
                        {
                            m_effectPrefabs = Utilities.EffectUtils.CreateEffectList(new string[] {
                                "vfx_boar_love",
                                idleSoundPrefab?.name,
                            })
                        };
                    }

                    if (procreation.m_birthEffects?.m_effectPrefabs == null || procreation.m_birthEffects.m_effectPrefabs.Length == 0)
                    {
                        procreation.m_birthEffects = new EffectList
                        {
                            m_effectPrefabs = Utilities.EffectUtils.CreateEffectList(new string[] {
                            "vfx_boar_birth",
                            idleSoundPrefab?.name,
                        })
                        };
                    }

                    // will be handled via ProcreationTrait
                    procreation.m_offspring = null;
                    procreation.m_seperatePartner = null;

                }
            }

            return valid;
        }

        //------------------------------------------------

        public override bool FinalizeProcess()
        {
            return true;
        }

        //------------------------------------------------

        public override void RestorePrefab(string creatureName)
        {
            OTABPrefabRegistry.Instance.RestorePrefab(creatureName, (current, backup) => {

                PrefabUtils.RestoreComponent<AnimalAI>(current, backup);
                PrefabUtils.RestoreComponent<MonsterAI>(current, backup);
                PrefabUtils.RestoreComponent<Character>(current, backup);
                PrefabUtils.RestoreComponent<CharacterDrop>(current, backup);
                PrefabUtils.RestoreComponent<Growup>(current, backup);
                PrefabUtils.RestoreComponent<Pet>(current, backup);
                PrefabUtils.RestoreComponent<Procreation>(current, backup);
                PrefabUtils.RestoreComponent<Ragdoll>(current, backup);
                PrefabUtils.RestoreComponent<Tameable>(current, backup);

                current.transform.localScale = backup.transform.localScale;

                VfxUtils.RestoreVfx(current, backup);
                PrefabUtils.RestoreFields<MonsterAI>(current, backup);
                PrefabUtils.RestoreFields<AnimalAI>(current, backup);
                PrefabUtils.RestoreFields<Tameable>(current, backup);
                PrefabUtils.RestoreFields<Growup>(current, backup);
                PrefabUtils.RestoreFields<Procreation>(current, backup);

                var currentLodGroup = current.transform.Find("Visual")?.GetComponent<LODGroup>();
                var backupLodGroup = backup.transform.Find("Visual")?.GetComponent<LODGroup>();
                if (currentLodGroup && backupLodGroup)
                {
                    currentLodGroup.size = backupLodGroup.size; // property
                }

                var currentFootStep = current.GetComponent<FootStep>();
                var backupFootStep = backup.GetComponent<FootStep>();
                if (currentFootStep && backupFootStep)
                {
                    currentFootStep.m_effects = backupFootStep.m_effects;
                    currentFootStep.enabled = backupFootStep.enabled; // property
                }

                var currentCharacterDrop = current.GetComponent<CharacterDrop>();
                var backupCharacterDrop = backup.GetComponent<CharacterDrop>();
                if (currentCharacterDrop && backupCharacterDrop)
                {
                    currentCharacterDrop.m_drops = backupCharacterDrop.m_drops;
                }

                var currentCharacter = current.GetComponent<Character>();
                var backupCharacter = backup.GetComponent<Character>();
                if (currentCharacter && backupCharacter)
                {
                    currentCharacter.m_deathEffects = backupCharacter.m_deathEffects;
                }

                var currentLevelFx = current.GetComponentInChildren<LevelEffects>(true);
                var backupLevelFx = backup.GetComponentInChildren<LevelEffects>(true);
                if (currentLevelFx && backupLevelFx)
                {
                    currentLevelFx.enabled = backupLevelFx.enabled; // property
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

                /*
                var currentCollider = current.GetComponent<CapsuleCollider>();
                var backupCollider = backup.GetComponent<CapsuleCollider>();
                if (currentCollider && backupCollider)
                {
                    currentCollider.height = backupCollider.height;
                    currentCollider.center = backupCollider.center;
                    currentCollider.radius = backupCollider.radius;
                }
                */

                var currentGrowup = current.GetComponent<Growup>();
                var backupGrowup = backup.GetComponent<Growup>();
                if (currentGrowup && backupGrowup)
                {
                    currentGrowup.m_grownPrefab = backupGrowup.m_grownPrefab;
                    currentGrowup.m_altGrownPrefabs = backupGrowup.m_altGrownPrefabs;
                }

                var currentTameable = current.GetComponent<Tameable>();
                var backupTameable = backup.GetComponent<Tameable>();
                if (currentTameable && backupTameable)
                {
                    currentTameable.m_tamedEffect = backupTameable.m_tamedEffect;
                    currentTameable.m_sootheEffect = backupTameable.m_sootheEffect;
                    currentTameable.m_petEffect = backupTameable.m_petEffect;
                    currentTameable.m_tameTextGetter = backupTameable.m_tameTextGetter;
                }

                var currentProcreation = current.GetComponent<Procreation>();
                var backupProcreation = backup.GetComponent<Procreation>();
                if (currentProcreation && backupProcreation)
                {
                    currentProcreation.m_birthEffects = backupProcreation.m_birthEffects;
                    currentProcreation.m_loveEffects = backupProcreation.m_loveEffects;
                    currentProcreation.m_offspring = backupProcreation.m_offspring;
                    currentProcreation.m_seperatePartner = backupProcreation.m_seperatePartner;
                }
            });
        }
        
        //------------------------------------------------

        public override void CleanupProcess()
        {
            BaseAITrait.s_consumeItemsStore.Clear();
            ProcreationTrait.s_partnerListStore.Clear();
            ProcreationTrait.s_offspringListStore.Clear();
            ProcreationTrait.s_maxCreaturesPrefabsStore.Clear();
        }

    }

}




