using OfTamingAndBreeding.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using OfTamingAndBreeding.Data.Models;
using OfTamingAndBreeding.Data.Models.SubData;
using OfTamingAndBreeding.Components;

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

        public override void Prepare(Base.PrefabRegistry reg)
        {

        }

        //------------------------------------------------
        // VALIDATE DATA
        //------------------------------------------------

        public override bool ValidateData(Base.PrefabRegistry reg, string creatureName, CreatureData data)
        {
            var model = $"{nameof(CreatureData)}.{creatureName}";
            var error = false;

            switch (data.Components.Character)
            {
                case ComponentBehavior.Remove:
                    Plugin.LogWarning($"{model}.{nameof(data.Components)}.{nameof(data.Components.Character)}({nameof(ComponentBehavior.Remove)}): Component cannot be removed");
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
                        error = true;
                    }
                    break;
                case ComponentBehavior.Inherit:
                    if (data.MonsterAI != null)
                    {
                        Plugin.LogWarning($"{model}.{nameof(data.Components)}.{nameof(data.Components.MonsterAI)}({nameof(ComponentBehavior.Inherit)}): Component data will be ignored");
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
                        error = true;
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
                    Plugin.LogWarning($"{model}.{nameof(data.Tameable)}.{nameof(data.Tameable.StarvingGraceFactor)}: Negative values not allowed - Using null");
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
                            error = true;
                        }
                    }
                }

                if (data.Procreation.PartnerRecheckSeconds != null)
                {
                    if (data.Procreation.Partner == null)
                    {
                        Plugin.LogWarning($"{model}.{nameof(data.Procreation)}.{nameof(data.Procreation.PartnerRecheckSeconds)}: Field set to null (Partner list is null or empty)");
                        data.Procreation.PartnerRecheckSeconds = null;
                    }
                    else if (data.Procreation.Partner.Length == 1)
                    {
                        Plugin.LogWarning($"{model}.{nameof(data.Procreation)}.{nameof(data.Procreation.PartnerRecheckSeconds)}: Field set to null (Partner list only contains one prefab)");
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
                            Plugin.LogWarning($"{model}.{nameof(data.Procreation)}.{nameof(data.Procreation.Offspring)}.{i}.{nameof(offspringData.LevelUpChance)}: Field needs 'MaxLevel' to be set.");
                            offspringData.LevelUpChance = null;
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

        public override bool ReservePrefab(Base.PrefabRegistry ctx, string creatureName, CreatureData data)
        {
            var model = $"{nameof(CreatureData)}.{creatureName}";

            var creature = ctx.GetReservedPrefab(creatureName);
            if (creature == null)
            {
                creature = ctx.GetOriginalPrefab(creatureName);
                if (creature == null)
                {
                    Plugin.LogError($"{model}: Prefab not found");
                    return false;
                }
                else
                {
                    ctx.MakeOriginalBackup(creature);
                }

                ctx.ReservePrefab(creatureName, creature);
            }
            
            return true;
        }

        //------------------------------------------------
        // VALIDATE PREFAB
        //------------------------------------------------

        public override bool ValidatePrefab(Base.PrefabRegistry reg, string creatureName, CreatureData data)
        {
            var model = $"{nameof(CreatureData)}.{creatureName}";
            var error = false;

            var creature = reg.GetReservedPrefab(creatureName);
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
                    Plugin.LogWarning($"{model}.{nameof(data.Procreation)}: Component requires {nameof(data.Tameable)}");
                    //error = true;
                }

            }

            if (data.MonsterAI != null && data.Components.MonsterAI == ComponentBehavior.Patch)
            {
                if (data.MonsterAI.ConsumeItems != null)
                {
                    foreach (var (foodData, i) in data.MonsterAI.ConsumeItems.Select((value, i) => (value, i)))
                    {
                        var foodItem = reg.GetCustomPrefab(foodData.Prefab) ?? reg.GetOriginalPrefab(foodData.Prefab);
                        if (foodItem == null)
                        {
                            Plugin.LogError($"{model}.{nameof(data.MonsterAI)}.{nameof(data.MonsterAI.ConsumeItems)}.{i}.{nameof(foodData.Prefab)}: '{foodData.Prefab}' not found");
                            error = true;
                        }
                        else
                        {
                            var foodItemDrop = foodItem.GetComponent<ItemDrop>();
                            if (foodItemDrop == null)
                            {
                                Plugin.LogError($"{model}.{nameof(data.MonsterAI)}.{nameof(data.MonsterAI.ConsumeItems)}.{i}.{nameof(foodData.Prefab)}: '{foodData.Prefab}' has no ItemDrop component");
                                error = true;
                            }
                        }
                    }
                }
            }

            if (data.Procreation != null && data.Components.Procreation == ComponentBehavior.Patch)
            {

                if (data.Procreation.Partner != null)
                {
                    foreach (var (partnerData, i) in data.Procreation.Partner.Select((value, i) => (value, i)))
                    {
                        if (!reg.PrefabExists(partnerData.Prefab))
                        {
                            Plugin.LogError($"{model}.{nameof(data.Procreation)}.{nameof(data.Procreation.Partner)}.{i}.{nameof(partnerData.Prefab)}: '{partnerData.Prefab}' not found");
                            error = true;
                        }
                    }
                }

                foreach (var (offspringData, i) in data.Procreation.Offspring.Select((value, i) => (value, i)))
                {

                    if (!reg.PrefabExists(offspringData.Prefab))
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
                        if (!reg.PrefabExists(offspringData.NeedPartnerPrefab))
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
                        if (!reg.PrefabExists(prefabName))
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

        public override void RegisterPrefab(Base.PrefabRegistry reg, string creatureName, CreatureData data)
        {
            var model = $"{nameof(CreatureData)}.{creatureName}";
            var creature = reg.GetReservedPrefab(creatureName);
            var creatureComponent = reg.GetOrAddComponent<OTAB_Creature>(creatureName, creature);

            var idleSoundPrefab = Utils.PrefabUtils.FindEffectPrefab<BaseAI>(creatureName, "m_idleSound", 0);

            if (data.Components.Character == ComponentBehavior.Patch)
            {
                var character = creature.GetComponent<Character>();
                if (data.Character != null)
                {
                    Plugin.LogDebug($"{model}.{nameof(data.Character)}: Setting Character values");

                    if (data.Character.Group != null)
                    {
                        character.m_group = data.Character.Group;
                    }

                    if (data.Character.GroupWhenTamed != null)
                    {
                        creatureComponent.m_changeGroupWhenTamed = true;
                        creatureComponent.m_changeGroupWhenTamedTo = data.Character.GroupWhenTamed;
                    }

                    if (data.Character.FactionWhenTamed.HasValue)
                    {
                        creatureComponent.m_changeFactionWhenTamed = true;
                        creatureComponent.m_changeFactionWhenTamedTo = data.Character.FactionWhenTamed.Value;
                    }

                    creatureComponent.m_stickToFaction = data.Character.TamedStickToFaction;
                    creatureComponent.m_tamedCanAttackTamed = data.Character.TamedCanAttackTamed;
                    creatureComponent.m_tamedCanBeAttackedByTamed = data.Character.TamedCanBeAttackedByTamed;
                    creatureComponent.m_tamedCanAttackPlayer = data.Character.TamedCanAttackPlayer;

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

                    StaticContext.CreatureDataContext.ConsumeItem[] consumeItems = null;
                    if (data.MonsterAI.ConsumeItems != null)
                    {
                        consumeItems = data.MonsterAI.ConsumeItems
                            .OrderByDescending(i => i.FedDurationFactor)
                            .Select((ci) =>
                            {
                                var foodItem = reg.GetCustomPrefab(ci.Prefab) ?? reg.GetOriginalPrefab(ci.Prefab);
                                return new StaticContext.CreatureDataContext.ConsumeItem
                                {
                                    itemDrop = foodItem.GetComponent<ItemDrop>(),
                                    fedDurationFactor = ci.FedDurationFactor,
                                };
                            }).ToArray();
                        creatureComponent.SetCustomConsumeItems(consumeItems);
                    }

                    if (monsterAI != null)
                    {

                        Plugin.LogDebug($"{model}.{nameof(data.MonsterAI)}: Setting MonsterAI values");

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
                            Plugin.LogDebug($"{model}.{nameof(data.MonsterAI)}: Setting custom AnimalAI values");

                            //var prefabAnimalAIData = animalAI.CreateExtraData();
                            var customAnimalAI = reg.GetOrAddComponent<OTAB_CustomAnimalAI>(creatureName, creature);

                            if (data.MonsterAI.ConsumeRange != null) customAnimalAI.m_consumeRange = (float)data.MonsterAI.ConsumeRange;
                            if (data.MonsterAI.ConsumeSearchRange != null) customAnimalAI.m_consumeSearchRange = (float)data.MonsterAI.ConsumeSearchRange;
                            if (data.MonsterAI.ConsumeSearchInterval != null) customAnimalAI.m_consumeSearchInterval = (float)data.MonsterAI.ConsumeSearchInterval;
                            
                            if (consumeItems != null)
                            {
                                customAnimalAI.m_consumeItems = new List<ItemDrop>();
                                foreach (var ci in consumeItems)
                                {
                                    customAnimalAI.m_consumeItems.Add(ci.itemDrop);
                                }
                            }
                        }
                    }

                    if (data.MonsterAI.TamedStayNearSpawn != null && (bool)data.MonsterAI.TamedStayNearSpawn == true)
                    {
                        creatureComponent.m_tamedStayNearSpawn = true;
                    }

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
                                var runner = reg.GetOrAddComponent<OTAB_ConsumeClipOverlay>(creatureName, creature);
                                runner.m_animClipName = customAnimation;
                            }
                            else
                            {
                                Plugin.LogWarning(
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
                    var tameable = reg.GetOrAddComponent<Tameable>(creatureName, creature);
                    var pet = reg.GetOrAddComponent<Pet>(creatureName, creature); // aso neccessary
                    Plugin.LogDebug($"{model}.{nameof(data.Tameable)}: Setting Tameable values");

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
                            creatureComponent.m_tamingDisabled = true;
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
                            creatureComponent.m_fedTimerDisabled = true;
                        }
                        tameable.m_fedDuration = fedDuration >= 0 ? fedDuration : 0; // better clamp. dunno if other mods can handle negative values
                    }
                    else
                    {
                        tameable.m_fedDuration = 600; // we are using 600 as default, not 60
                    }

                    if (data.Tameable.StarvingGraceFactor.HasValue)
                    {
                        creatureComponent.m_starvingGraceFactor = data.Tameable.StarvingGraceFactor.Value;
                    }









                    Plugin.LogDebug($"{model}.{nameof(data.Tameable)}: Setting effects");
                    if (tameable.m_sootheEffect.m_effectPrefabs.Length == 0)
                    {
                        tameable.m_sootheEffect = new EffectList
                        {
                            m_effectPrefabs = Utils.PrefabUtils.CreateEffectList(new string[] {
                                "vfx_creature_soothed",
                            })
                        };
                    }
                    if (tameable.m_tamedEffect.m_effectPrefabs.Length == 0)
                    {
                        tameable.m_tamedEffect = new EffectList
                        {
                            m_effectPrefabs = Utils.PrefabUtils.CreateEffectList(new string[] {
                                "fx_creature_tamed",
                            })
                        };
                    }
                    if (tameable.m_petEffect.m_effectPrefabs.Length == 0)
                    {
                        tameable.m_petEffect = new EffectList
                        {
                            m_effectPrefabs = Utils.PrefabUtils.CreateEffectList(new UnityEngine.GameObject[] {
                                PrefabUtils.GetVisualOnlyEffect("fx_boar_pet", "otab_vfx_pet"),
                                idleSoundPrefab,
                            })
                        };
                    }
                }
            }
            else if (data.Components.Tameable == ComponentBehavior.Remove)
            {
                Plugin.LogDebug($"{model}.{nameof(Tameable)}: Removing Tameable component (if exist)");
                reg.DestroyComponentIfExists<Tameable>(creatureName, creature);
            }

            if (data.Components.Procreation == ComponentBehavior.Patch)
            {
                if (data.Procreation != null)
                {
                    var procreation = reg.GetOrAddComponent<Procreation>(creatureName, creature);
                    Plugin.LogDebug($"{model}.{nameof(data.Procreation)}: Setting Procreation values");

                    creatureComponent.m_procreateWhileSwimming = data.Procreation.ProcreateWhileSwimming; // todo: rename yaml option to: "DisableProcreationWhileSwimming"
                    creatureComponent.m_maxSiblingsPerPregnancy = data.Procreation.MaxSiblingsPerPregnancy;
                    creatureComponent.m_extraSiblingChance = data.Procreation.ExtraSiblingChance;

                    if (data.Procreation.Partner != null)
                    {
                        creatureComponent.SetPartnerList(data.Procreation.Partner);
                    }

                    if (data.Procreation.Offspring != null)
                    {
                        creatureComponent.SetOffspringList(data.Procreation.Offspring);
                    }

                    if (data.Procreation.MaxCreaturesCountPrefabs != null)
                    {
                        creatureComponent.SetMaxCreaturesPrefabs(data.Procreation.MaxCreaturesCountPrefabs);
                    }




                    











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
                        creatureComponent.m_partnerRecheckTicks = TimeSpan.FromSeconds(seconds).Ticks;
                    }

                    Plugin.LogDebug($"{model}.{nameof(data.Procreation)}: Setting effects");

                    if (procreation.m_loveEffects.m_effectPrefabs.Length == 0)
                    {
                        procreation.m_loveEffects = new EffectList
                        {
                            m_effectPrefabs = Utils.PrefabUtils.CreateEffectList(new string[] {
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
                            m_effectPrefabs = Utils.PrefabUtils.CreateEffectList(new string[] {
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
                Plugin.LogDebug($"{model}.{nameof(Procreation)}: Removing Procreation component (if exist)");
                reg.DestroyComponentIfExists<Procreation>(creatureName, creature);
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

        public override void RestorePrefab(Base.PrefabRegistry reg, string creatureName)
        {
            reg.Restore(creatureName, RestorePrefabFromBackup);
        }

        private void RestorePrefabFromBackup(UnityEngine.GameObject backup, UnityEngine.GameObject current)
        {
            PrefabUtils.RestoreComponent<Character>(backup, current);
            PrefabUtils.RestoreComponent<MonsterAI>(backup, current);
            PrefabUtils.RestoreComponent<AnimalAI>(backup, current);
            PrefabUtils.RestoreComponent<Tameable>(backup, current);
            PrefabUtils.RestoreComponent<Pet>(backup, current);
            PrefabUtils.RestoreComponent<Procreation>(backup, current);
        }

        //------------------------------------------------
        // CLEANUP
        //------------------------------------------------

        public override void Cleanup(Base.PrefabRegistry reg)
        {
        }

    }

}

