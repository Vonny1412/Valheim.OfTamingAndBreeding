using Jotunn.Managers;
using OfTamingAndBreeding.Components;
using OfTamingAndBreeding.Components.Extensions;
using OfTamingAndBreeding.Components.Traits;
using OfTamingAndBreeding.Data.Models;
using OfTamingAndBreeding.Data.Models.SubData;
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
            ComponentBehavior component_BaseAI = ComponentBehavior.Inherit;
            string data_BaseAI_name = "";

            if (hasMonsterAI)
            {
                if (data.Components.AnimalAI.HasValue == true)
                {
                    Plugin.LogError($"{model}.{nameof(data.Components)}.{nameof(data.Components.AnimalAI)}: Invalid field because MonsterAI is present");
                    valid = false;
                }
                if (data.Components.MonsterAI.HasValue == false)
                {
                    Plugin.LogError($"{model}.{nameof(data.Components)}.{nameof(data.Components.MonsterAI)}: Missing field");
                    valid = false;
                }
                else
                {
                    data_BaseAI = data.MonsterAI;
                    component_BaseAI = data.Components.MonsterAI.Value;
                    data_BaseAI_name = nameof(data.MonsterAI);
                }
            }
            else if (hasAnimalAI)
            {
                if (data.Components.MonsterAI.HasValue == true)
                {
                    Plugin.LogError($"{model}.{nameof(data.Components)}.{nameof(data.Components.MonsterAI)}: Invalid field because AnimalAI is present");
                    valid = false;
                }
                if (data.Components.AnimalAI.HasValue == false)
                {
                    Plugin.LogError($"{model}.{nameof(data.Components)}.{nameof(data.Components.AnimalAI)}: Missing field");
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

            if (valid == false)
            {
                return false;
            }

            switch (component_BaseAI)
            {
                case ComponentBehavior.Remove:
                    Plugin.LogError($"{model}.{nameof(data.Components)}.{data_BaseAI_name}({nameof(ComponentBehavior.Remove)}): Component cannot be removed");
                    valid = false;
                    break;
                case ComponentBehavior.Patch:
                    if (data_BaseAI == null)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Components)}.{data_BaseAI_name}({nameof(ComponentBehavior.Patch)}): Missing component data");
                        valid = false;
                    }
                    break;
                case ComponentBehavior.Inherit:
                    if (data_BaseAI != null)
                    {
                        Plugin.LogWarning($"{model}.{nameof(data.Components)}.{data_BaseAI_name}({nameof(ComponentBehavior.Inherit)}): Component data will be ignored");
                    }
                    break;
            }

            // ---------------------------
            // Character
            // ---------------------------

            switch (data.Components.Character)
            {
                case ComponentBehavior.Remove:
                    Plugin.LogError($"{model}.{nameof(data.Components)}.{nameof(data.Components.Character)}({nameof(ComponentBehavior.Remove)}): Component cannot be removed");
                    valid = false;
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

            if (data.Character != null)
            {
                // nothing to validate here
            }

            // ---------------------------
            // Tameable
            // ---------------------------

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

                if (data.Growup.RequireFeeding != null && !hasTameable && data.Components.Tameable != ComponentBehavior.Patch)
                {
                    Plugin.LogError($"{model}.{nameof(data.Growup)}.{nameof(data.Growup.RequireFeeding)}: Field requires Tameable component");
                    valid = false;
                }

            }

            // ---------------------------
            // Procreation
            // ---------------------------

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

            if (data.Procreation != null)
            {
                bool wantProcreationActive = data.Components.Procreation == ComponentBehavior.Patch || (hasProcreation && data.Components.Procreation != ComponentBehavior.Remove);
                bool wantTameableActive = data.Components.Tameable != ComponentBehavior.Remove && (hasTameable || data.Components.Tameable == ComponentBehavior.Patch);
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
            ComponentBehavior component_BaseAI = ComponentBehavior.Inherit;
            string data_BaseAI_name = "";

            if (monsterAI)
            {
                data_BaseAI = data.MonsterAI;
                component_BaseAI = data.Components.MonsterAI.Value;
                data_BaseAI_name = nameof(data.MonsterAI);
            }
            else if (animalAI)
            {
                data_BaseAI = data.AnimalAI;
                component_BaseAI = data.Components.AnimalAI.Value;
                data_BaseAI_name = nameof(data.AnimalAI);
            }
            else
            {
                Plugin.LogError($"{model}: Prefab has neither MonsterAI nor AnimalAI");
                return false;
            }

            if (component_BaseAI == ComponentBehavior.Patch)
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

            // ---------------------------
            // Character
            // ---------------------------

            if (data.Components.Character == ComponentBehavior.Patch)
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
            else if (data.Components.Character == ComponentBehavior.Remove)
            {
                // ignore, cannot be removed
            }

            // ---------------------------
            // Tameable
            // ---------------------------

            if (data.Components.Tameable == ComponentBehavior.Patch)
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
            else if (data.Components.Tameable == ComponentBehavior.Remove)
            {
                Plugin.LogDebug($"{model}.{nameof(Tameable)}: Removing Tameable component (if exist)");
                OTABPrefabRegistry.Instance.DestroyComponentIfExists<Tameable>(creatureName, creature);
            }

            // ---------------------------
            // Growup
            // ---------------------------

            if (data.Components.Growup == ComponentBehavior.Patch)
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
            else if (data.Components.Growup == ComponentBehavior.Remove)
            {
                Plugin.LogDebug($"{model}.{nameof(Growup)}: Removing Growup component (if exist)");
                OTABPrefabRegistry.Instance.DestroyComponentIfExists<Growup>(creatureName, creature);
            }

            // ---------------------------
            // Procreation
            // ---------------------------

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
                            levelUpChance: o.LevelUpChance ?? 0,
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
            else if (data.Components.Procreation == ComponentBehavior.Remove)
            {
                Plugin.LogDebug($"{model}.{nameof(Procreation)}: Removing Procreation component (if exist)");
                OTABPrefabRegistry.Instance.DestroyComponentIfExists<Procreation>(creatureName, creature);
            }

            return valid;
        }



























        private void PrepareClone(string creatureName, CreatureFile data, UnityEngine.GameObject creature)
        {
            var model = $"{nameof(CreatureFile)}.{creatureName}";

            var creatureCharacter = creature.GetComponent<Character>();
            var creatureBaseAI = creature.GetComponent<BaseAI>();
            var creatureMonsterAI = creature.GetComponent<MonsterAI>();
            var creatureAnimalAI = creature.GetComponent<AnimalAI>();


            if (data.Clone.RemoveMonsterAI == true && creatureMonsterAI)
            {
                // BaseAI fields
                var baseAISnapshot = new Common.FieldsSnapshot<BaseAI>(creatureMonsterAI);

                // MonsterAI fields
                var m_avoidLand = creatureMonsterAI.m_avoidLand;
                var m_fleeInLava = creatureMonsterAI.m_fleeInLava;

                OTABPrefabRegistry.Instance.DestroyComponentIfExists<MonsterAI>(creatureName, creature);
                creatureAnimalAI = OTABPrefabRegistry.Instance.GetOrAddComponent<AnimalAI>(creatureName, creature);
                creatureMonsterAI = null;

                // BaseAI fields
                baseAISnapshot.ApplyTo(creatureAnimalAI);

                // MonsterAI fields
                var animalAITrait = AnimalAITrait.GetOrAddComponent(creature);
                animalAITrait.m_avoidLand = m_avoidLand;
                animalAITrait.m_fleeInLava = m_fleeInLava;

            }

            if (creature.TryGetComponent<CharacterDrop>(out var charDrop))
            {
                var drops = new List<CharacterDrop.Drop>();
                foreach (var source in charDrop.m_drops)
                {
                    var isSpecial =
                        creatureCharacter.m_boss ||
                        source.m_onePerPlayer ||
                        source.m_prefab.name.StartsWith("trophy", StringComparison.OrdinalIgnoreCase);
                    if (isSpecial)
                    {
                        continue;
                    }
                    drops.Add(new CharacterDrop.Drop
                    {
                        m_prefab = source.m_prefab,
                        m_amountMin = 0,
                        m_amountMax = source.m_amountMax > 1 ? (int)(source.m_amountMax / 2f + 0.5f) : source.m_amountMax,
                        m_chance = source.m_chance / 2f,
                        m_onePerPlayer = source.m_onePerPlayer,
                        m_levelMultiplier = false,
                        m_dontScale = true,
                    });
                }

                charDrop.m_drops = drops;
            }

            // display higher level creatures always as level 1 creature
            var levelFx = creature.GetComponentInChildren<LevelEffects>(true);
            if (levelFx != null)
            {
                levelFx.enabled = false;
            }

            var footStep = creature.GetComponent<FootStep>();
            if (footStep)
            {
                //footStep.enabled = false; this would result in NRE
                footStep.m_effects = footStep.m_effects.Select(effect => new FootStep.StepEffect
                {
                    m_name = effect.m_name,
                    m_motionType = effect.m_motionType,
                    m_material = effect.m_material,
                    m_effectPrefabs = Array.Empty<GameObject>(),
                }).ToList();
            }

            if (data.Clone.RemoveEffects != null)
            {
                VfxUtils.DisableVfx(creature, data.Clone.RemoveEffects);
            }

            Plugin.LogDebug($"{model}.{nameof(data.Clone)}: Setting Character values");
            if (data.Clone.Name != null) creatureCharacter.m_name = data.Clone.Name;


            var comp1 = creature.GetComponent<MovementDamage>();
            if (comp1)
            {
                // disable faders walk damage
                comp1.enabled = false;
                if (comp1.m_runDamageObject)
                {
                    comp1.m_runDamageObject.SetActive(false);
                }
            }

            if (data.Clone.Scale.HasValue)
            {
                var setScale = data.Clone.Scale.Value;

                Plugin.LogDebug($"{model}.{nameof(data.Clone)}: Setting custom scaling to {setScale}");

                creature.transform.localScale *= setScale;

                creatureCharacter.m_speed *= setScale;

                creatureCharacter.m_crouchSpeed *= setScale;
                creatureCharacter.m_walkSpeed *= setScale;
                creatureCharacter.m_runSpeed *= setScale;
                creatureCharacter.m_swimSpeed *= setScale;
                creatureCharacter.m_flySlowSpeed *= setScale;
                creatureCharacter.m_flyFastSpeed *= setScale;
                creatureBaseAI.m_randomMoveRange *= setScale;

                Plugin.LogDebug($"{model}.{nameof(data.Clone)}: Setting vfx scaling");
                VfxUtils.ScaleVfx(creature, setScale); // scale model particles

                Plugin.LogDebug($"{model}.{nameof(data.Clone)}: Setting death effects scaling");
                creatureCharacter.m_deathEffects = EffectUtils.CloneEffectList(creatureCharacter.m_deathEffects);
                foreach (var eff in creatureCharacter.m_deathEffects.m_effectPrefabs)
                {
                    var originalEffect = eff.m_prefab;

                    var clonedEffectName = $"{creatureName}_{originalEffect.name}";
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

                            var clonedRemoveEffectName = $"{creatureName}_{originalEffect.name}_{originalRemoveEffect.name}";
                            var clonedRemoveEffect = PrefabManager.Instance.GetPrefab(clonedRemoveEffectName);
                            if (clonedRemoveEffect == null)
                            {
                                clonedRemoveEffect = PrefabManager.Instance.CreateClonedPrefab(clonedRemoveEffectName, originalRemoveEffect.name);
                            }
                            else
                            {
                                VfxUtils.RestoreVfx(clonedRemoveEffect, originalRemoveEffect);
                            }

                            VfxUtils.ScaleVfx(clonedRemoveEffect, setScale);
                            eff2.m_prefab = clonedRemoveEffect;
                        }
                    }
                }

                var scaledCreature = OTABPrefabRegistry.Instance.GetOrAddComponent<ScaledCreature>(creatureName, creature);
                scaledCreature.m_scale = setScale;
                scaledCreature.m_animationScale = 1 / setScale;

                // do not hide the creature if we just go some steps away
                var lodGroup = creatureCharacter.transform.Find("Visual")?.GetComponent<LODGroup>();
                if (lodGroup)
                {
                    // todo: is this beeing restored correctly?
                    lodGroup.size /= setScale;
                }

                // stats

                var setHealthScale = setScale;
                var setAttackScale = setScale;
                creatureCharacter.m_health *= setHealthScale;
                scaledCreature.m_attackScale = setAttackScale;









            }


            // additional stuff

            creatureCharacter.m_boss = false;
            creatureCharacter.m_bossEvent = "";
            creatureCharacter.m_defeatSetGlobalKey = "";
            creatureCharacter.m_killedForAchievements = Utils.AchievementInclusion.Excluded;
            creatureBaseAI.m_spawnMessage = "";
            creatureBaseAI.m_deathMessage = "";

            if (creatureMonsterAI)
            {
                creatureMonsterAI.m_enableHuntPlayer = false;
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




