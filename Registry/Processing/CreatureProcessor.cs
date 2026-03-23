using OfTamingAndBreeding.Components;
using OfTamingAndBreeding.Components.SpecialPrefabs;
using OfTamingAndBreeding.Components.Traits;
using OfTamingAndBreeding.Data.Models;
using OfTamingAndBreeding.Data.Models.SubData;
using OfTamingAndBreeding.OTABUtils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

namespace OfTamingAndBreeding.Registry.Processing
{
    internal class CreatureProcessor : Base.DataProcessor<CreatureData>
    {
        public override string DirectoryName => CreatureData.DirectoryName;

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

        public override bool ValidateData(string creatureName, CreatureData data)
        {
            var model = $"{nameof(CreatureData)}.{creatureName}";
            var error = false;

            switch (data.Components.Character)
            {
                case ComponentBehavior.Remove:
                    Plugin.LogServerWarning($"{model}.{nameof(data.Components)}.{nameof(data.Components.Character)}({nameof(ComponentBehavior.Remove)}): Component cannot be removed");
                    break;
                case ComponentBehavior.Patch:
                    if (data.Character == null)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Components)}.{nameof(data.Components.Character)}({nameof(ComponentBehavior.Patch)}): Missing component data");
                        error = true;
                    }
                    break;
                case ComponentBehavior.Inherit:
                    if (data.Character != null)
                    {
                        Plugin.LogServerWarning($"{model}.{nameof(data.Components)}.{nameof(data.Components.Character)}({nameof(ComponentBehavior.Inherit)}): Component data will be ignored");
                    }
                    break;
            }

            switch (data.Components.MonsterAI)
            {
                case ComponentBehavior.Remove:
                    Plugin.LogServerWarning($"{model}.{nameof(data.Components)}.{nameof(data.Components.MonsterAI)}({nameof(ComponentBehavior.Remove)}): Component cannot be removed");
                    break;
                case ComponentBehavior.Patch:
                    if (data.MonsterAI == null)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Components)}.{nameof(data.Components.MonsterAI)}({nameof(ComponentBehavior.Patch)}): Missing component data");
                        error = true;
                    }
                    break;
                case ComponentBehavior.Inherit:
                    if (data.MonsterAI != null)
                    {
                        Plugin.LogServerWarning($"{model}.{nameof(data.Components)}.{nameof(data.Components.MonsterAI)}({nameof(ComponentBehavior.Inherit)}): Component data will be ignored");
                    }
                    break;
            }

            switch (data.Components.Tameable)
            {
                case ComponentBehavior.Patch:
                    if (data.Tameable == null)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Components)}.{nameof(data.Components.Tameable)}({nameof(ComponentBehavior.Patch)}): Missing component data");
                        error = true;   
                    }
                    break;
                case ComponentBehavior.Inherit:
                    if (data.Tameable != null)
                    {
                        Plugin.LogServerWarning($"{model}.{nameof(data.Components)}.{nameof(data.Components.Tameable)}({nameof(ComponentBehavior.Inherit)}): Component data will be ignored");
                    }
                    break;
            }

            switch (data.Components.Procreation)
            {
                case ComponentBehavior.Patch:
                    if (data.Procreation == null)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Components)}.{nameof(data.Components.Procreation)}({nameof(ComponentBehavior.Patch)}): Missing component data");
                        error = true;
                    }
                    break;
                case ComponentBehavior.Inherit:
                    if (data.Procreation != null)
                    {
                        Plugin.LogServerWarning($"{model}.{nameof(data.Components)}.{nameof(data.Components.Procreation)}({nameof(ComponentBehavior.Inherit)}): Component data will be ignored");
                    }
                    break;
            }

            if (data.Character != null && data.Components.Character == ComponentBehavior.Patch)
            {
                // nothing to validate here
            }

            if (data.MonsterAI != null && data.Components.MonsterAI == ComponentBehavior.Patch)
            {
                if (data.MonsterAI.ConsumeItems == null)
                {
                    // null = keep original
                    // empty list = clear: wont eat anything
                }
            }

            if (data.Tameable != null && data.Components.Tameable == ComponentBehavior.Patch)
            {
                if (data.Tameable.StarvingGraceFactor.HasValue && data.Tameable.StarvingGraceFactor.Value < 0)
                {
                    Plugin.LogServerWarning($"{model}.{nameof(data.Tameable)}.{nameof(data.Tameable.StarvingGraceFactor)}: Negative values not allowed - Using null");
                    data.Tameable.StarvingGraceFactor = null;
                }
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
                    Plugin.LogServerWarning($"{model}.{nameof(data.Procreation)}.{nameof(data.Procreation.Partner)}: Field set to null (list was empty)");
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
                            error = true;
                        }
                    }
                }

                if (data.Procreation.PartnerRecheckSeconds != null)
                {
                    if (data.Procreation.Partner == null)
                    {
                        Plugin.LogServerWarning($"{model}.{nameof(data.Procreation)}.{nameof(data.Procreation.PartnerRecheckSeconds)}: Field set to null (Partner list is null or empty)");
                        data.Procreation.PartnerRecheckSeconds = null;
                    }
                    else if (data.Procreation.Partner.Length == 1)
                    {
                        Plugin.LogServerWarning($"{model}.{nameof(data.Procreation)}.{nameof(data.Procreation.PartnerRecheckSeconds)}: Field set to null (Partner list only contains one prefab)");
                        data.Procreation.PartnerRecheckSeconds = null;
                    }
                }

                if (data.Procreation.Offspring == null || data.Procreation.Offspring.Length == 0)
                {
                    Plugin.LogError($"{model}.{nameof(data.Procreation)}.{nameof(data.Procreation.Offspring)}: Field is required but null or empty");
                    error = true;
                }
                else
                {
                    foreach (var (offspringData, i) in data.Procreation.Offspring.Select((value, i) => (value, i)))
                    {
                        offspringData.Weight = Math.Max(0f, offspringData.Weight);
                        if (offspringData.Prefab == null)
                        {
                            Plugin.LogError($"{model}.{nameof(data.Procreation)}.{nameof(data.Procreation.Offspring)}.{i}.{nameof(offspringData.Prefab)}: Field is empty");
                            error = true;
                        }
                        if (offspringData.LevelUpChance != null && offspringData.MaxLevel == null)
                        {
                            Plugin.LogServerWarning($"{model}.{nameof(data.Procreation)}.{nameof(data.Procreation.Offspring)}.{i}.{nameof(offspringData.LevelUpChance)}: Field needs 'MaxLevel' to be set.");
                            offspringData.LevelUpChance = null;
                            // no error
                        }
                        if (offspringData.MaxLevel != null && offspringData.MaxLevel <= 0)
                        {
                            Plugin.LogServerWarning($"{model}.{nameof(data.Procreation)}.{nameof(data.Procreation.Offspring)}.{i}.{nameof(offspringData.MaxLevel)}: Invalid value '{offspringData.MaxLevel}' - Setting to 0");
                            offspringData.MaxLevel = 1;
                            // no error
                        }
                    }
                }

            }

            return error == false;
        }

        //------------------------------------------------
        // RESERVE PREFAB
        //------------------------------------------------

        public override bool ReservePrefab(string creatureName, CreatureData data)
        {
            var model = $"{nameof(CreatureData)}.{creatureName}";

            var creature = PrefabRegistry.Instance.GetReservedPrefab(creatureName);
            if (creature == null)
            {
                creature = PrefabRegistry.Instance.GetOriginalPrefab(creatureName);
                if (creature == null)
                {
                    Plugin.LogError($"{model}: Prefab not found");
                    return false;
                }
                else
                {
                    PrefabRegistry.Instance.MakeOriginalBackup(creatureName);
                }

                PrefabRegistry.Instance.ReservePrefab(creatureName, creature);
            }
            
            return true;
        }

        //------------------------------------------------
        // VALIDATE PREFAB
        //------------------------------------------------

        public override bool ValidatePrefab(string creatureName, CreatureData data)
        {
            var model = $"{nameof(CreatureData)}.{creatureName}";
            var error = false;

            var creature = PrefabRegistry.Instance.GetReservedPrefab(creatureName);
            if (!creature)
            {
                Plugin.LogError($"{model}: Prefab not found");
                error = true;
            }
            else
            {

                if (!creature.GetComponent<MonsterAI>() && !creature.GetComponent<AnimalAI>())
                {
                    Plugin.LogError($"{model}: Prefab has no supported AI");
                    error = true;
                }

                if (!creature.GetComponent<Character>())
                {
                    Plugin.LogError($"{model}: Prefab has no Character");
                    error = true;
                }

                var hasProcreation = (bool)creature.GetComponent<Procreation>();
                var hasTameable = (bool)creature.GetComponent<Tameable>();
                bool wantProcreationActive = data.Components.Procreation == ComponentBehavior.Patch || (hasProcreation && data.Components.Procreation != ComponentBehavior.Remove);
                bool wantTameableActive = data.Components.Tameable != ComponentBehavior.Remove && (hasTameable || data.Components.Tameable == ComponentBehavior.Patch);
                if (wantProcreationActive && !wantTameableActive)
                {
                    Plugin.LogServerWarning($"{model}.{nameof(data.Procreation)}: Component requires {nameof(data.Tameable)}");
                    //error = true;
                }

            }

            if (data.MonsterAI != null && data.Components.MonsterAI == ComponentBehavior.Patch)
            {
                if (data.MonsterAI.ConsumeItems != null)
                {
                    data.MonsterAI.ConsumeItems = data.MonsterAI.ConsumeItems.Where((CreatureData.MonsterAIConsumItemData foodData, int i) => {

                        if (SpecialPrefabRegistry.IsSpecialPrefabCommand(foodData.Prefab))
                        {
                            return true;
                        }

                        var foodItem = PrefabRegistry.Instance.GetCustomPrefab(foodData.Prefab)
                                    ?? PrefabRegistry.Instance.GetOriginalPrefab(foodData.Prefab);
                        if (foodItem == null)
                        {
                            Plugin.LogServerWarning($"{model}.{nameof(data.MonsterAI)}.{nameof(data.MonsterAI.ConsumeItems)}.{i}.{nameof(foodData.Prefab)}: '{foodData.Prefab}' not found");
                            return false;
                        }
                        else
                        {
                            var foodItemDrop = foodItem.GetComponent<ItemDrop>();
                            if (foodItemDrop == null)
                            {
                                Plugin.LogError($"{model}.{nameof(data.MonsterAI)}.{nameof(data.MonsterAI.ConsumeItems)}.{i}.{nameof(foodData.Prefab)}: '{foodData.Prefab}' has no ItemDrop component");
                                error = true;
                                return false;
                            }
                        }
                        return true;

                    }).ToArray();
                }
            }

            if (data.Procreation != null && data.Components.Procreation == ComponentBehavior.Patch)
            {

                if (data.Procreation.Partner != null)
                {
                    foreach (var (partnerData, i) in data.Procreation.Partner.Select((value, i) => (value, i)))
                    {
                        if (!PrefabRegistry.Instance.PrefabExists(partnerData.Prefab))
                        {
                            Plugin.LogError($"{model}.{nameof(data.Procreation)}.{nameof(data.Procreation.Partner)}.{i}.{nameof(partnerData.Prefab)}: '{partnerData.Prefab}' not found");
                            error = true;
                        }
                    }
                }

                foreach (var (offspringData, i) in data.Procreation.Offspring.Select((value, i) => (value, i)))
                {

                    if (!PrefabRegistry.Instance.PrefabExists(offspringData.Prefab))
                    {
                        Plugin.LogError($"{model}.{nameof(data.Procreation)}.{nameof(data.Procreation.Offspring)}.{i}.{nameof(offspringData.Prefab)}: '{offspringData.Prefab}' not found");
                        error = true;
                    }

                    if (offspringData.NeedPartner == false)
                    {
                        offspringData.NeedPartnerPrefab = null;
                    }

                    if (offspringData.NeedPartnerPrefab != null)
                    {
                        if (!PrefabRegistry.Instance.PrefabExists(offspringData.NeedPartnerPrefab))
                        {
                            Plugin.LogError($"{model}.{nameof(data.Procreation)}.{nameof(data.Procreation.Offspring)}.{i}.{nameof(offspringData.NeedPartnerPrefab)}: '{offspringData.NeedPartnerPrefab}' not found");
                            error = true;
                        }
                    }
                }

                if (data.Procreation.MaxCreaturesCountPrefabs != null)
                {
                    foreach (var (prefabName, i) in data.Procreation.MaxCreaturesCountPrefabs.Select((value, i) => (value, i)))
                    {
                        if (!PrefabRegistry.Instance.PrefabExists(prefabName))
                        {
                            Plugin.LogError($"{model}.{nameof(data.Procreation)}.{nameof(data.Procreation.MaxCreaturesCountPrefabs)}.{i}: '{prefabName}' not found");
                            error = true;
                        }
                    }
                }
                

            }

            return error == false;
        }

        //------------------------------------------------
        // REGISTER PREFAB
        //------------------------------------------------

        public override void RegisterPrefab(string creatureName, CreatureData data)
        {
            // no need to register any
        }

        //------------------------------------------------
        // EDIT PREFAB
        //------------------------------------------------

        public override void EditPrefab(string creatureName, CreatureData data)
        {
            var model = $"{nameof(CreatureData)}.{creatureName}";
            var creature = PrefabRegistry.Instance.GetReservedPrefab(creatureName);

            var idleSoundPrefab = OTABUtils.PrefabUtils.FindEffectPrefab<BaseAI>(creatureName, "m_idleSound", 0);

            if (data.Components.Character == ComponentBehavior.Patch)
            {
                if (data.Character != null)
                {
                    var character = creature.GetComponent<Character>();
                    var characterTrait = CharacterTrait.GetOrAddComponent(creature);
                    Plugin.LogServerDebug($"{model}.{nameof(data.Character)}: Setting Character values");

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

                    characterTrait.m_tamedCanAttackPlayer = data.Character.TamedCanAttackPlayer;
                    characterTrait.m_tamedCanBeAttackedByPlayer = data.Character.TamedCanBeAttackedByPlayer;
                    characterTrait.m_tamedCanAttackTamed = data.Character.TamedCanAttackTamed;
                    characterTrait.m_tamedCanBeAttackedByTamed = data.Character.TamedCanBeAttackedByTamed;
                    characterTrait.m_tamedCanAttackWild = data.Character.TamedCanAttackWild;
                    characterTrait.m_tamedCanBeAttackedByWild = data.Character.TamedCanBeAttackedByWild;
                    characterTrait.m_tamedCanAttackGroup = data.Character.TamedCanAttackGroup;
                    characterTrait.m_tamedCanBeAttackedByGroup = data.Character.TamedCanBeAttackedByGroup;
                    characterTrait.m_tamedCanAttackFaction = data.Character.TamedCanAttackFaction;
                    characterTrait.m_tamedCanBeAttackedByFaction = data.Character.TamedCanBeAttackedByFaction;

                }
            }
            else if (data.Components.Character == ComponentBehavior.Remove)
            {
                // ignore, cannot be removed
            }

            if (data.Components.MonsterAI == ComponentBehavior.Patch)
            {
                if (data.MonsterAI != null)
                {
                    var monsterAI = creature.GetComponent<MonsterAI>();
                    var animalAI = creature.GetComponent<AnimalAI>();
                    var baseAITrait = BaseAITrait.GetOrAddComponent(creature);

                    BaseAITrait.ConsumeItem[] consumeItems = null;

                    if (data.MonsterAI.ConsumeItems != null)
                    {
                        consumeItems = data.MonsterAI.ConsumeItems
                            .OrderByDescending(i => i.FedDurationFactor)
                            .Select((ci) =>
                            {

                                if (SpecialPrefabRegistry.IsSpecialPrefabCommand(ci.Prefab))
                                {
                                    SpecialPrefabRegistry.CreateSpecialPrefabFromCommand(ci.Prefab, out var specialPrefab);
                                    return new BaseAITrait.ConsumeItem
                                    {
                                        itemDrop = specialPrefab.GetComponent<ItemDrop>(),
                                        fedDurationFactor = ci.FedDurationFactor,
                                    };
                                }

                                var foodItem = PrefabRegistry.Instance.GetCustomPrefab(ci.Prefab)
                                            ?? PrefabRegistry.Instance.GetOriginalPrefab(ci.Prefab);
                                return new BaseAITrait.ConsumeItem
                                {
                                    itemDrop = foodItem.GetComponent<ItemDrop>(),
                                    fedDurationFactor = ci.FedDurationFactor,
                                };
                            }).ToArray();
                        baseAITrait.SetCustomConsumeItems(consumeItems);
                    }

                    if (monsterAI != null)
                    {

                        Plugin.LogServerDebug($"{model}.{nameof(data.MonsterAI)}: Setting MonsterAI values");

                        if (data.MonsterAI.ConsumeRange != null) monsterAI.m_consumeRange = (float)data.MonsterAI.ConsumeRange;
                        if (data.MonsterAI.ConsumeSearchRange != null) monsterAI.m_consumeSearchRange = (float)data.MonsterAI.ConsumeSearchRange;
                        if (data.MonsterAI.ConsumeSearchInterval != null) monsterAI.m_consumeSearchInterval = (float)data.MonsterAI.ConsumeSearchInterval;

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
                            Plugin.LogServerDebug($"{model}.{nameof(data.MonsterAI)}: Setting custom AnimalAI values");

                            var exAnimalAI = AnimalAITrait.GetOrAddComponent(creature);

                            if (data.MonsterAI.ConsumeRange != null) exAnimalAI.m_consumeRange = (float)data.MonsterAI.ConsumeRange;
                            if (data.MonsterAI.ConsumeSearchRange != null) exAnimalAI.m_consumeSearchRange = (float)data.MonsterAI.ConsumeSearchRange;
                            if (data.MonsterAI.ConsumeSearchInterval != null) exAnimalAI.m_consumeSearchInterval = (float)data.MonsterAI.ConsumeSearchInterval;
                            
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

                    baseAITrait.m_tamedStayNearSpawn = data.MonsterAI.TamedStayNearSpawn;
                    

                    if (data.MonsterAI.ConsumeAnimation != null)
                    {
                        var customAnimation = data.MonsterAI.ConsumeAnimation;
                        if (customAnimation.ToLower() == "debug")
                        {
                            var zanim = creature.GetComponent<ZSyncAnimation>();
                            AnimationUtils.DumpZSyncAnim(zanim, $"{model}:");
                        }
                        else
                        {
                            if (AnimationUtils.AnimationExists(creature, customAnimation, out AnimationClip animClip))
                            {
                                var runner = PrefabRegistry.Instance.GetOrAddComponent<AnimationClipOverlay>(creatureName, creature);
                                runner.m_animClipName = customAnimation;
                            }
                            else
                            {
                                Plugin.LogServerWarning(
                                    $"{model}.{nameof(MonsterAI)}.{nameof(data.MonsterAI.ConsumeAnimation)}: Animation '{customAnimation}' not found on prefab '{creatureName}'. Custom consume animation ignored."
                                );
                            }
                        }
                    }

                }
            }
            else if (data.Components.MonsterAI == ComponentBehavior.Remove)
            {
                // ignore, cannot be removed
            }


            if (data.Components.Tameable == ComponentBehavior.Patch)
            {
                if (data.Tameable != null)
                {
                    var tameable = PrefabRegistry.Instance.GetOrAddComponent<Tameable>(creatureName, creature);
                    var tameableTrait = TameableTrait.GetOrAddComponent(creature);
                    var pet = PrefabRegistry.Instance.GetOrAddComponent<Pet>(creatureName, creature); // also neccessary

                    Plugin.LogServerDebug($"{model}.{nameof(data.Tameable)}: Setting Tameable values");

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

                    if (data.Tameable.StarvingGraceFactor.HasValue)
                    {
                        tameableTrait.m_starvingGraceFactor = data.Tameable.StarvingGraceFactor.Value;
                    }

                    if (data.Tameable.RequireGlobalKeys != null)
                    {
                        var keysList = ParseGlobalKeys(data.Tameable.RequireGlobalKeys);
                        tameableTrait.SetRequiredGlobalKeys(keysList);
                    }

                    Plugin.LogServerDebug($"{model}.{nameof(data.Tameable)}: Setting effects");
                    if (tameable.m_sootheEffect.m_effectPrefabs.Length == 0)
                    {
                        tameable.m_sootheEffect = new EffectList
                        {
                            m_effectPrefabs = OTABUtils.PrefabUtils.CreateEffectList(new string[] {
                                "vfx_creature_soothed",
                            })
                        };
                    }
                    if (tameable.m_tamedEffect.m_effectPrefabs.Length == 0)
                    {
                        tameable.m_tamedEffect = new EffectList
                        {
                            m_effectPrefabs = OTABUtils.PrefabUtils.CreateEffectList(new string[] {
                                "fx_creature_tamed",
                            })
                        };
                    }

                    tameable.m_petEffect ??= new EffectList(); // just to make sure
                    if (data.Tameable.ShowPetEffect == false)
                    {
                        tameable.m_petEffect.m_effectPrefabs = Array.Empty<EffectList.EffectData>();
                    }
                    else
                    {
                        if (tameable.m_petEffect.m_effectPrefabs == null || tameable.m_petEffect.m_effectPrefabs.Length == 0)
                        {
                            tameable.m_petEffect.m_effectPrefabs = OTABUtils.PrefabUtils.CreateEffectList(new UnityEngine.GameObject[]
                            {
                                PrefabUtils.GetVisualOnlyEffect("fx_boar_pet", "otab_vfx_pet"),
                                idleSoundPrefab,
                            });
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
                Plugin.LogServerDebug($"{model}.{nameof(Tameable)}: Removing Tameable component (if exist)");
                PrefabRegistry.Instance.DestroyComponentIfExists<Tameable>(creatureName, creature);
            }

            if (data.Components.Procreation == ComponentBehavior.Patch)
            {
                if (data.Procreation != null)
                {
                    var procreation = PrefabRegistry.Instance.GetOrAddComponent<Procreation>(creatureName, creature);
                    var procreationTrait = ProcreationTrait.GetOrAddComponent(creature);
                    Plugin.LogServerDebug($"{model}.{nameof(data.Procreation)}: Setting Procreation values");

                    procreationTrait.m_procreateWhileSwimming = data.Procreation.ProcreateWhileSwimming; // todo: rename yaml option to: "DisableProcreationWhileSwimming"
                    procreationTrait.m_maxSiblingsPerPregnancy = data.Procreation.MaxSiblingsPerPregnancy;
                    procreationTrait.m_extraSiblingChance = data.Procreation.ExtraSiblingChance;

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
                            maxLevel: o.MaxLevel ?? 1,
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

                    if (data.Procreation.PartnerRecheckSeconds.HasValue)
                    {
                        var seconds = data.Procreation.PartnerRecheckSeconds.Value;
                        procreationTrait.m_partnerRecheckTicks = TimeSpan.FromSeconds(seconds).Ticks;
                    }

                    Plugin.LogServerDebug($"{model}.{nameof(data.Procreation)}: Setting effects");

                    if (procreation.m_loveEffects.m_effectPrefabs.Length == 0)
                    {
                        procreation.m_loveEffects = new EffectList
                        {
                            m_effectPrefabs = OTABUtils.PrefabUtils.CreateEffectList(new string[] {
                            "vfx_boar_love",
                            idleSoundPrefab?.name,
                        })
                        };

                        procreation.m_loveEffects.m_effectPrefabs[0].m_scale = true;
                        procreation.m_loveEffects.m_effectPrefabs[0].m_prefab.transform.localScale = UnityEngine.Vector3.one * 0.5f;

                    }
                    if (procreation.m_birthEffects.m_effectPrefabs.Length == 0)
                    {
                        procreation.m_birthEffects = new EffectList
                        {
                            m_effectPrefabs = OTABUtils.PrefabUtils.CreateEffectList(new string[] {
                            "vfx_boar_birth",
                            idleSoundPrefab?.name,
                        })
                        };
                    }

                    // will be handled via ProcreationAPI
                    procreation.m_offspring = null;
                    procreation.m_seperatePartner = null;

                }
            }
            else if (data.Components.Procreation == ComponentBehavior.Remove)
            {
                Plugin.LogServerDebug($"{model}.{nameof(Procreation)}: Removing Procreation component (if exist)");
                PrefabRegistry.Instance.DestroyComponentIfExists<Procreation>(creatureName, creature);
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

        public override void RestorePrefab(string creatureName)
        {
            PrefabRegistry.Instance.RestorePrefab(creatureName, (current, backup) => {
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

