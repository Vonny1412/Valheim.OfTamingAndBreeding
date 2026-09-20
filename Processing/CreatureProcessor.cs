using OfTamingAndBreeding.Components;
using OfTamingAndBreeding.Components.Traits;
using OfTamingAndBreeding.Data.Models;
using OfTamingAndBreeding.Data.Models.SubData;
using OfTamingAndBreeding.Processing.Core;
using OfTamingAndBreeding.Registry;
using OfTamingAndBreeding.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

namespace OfTamingAndBreeding.Processing
{
    internal class CreatureProcessor : DataProcessor<CreatureFile>
    {
        public override string DirectoryName => CreatureFile.DirectoryName;

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

        public override bool ValidateData(string creatureName, CreatureFile data)
        {
            var model = $"{nameof(CreatureFile)}.{creatureName}";
            var valid = true;

            switch (data.Components.Character)
            {
                case ComponentBehavior.Remove:
                    Plugin.LogWarning($"{model}.{nameof(data.Components)}.{nameof(data.Components.Character)}({nameof(ComponentBehavior.Remove)}): Component cannot be removed");
                    break;
                case ComponentBehavior.Patch:
                    if (data.Character == null)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Components)}.{nameof(data.Components.Character)}({nameof(ComponentBehavior.Patch)}): Missing component data");
                        valid = false;
                    }
                    break;
                case ComponentBehavior.Inherit:
                    if (data.Character != null)
                    {
                        Plugin.LogWarning($"{model}.{nameof(data.Components)}.{nameof(data.Components.Character)}({nameof(ComponentBehavior.Inherit)}): Component data will be ignored");
                    }
                    break;
            }

            switch (data.Components.MonsterAI)
            {
                case ComponentBehavior.Remove:
                    Plugin.LogWarning($"{model}.{nameof(data.Components)}.{nameof(data.Components.MonsterAI)}({nameof(ComponentBehavior.Remove)}): Component cannot be removed");
                    break;
                case ComponentBehavior.Patch:
                    if (data.MonsterAI == null)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Components)}.{nameof(data.Components.MonsterAI)}({nameof(ComponentBehavior.Patch)}): Missing component data");
                        valid = false;
                    }
                    break;
                case ComponentBehavior.Inherit:
                    if (data.MonsterAI != null)
                    {
                        Plugin.LogWarning($"{model}.{nameof(data.Components)}.{nameof(data.Components.MonsterAI)}({nameof(ComponentBehavior.Inherit)}): Component data will be ignored");
                    }
                    break;
            }

            switch (data.Components.AnimalAI)
            {
                case ComponentBehavior.Remove:
                    Plugin.LogWarning($"{model}.{nameof(data.Components)}.{nameof(data.Components.AnimalAI)}({nameof(ComponentBehavior.Remove)}): Component cannot be removed");
                    break;
                case ComponentBehavior.Patch:
                    if (data.AnimalAI == null)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Components)}.{nameof(data.Components.AnimalAI)}({nameof(ComponentBehavior.Patch)}): Missing component data");
                        valid = false;
                    }
                    break;
                case ComponentBehavior.Inherit:
                    if (data.AnimalAI != null)
                    {
                        Plugin.LogWarning($"{model}.{nameof(data.Components)}.{nameof(data.Components.AnimalAI)}({nameof(ComponentBehavior.Inherit)}): Component data will be ignored");
                    }
                    break;
            }

            switch (data.Components.Tameable)
            {
                case ComponentBehavior.Patch:
                    if (data.Tameable == null)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Components)}.{nameof(data.Components.Tameable)}({nameof(ComponentBehavior.Patch)}): Missing component data");
                        valid = false;   
                    }
                    break;
                case ComponentBehavior.Inherit:
                    if (data.Tameable != null)
                    {
                        Plugin.LogWarning($"{model}.{nameof(data.Components)}.{nameof(data.Components.Tameable)}({nameof(ComponentBehavior.Inherit)}): Component data will be ignored");
                    }
                    break;
            }

            switch (data.Components.Procreation)
            {
                case ComponentBehavior.Patch:
                    if (data.Procreation == null)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Components)}.{nameof(data.Components.Procreation)}({nameof(ComponentBehavior.Patch)}): Missing component data");
                        valid = false;
                    }
                    break;
                case ComponentBehavior.Inherit:
                    if (data.Procreation != null)
                    {
                        Plugin.LogWarning($"{model}.{nameof(data.Components)}.{nameof(data.Components.Procreation)}({nameof(ComponentBehavior.Inherit)}): Component data will be ignored");
                    }
                    break;
            }

            if (data.Character != null && data.Components.Character == ComponentBehavior.Patch)
            {
                // nothing to validate here
            }

            if (data.Procreation != null && data.Components.Procreation == ComponentBehavior.Patch)
            {

                if (data.Tameable == null)
                {
                    // we gonna check this in prefab validation
                }

                if (data.Procreation.MaxCreaturesCountPrefabs == null)
                {
                    // if == null then this feature is just disabled
                }

                if (data.Procreation.Partner == null)
                {
                    // nothing todo
                }
                else if (data.Procreation.Partner.Length == 0)
                {
                    Plugin.LogWarning($"{model}.{nameof(data.Procreation)}.{nameof(data.Procreation.Partner)}: Field set to null (list was empty)");
                    data.Procreation.Partner = null; // just clean it up
                }
                else
                {
                    foreach (var (partnerData, i) in data.Procreation.Partner.Select((value, i) => (value, i)))
                    {
                        partnerData.Weight = Math.Max(0f, partnerData.Weight);
                        if (partnerData.Prefab == null)
                        {
                            Plugin.LogError($"{model}.{nameof(data.Procreation)}.{nameof(data.Procreation.Partner)}.{i}.{nameof(partnerData.Prefab)}: Field is empty");
                            valid = false;
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

            }

            return valid;
        }

        //------------------------------------------------
        // RESERVE PREFAB
        //------------------------------------------------

        public override bool ReservePrefab(string creatureName, CreatureFile data)
        {
            var model = $"{nameof(CreatureFile)}.{creatureName}";

            var creature = OTABPrefabRegistry.Instance.GetReservedPrefab(creatureName);
            if (creature == null)
            {
                creature = OTABPrefabRegistry.Instance.GetOriginalPrefab(creatureName);
                if (creature == null)
                {
                    Plugin.LogError($"{model}: Prefab not found");
                    return false;
                }
                else
                {
                    OTABPrefabRegistry.Instance.MakeOriginalBackup(creatureName);
                }

                OTABPrefabRegistry.Instance.ReservePrefab(creatureName, creature);
            }
            
            return true;
        }

        //------------------------------------------------
        // VALIDATE PREFAB
        //------------------------------------------------

        public override bool ValidatePrefab(string creatureName, CreatureFile data)
        {
            var model = $"{nameof(CreatureFile)}.{creatureName}";
            var valid = true;

            var creature = OTABPrefabRegistry.Instance.GetReservedPrefab(creatureName);
            if (!creature)
            {
                Plugin.LogError($"{model}: Prefab not found");
                valid = false;
            }
            else
            {

                var monsterAI = creature.GetComponent<MonsterAI>();
                var animalAI = creature.GetComponent<AnimalAI>();

                if (!monsterAI && !animalAI)
                {
                    Plugin.LogError($"{model}: Prefab has no supported AI");
                    valid = false;
                }

                if (!creature.GetComponent<Character>())
                {
                    Plugin.LogError($"{model}: Prefab has no Character");
                    valid = false;
                }

                var hasProcreation = (bool)creature.GetComponent<Procreation>();
                var hasTameable = (bool)creature.GetComponent<Tameable>();
                bool wantProcreationActive = data.Components.Procreation == ComponentBehavior.Patch || (hasProcreation && data.Components.Procreation != ComponentBehavior.Remove);
                bool wantTameableActive = data.Components.Tameable != ComponentBehavior.Remove && (hasTameable || data.Components.Tameable == ComponentBehavior.Patch);
                if (wantProcreationActive && !wantTameableActive)
                {
                    Plugin.LogError($"{model}.{nameof(data.Procreation)}: Component requires {nameof(data.Tameable)}");
                    valid = false;
                }

            }

            if (data.Procreation != null && data.Components.Procreation == ComponentBehavior.Patch)
            {

                if (data.Procreation.Partner != null)
                {
                    foreach (var (partnerData, i) in data.Procreation.Partner.Select((value, i) => (value, i)))
                    {
                        if (!OTABPrefabRegistry.Instance.PrefabExists(partnerData.Prefab))
                        {
                            Plugin.LogError($"{model}.{nameof(data.Procreation)}.{nameof(data.Procreation.Partner)}.{i}.{nameof(partnerData.Prefab)}: '{partnerData.Prefab}' not found");
                            valid = false;
                        }
                    }
                }

                foreach (var (offspringData, i) in data.Procreation.Offspring.Select((value, i) => (value, i)))
                {

                    if (!OTABPrefabRegistry.Instance.PrefabExists(offspringData.Prefab))
                    {
                        Plugin.LogError($"{model}.{nameof(data.Procreation)}.{nameof(data.Procreation.Offspring)}.{i}.{nameof(offspringData.Prefab)}: '{offspringData.Prefab}' not found");
                        valid = false;
                    }

                    if (offspringData.NeedPartner == false)
                    {
                        offspringData.NeedPartnerPrefab = null;
                    }

                    if (offspringData.NeedPartnerPrefab != null)
                    {
                        if (!OTABPrefabRegistry.Instance.PrefabExists(offspringData.NeedPartnerPrefab))
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
                        if (!OTABPrefabRegistry.Instance.PrefabExists(prefabName))
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
        // REGISTER PREFAB
        //------------------------------------------------

        public override void RegisterPrefab(string creatureName, CreatureFile data)
        {
            // no need to register any
        }

        //------------------------------------------------
        // EDIT PREFAB
        //------------------------------------------------

        public override bool ProcessPrefab(string creatureName, CreatureFile data)
        {
            var model = $"{nameof(CreatureFile)}.{creatureName}";
            var valid = true;

            var creature = OTABPrefabRegistry.Instance.GetReservedPrefab(creatureName);
            var idleSoundPrefab = Utilities.EffectUtils.FindEffectPrefab<BaseAI>(creatureName, "m_idleSound", 0);

            if (data.Components.Character == ComponentBehavior.Patch)
            {
                if (data.Character != null)
                {
                    var character = creature.GetComponent<Character>();
                    var characterTrait = CharacterTrait.GetOrAddComponent(creature);
                    Plugin.LogDebug($"{model}.{nameof(data.Character)}: Setting Character values");

                    if (data.Character.MaxLevel != null)
                    {
                        characterTrait.m_maxLevel = data.Character.MaxLevel.Value;
                        //todo: validate for positive values?
                    }

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

                    characterTrait.m_tamedVersusPlayer = data.Character.TamedVersusPlayer;
                    characterTrait.m_tamedVersusGroup = data.Character.TamedVersusGroup;
                    characterTrait.m_tamedVersusFaction = data.Character.TamedVersusFaction;
                    characterTrait.m_tamedVersusTamed = data.Character.TamedVersusTamed;
                    characterTrait.m_tamedVersusWild = data.Character.TamedVersusWild;


                }
            }
            else if (data.Components.Character == ComponentBehavior.Remove)
            {
                // ignore, cannot be removed
            }







            var monsterAI = creature.GetComponent<MonsterAI>();
            var animalAI = creature.GetComponent<AnimalAI>();

            CreatureFile.BaseAIData data_BaseAI = null;
            ComponentBehavior component_BaseAI = ComponentBehavior.Inherit;
            string data_BaseAI_name = "";


            // this is the correct place for following checks
            // because offspringprocessor is removing monsterai and replacing it with animalai
            if (monsterAI)
            {
                if (data.Components.AnimalAI.HasValue == true)
                {
                    Plugin.LogError($"{model}.{nameof(data.Components)}.{nameof(data.Components.AnimalAI)}: Invalid presence because MonsterAI is present");
                    valid = false;
                }
                if (data.Components.MonsterAI.HasValue == false)
                {
                    Plugin.LogError($"{model}.{nameof(data.Components)}.{nameof(data.Components.MonsterAI)}: Missing value");
                    valid = false;
                }
                else
                {
                    data_BaseAI = data.MonsterAI;
                    component_BaseAI = data.Components.MonsterAI.Value;
                    data_BaseAI_name = nameof(data.MonsterAI);
                }
            }
            else if (animalAI)
            {
                if (data.Components.MonsterAI.HasValue == true)
                {
                    Plugin.LogError($"{model}.{nameof(data.Components)}.{nameof(data.Components.MonsterAI)}: Invalid presence because AnimalAI is present");
                    valid = false;
                }
                if (data.Components.AnimalAI.HasValue == false)
                {
                    Plugin.LogError($"{model}.{nameof(data.Components)}.{nameof(data.Components.AnimalAI)}: Missing value");
                    valid = false;
                }
                else
                {
                    data_BaseAI = data.AnimalAI;
                    component_BaseAI = data.Components.AnimalAI.Value;
                    data_BaseAI_name = nameof(data.AnimalAI);
                }
            }
            else
            {
                Plugin.LogError($"{model}: Prefab has neither MonsterAI nor AnimalAI");
                valid = false;
            }









            if (component_BaseAI == ComponentBehavior.Patch)
            {
                //var baseAI = creature.GetComponent<BaseAI>();
                var baseAITrait = BaseAITrait.GetOrAddComponent(creature);

                BaseAITrait.ConsumeItem[] consumeItems = null;

                if (data_BaseAI.ConsumeItems != null)
                {
                    consumeItems = data_BaseAI.ConsumeItems
                        .OrderByDescending(i => i.FedDurationFactor)
                        .Select((ci, i) =>
                        {
                            var foodItem = OTABPrefabRegistry.Instance.GetCustomPrefab(ci.Prefab)
                                        ?? OTABPrefabRegistry.Instance.GetOriginalPrefab(ci.Prefab);

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

                baseAITrait.m_tamedStayNearSpawn = data_BaseAI.TamedStayNearSpawn;


                if (data_BaseAI.ConsumeAnimation != null)
                {
                    var customAnimation = data_BaseAI.ConsumeAnimation;
                    if (customAnimation.ToLower() == "debug")
                    {
                        var zanim = creature.GetComponent<ZSyncAnimation>();
                        AnimationUtils.DumpZSyncAnim(zanim, $"{model}:");
                    }
                    else
                    {
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
                }

                if (data_BaseAI.IdleSoundChanceWhenTamed.HasValue)
                {
                    // todo: validate and clamp 0-1
                    baseAITrait.m_idleSoundChanceWhenTamed = data_BaseAI.IdleSoundChanceWhenTamed.Value;
                }
                
            }
            else if (component_BaseAI == ComponentBehavior.Remove)
            {
                // ignore, cannot be removed
            }

            if (data.Components.Tameable == ComponentBehavior.Patch)
            {
                if (data.Tameable != null)
                {
                    var tameable = OTABPrefabRegistry.Instance.GetOrAddComponent<Tameable>(creatureName, creature);
                    var tameableTrait = TameableTrait.GetOrAddComponent(creature);
                    var pet = OTABPrefabRegistry.Instance.GetOrAddComponent<Pet>(creatureName, creature); // also neccessary

                    Plugin.LogDebug($"{model}.{nameof(data.Tameable)}: Setting Tameable values");

                    if (data.Tameable.TamingBoostEnabled.HasValue)
                    {
                        if (data.Tameable.TamingBoostEnabled.Value == false)
                        {
                            tameable.m_tamingSpeedMultiplierRange = 0;
                            tameable.m_tamingBoostMultiplier = 1;
                        }
                    }

                    if (data.Tameable.Commandable.HasValue)
                    {
                        tameable.m_commandable = data.Tameable.Commandable.Value;
                    }

                    if (data.Tameable.TamingTime.HasValue)
                    {
                        var tamingTime = data.Tameable.TamingTime.Value;
                        if (tamingTime >= 0)
                        {
                            // tameable (even if its 0, maybe any other addon uses instant taming?)
                        }
                        else
                        {
                            // not tameable
                            tameableTrait.m_tamingDisabled = true;
                        }
                        tameable.m_tamingTime = tamingTime >= 0 ? tamingTime : 0; // better clamp. dunno if other mods can handle negative values
                    }

                    if (data.Tameable.FedDuration.HasValue)
                    {
                        var fedDuration = data.Tameable.FedDuration.Value;
                        if (fedDuration >= 0)
                        {
                            // can eat
                        }
                        else
                        {
                            // cannot eat
                            tameableTrait.m_fedTimerDisabled = true;
                        }
                        tameable.m_fedDuration = fedDuration >= 0 ? fedDuration : 0; // better clamp. dunno if other mods can handle negative values
                    }
                    else
                    {
                        tameable.m_fedDuration = 600; // we are using 600 as default, not 60
                    }

                    if (data.Tameable.RequireGlobalKeys != null)
                    {
                        var keysList = ParseGlobalKeys(data.Tameable.RequireGlobalKeys);
                        tameableTrait.SetRequiredGlobalKeys(keysList);
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

                    if (data.Tameable.ShowPetEffect == false)
                    {
                        tameable.m_petEffect = new EffectList
                        {
                            m_effectPrefabs = Array.Empty<EffectList.EffectData>()
                        };
                    }
                    else
                    {
                        if (tameable.m_petEffect?.m_effectPrefabs == null || tameable.m_petEffect.m_effectPrefabs.Length == 0)
                        {
                            tameable.m_petEffect = new EffectList
                            {
                                m_effectPrefabs = EffectUtils.CreateEffectList(new GameObject[]
                                {
                                    EffectUtils.GetVisualOnlyEffect("fx_boar_pet", "otab_vfx_pet"),
                                    idleSoundPrefab,
                                })
                            };
                        }
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
            }
            else if (data.Components.Tameable == ComponentBehavior.Remove)
            {
                Plugin.LogDebug($"{model}.{nameof(Tameable)}: Removing Tameable component (if exist)");
                OTABPrefabRegistry.Instance.DestroyComponentIfExists<Tameable>(creatureName, creature);
            }

            if (data.Components.Procreation == ComponentBehavior.Patch)
            {
                if (data.Procreation != null)
                {
                    var procreation = OTABPrefabRegistry.Instance.GetOrAddComponent<Procreation>(creatureName, creature);
                    var procreationTrait = ProcreationTrait.GetOrAddComponent(creature);
                    Plugin.LogDebug($"{model}.{nameof(data.Procreation)}: Setting Procreation values");

                    procreationTrait.m_procreateWhileSwimming = data.Procreation.ProcreateWhileSwimming; // todo: rename yaml option to: "DisableProcreationWhileSwimming"

                    if (data.Procreation.Partner != null)
                    {
                        var partnerList = data.Procreation.Partner.Select((p) => new ProcreationTrait.ProcreationPartner(
                            prefab: p.Prefab,
                            weight: p.Weight
                        )).ToArray();
                        procreationTrait.SetPartnerList(partnerList);
                    }

                    if (data.Procreation.Offspring != null)
                    {
                        var offspringList = data.Procreation.Offspring.Select((o) => new ProcreationTrait.ProcreationOffspring(
                            prefab: o.Prefab,
                            weight: o.Weight,
                            needPartner: o.NeedPartner,
                            needPartnerPrefab: o.NeedPartnerPrefab,
                            levelUpChance: o.LevelUpChance ?? 0,
                            spawnTamed: o.SpawnTamed
                        )).ToArray();
                        procreationTrait.SetOffspringList(offspringList);
                    }

                    if (data.Procreation.MaxCreaturesCountPrefabs != null)
                    {
                        procreationTrait.SetMaxCreaturesPrefabs(data.Procreation.MaxCreaturesCountPrefabs);
                    }




                    









                    // todo: cleanup, use HasValue/Value

                    if (data.Procreation.UpdateInterval != null) procreation.m_updateInterval = (float)data.Procreation.UpdateInterval;
                    if (data.Procreation.TotalCheckRange != null) procreation.m_totalCheckRange = (float)data.Procreation.TotalCheckRange;

                    if (data.Procreation.PartnerCheckRange != null) procreation.m_partnerCheckRange = (float)data.Procreation.PartnerCheckRange;
                    if (data.Procreation.RequiredLovePoints != null) procreation.m_requiredLovePoints = (int)data.Procreation.RequiredLovePoints;
                    else procreation.m_requiredLovePoints = 3;

                    if (data.Procreation.PregnancyChance != null) procreation.m_pregnancyChance = (float)data.Procreation.PregnancyChance;
                    else procreation.m_pregnancyChance = 0.33f; // because most vanilla creatures use 0.33 instead of default 0.5
                    if (data.Procreation.PregnancyDuration != null) procreation.m_pregnancyDuration = (float)data.Procreation.PregnancyDuration;
                    else procreation.m_pregnancyDuration = 60;

                    if (data.Procreation.SpawnOffset != null) procreation.m_spawnOffset = (float)data.Procreation.SpawnOffset;
                    if (data.Procreation.SpawnOffsetMax != null) procreation.m_spawnOffsetMax = (float)data.Procreation.SpawnOffsetMax;
                    if (data.Procreation.SpawnRandomDirection != null) procreation.m_spawnRandomDirection = (bool)data.Procreation.SpawnRandomDirection;

                    if (data.Procreation.MaxCreatures != null) procreation.m_maxCreatures = (int)data.Procreation.MaxCreatures;

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
            else if (data.Components.Procreation == ComponentBehavior.Remove)
            {
                Plugin.LogDebug($"{model}.{nameof(Procreation)}: Removing Procreation component (if exist)");
                OTABPrefabRegistry.Instance.DestroyComponentIfExists<Procreation>(creatureName, creature);
            }

            return valid;
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

        public override void RestorePrefab(string creatureName)
        {
            OTABPrefabRegistry.Instance.RestorePrefab(creatureName, (current, backup) =>
            {
                var currentMonsterAI = current.GetComponent<MonsterAI>();
                var backupMonsterAI = backup.GetComponent<MonsterAI>();
                var currentTameable = current.GetComponent<Tameable>();
                var backupTameable = backup.GetComponent<Tameable>();
                var currentProcreation = current.GetComponent<Procreation>();
                var backupProcreation = backup.GetComponent<Procreation>();

                if (currentMonsterAI && backupMonsterAI)
                {
                    currentMonsterAI.m_consumeItems = backupMonsterAI.m_consumeItems;
                }

                if (currentTameable && backupTameable)
                {
                    currentTameable.m_tamedEffect = backupTameable.m_tamedEffect;
                    currentTameable.m_sootheEffect = backupTameable.m_sootheEffect;
                    currentTameable.m_petEffect = backupTameable.m_petEffect;
                    currentTameable.m_tameTextGetter = backupTameable.m_tameTextGetter;
                }

                if (currentProcreation && backupProcreation)
                {
                    currentProcreation.m_birthEffects = backupProcreation.m_birthEffects;
                    currentProcreation.m_loveEffects = backupProcreation.m_loveEffects;
                    currentProcreation.m_offspring = backupProcreation.m_offspring;
                    currentProcreation.m_seperatePartner = backupProcreation.m_seperatePartner;
                }

                var comp1 = current.GetComponent(typeof(AnimationClipOverlay));
                if (comp1)
                {
                    UnityEngine.Object.DestroyImmediate(comp1);
                }
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

