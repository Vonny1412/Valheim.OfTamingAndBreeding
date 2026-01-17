using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using OfTamingAndBreeding.Data.Models;

namespace OfTamingAndBreeding.Data.Handling
{
    internal class CreatureModelHandler : ModelHandler<Creature>
    {
        public override string DirectoryName => Creature.DirectoryName;

        //------------------------------------------------
        // VALIDATE DATA
        //------------------------------------------------

        public override bool ValidateData(ModelHandlerContext ctx, string creatureName, Creature data)
        {
            var model = $"{nameof(Creature)}.{creatureName}";
            var error = false;

            var characterData = data.Character;
            var monsterAIData = data.MonsterAI;
            var tameableData = data.Tameable;
            var procreationData = data.Procreation;

            if (monsterAIData != null)
            {
                if (monsterAIData.ConsumeItems == null || monsterAIData.ConsumeItems.Length == 0)
                {
                    monsterAIData.ConsumeItems = new Creature.MonsterAIConsumItemData[] { };
                }
            }

            if (procreationData != null)
            {

                if (tameableData == null)
                {
                    Plugin.LogError($"{model}.{nameof(data.Procreation)} requires {nameof(data.Tameable)}");
                    error = true;
                }

                if (procreationData.Partner == null || procreationData.Partner.Length == 0)
                {
                    procreationData.Partner = new Creature.ProcreationPartnerData[] {  };
                }

                foreach (var (partnerData, i) in procreationData.Partner.Select((value, i) => (value, i)))
                {
                    partnerData.Weight = Math.Max(0f, partnerData.Weight);
                    if (partnerData.Prefab == null)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Procreation)}.{nameof(procreationData.Partner)}.{i}.{nameof(partnerData.Prefab)} is empty");
                        error = true;
                    }
                }

                if (procreationData.Offspring == null || procreationData.Offspring.Length == 0)
                {
                    Plugin.LogError($"{model}.{nameof(data.Procreation)}.{nameof(procreationData.Offspring)} is null or empty");
                    error = true;
                }

                foreach (var (offspringData, i) in procreationData.Offspring.Select((value, i) => (value, i)))
                {
                    offspringData.Weight = Math.Max(0f, offspringData.Weight);
                    if (offspringData.Prefab == null)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Procreation)}.{nameof(procreationData.Offspring)}.{i}.{nameof(offspringData.Prefab)} is empty");
                        error = true;
                    }
                }
            }

            return error == false;
        }

        //------------------------------------------------
        // PREPARE PREFAB
        //------------------------------------------------

        public override bool PreparePrefab(ModelHandlerContext ctx, string creatureName, Creature data)
        {
            var model = $"{nameof(Creature)}.{creatureName}";

            //var creature = ctx.zns.GetPrefab(creatureName);
            var creature = ctx.GetPrefab(creatureName);
            if (!creature)
            {
                Plugin.LogError($"{model}: Prefab not found");
                return false;
            }

            ctx.CachePrefab(creatureName, creature);
            return true;
        }

        //------------------------------------------------
        // VALIDATE PREFAB
        //------------------------------------------------

        public override bool ValidatePrefab(ModelHandlerContext ctx, string creatureName, Creature data)
        {
            var model = $"{nameof(Creature)}.{creatureName}";
            var error = false;

            var creature = ctx.GetPrefab(creatureName);
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
            }

            if (data.MonsterAI != null)
            {
                foreach (var (foodData, i) in data.MonsterAI.ConsumeItems.Select((value, i) => (value, i)))
                {
                    if (!ctx.PrefabExists(foodData.Prefab))
                    {
                        Plugin.LogFatal($"{model}.{nameof(data.MonsterAI)}.{nameof(data.MonsterAI.ConsumeItems)}.{i}.{nameof(foodData.Prefab)} '{foodData.Prefab}' not found");
                        error = true;
                    }
                    else
                    {
                        if (Helpers.PrefabHelper.GetItemDropByPrefab(foodData.Prefab) == null)
                        {
                            Plugin.LogFatal($"{model}.{nameof(data.MonsterAI)}.{nameof(data.MonsterAI.ConsumeItems)}.{i}.{nameof(foodData.Prefab)} '{foodData.Prefab}' has no ItemDrop component");
                            error = true;
                        }
                    }
                }
            }

            if (data.Procreation != null)
            {

                foreach (var (partnerData, i) in data.Procreation.Partner.Select((value, i) => (value, i)))
                {
                    if (!ctx.PrefabExists(partnerData.Prefab))
                    {
                        Plugin.LogFatal($"{model}.{nameof(data.Procreation)}.{nameof(data.Procreation.Partner)}.{i}.{nameof(partnerData.Prefab)} '{partnerData.Prefab}' not found");
                        error = true;
                    }
                }

                foreach (var (offspringData, i) in data.Procreation.Offspring.Select((value, i) => (value, i)))
                {

                    if (!ctx.PrefabExists(offspringData.Prefab))
                    {
                        Plugin.LogFatal($"{model}.{nameof(data.Procreation)}.{nameof(data.Procreation.Offspring)}.{i}.{nameof(offspringData.Prefab)} '{offspringData.Prefab}' not found");
                        error = true;
                    }

                    if (offspringData.NeedPartner == false)
                    {
                        offspringData.NeedPartnerPrefab = null;
                    }

                    if (offspringData.NeedPartnerPrefab != null)
                    {
                        if (!ctx.PrefabExists(offspringData.NeedPartnerPrefab))
                        {
                            Plugin.LogFatal($"{model}.{nameof(data.Procreation)}.{nameof(data.Procreation.Offspring)}.{i}.{nameof(offspringData.NeedPartnerPrefab)} '{offspringData.NeedPartnerPrefab}' not found");
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

        public override void RegisterPrefab(ModelHandlerContext ctx, string creatureName, Creature data)
        {
            var model = $"{nameof(Creature)}.{creatureName}";

            var creature = PrefabManager.Instance.GetPrefab(creatureName);
            // no need to register, already registered

            var monsterAI = creature.GetComponent<MonsterAI>();
            var animalAI = creature.GetComponent<AnimalAI>();
            var tameable = creature.GetComponent<Tameable>();
            var procreation = creature.GetComponent<Procreation>();
            var character = creature.GetComponent<Character>();
            var pet = creature.GetComponent<Pet>();

            var characterData = data.Character;
            var monsterAIData = data.MonsterAI;
            var tameableData = data.Tameable;
            var procreationData = data.Procreation;

            var idleSoundPrefab = Helpers.PrefabHelper.FindEffectPrefab<BaseAI>(creatureName, "m_idleSound", 0);

            if (monsterAIData != null)
            {

                if (monsterAI != null)
                {

                    Plugin.LogDebug($"{model}.{nameof(data.MonsterAI)}: Setting MonsterAI values");

                    monsterAI.m_consumeRange = (float)monsterAIData.ConsumeRange;
                    monsterAI.m_consumeSearchRange = (float)monsterAIData.ConsumeSearchRange;
                    monsterAI.m_consumeSearchInterval = (float)monsterAIData.ConsumeSearchInterval;
                    monsterAI.m_consumeItems = new List<ItemDrop>();
                    foreach (var entry in monsterAIData.ConsumeItems)
                    {
                        var itemDrop = Helpers.PrefabHelper.GetItemDropByPrefab(entry.Prefab);
                        if (itemDrop != null)
                        {
                            monsterAI.m_consumeItems.Add(itemDrop);
                        }
                    }

                }
                else
                {
                    // we need to make fake MonsterAI
                    if (animalAI != null)
                    {
                        Plugin.LogDebug($"{model}.{nameof(data.MonsterAI)}: Setting AnimalAIAPI values");

                        var prefabAnimalAIAPI = Internals.AnimalAIAPI.GetOrCreate(animalAI);
                        prefabAnimalAIAPI.m_consumeRange = (float)monsterAIData.ConsumeRange;
                        prefabAnimalAIAPI.m_consumeSearchRange = (float)monsterAIData.ConsumeSearchRange;
                        prefabAnimalAIAPI.m_consumeSearchInterval = (float)monsterAIData.ConsumeSearchInterval;
                        prefabAnimalAIAPI.m_consumeItems = new List<ItemDrop>();

                        foreach (var entry in monsterAIData.ConsumeItems)
                        {
                            var itemDrop = Helpers.PrefabHelper.GetItemDropByPrefab(entry.Prefab);
                            if (itemDrop != null)
                            {
                                prefabAnimalAIAPI.m_consumeItems.Add(itemDrop);
                            }
                        }
                    }
                }
            }

            if (characterData != null)
            {
                if (characterData.Group != null)
                {
                    character.m_group = characterData.Group;
                }

                if (data.Character.StickToFaction)
                {
                    Patches.Contexts.DataContext.SetObjectSticksToFaction(creatureName);
                }
                if (data.Character.CanAttackTames)
                {
                    Patches.Contexts.DataContext.SetObjectCanAttackTames(creatureName);
                }
                
            }

            if (tameableData != null)
            {
                if (!tameable)
                {
                    Plugin.LogDebug($"{model}.{nameof(data.Tameable)}: Adding Tameable component");
                    tameable = creature.AddComponent<Tameable>();
                }
                if (!pet)
                {
                    Plugin.LogDebug($"{model}.{nameof(data.Tameable)}: Adding Pet component");
                    pet = creature.AddComponent<Pet>();
                }

                Plugin.LogDebug($"{model}.{nameof(data.Tameable)}: Setting Tameable values");
                tameable.m_fedDuration = tameableData.FedDuration;
                tameable.m_tamingTime = tameableData.TamingTime;
                tameable.m_commandable = tameableData.Commandable;

                Plugin.LogDebug($"{model}.{nameof(data.Tameable)}: Setting effects");
                if (tameable.m_sootheEffect.m_effectPrefabs.Length == 0)
                {
                    tameable.m_sootheEffect = new EffectList
                    {
                        m_effectPrefabs = Helpers.PrefabHelper.GetEffects(new string[] {
                            "vfx_creature_soothed",
                        })
                    };
                }
                if (tameable.m_tamedEffect.m_effectPrefabs.Length == 0)
                {
                    tameable.m_tamedEffect = new EffectList
                    {
                        m_effectPrefabs = Helpers.PrefabHelper.GetEffects(new string[] {
                            "fx_creature_tamed",
                        })
                    };
                }
                if (tameable.m_petEffect.m_effectPrefabs.Length == 0)
                {
                    tameable.m_petEffect = new EffectList
                    {
                        m_effectPrefabs = Helpers.PrefabHelper.GetEffects(new string[] {
                            //"vfx_boar_love",
                            "fx_boar_pet",
                            idleSoundPrefab?.name,
                            //"fx_creature_tamed",
                            //"vfx_creature_soothed",
                        })
                    };
                }
                
            }
            else
            {
                if (tameable)
                {
                    Plugin.LogDebug($"{model}.{nameof(data.Tameable)}: Removing Tameable component");
                    UnityEngine.Object.DestroyImmediate(tameable);
                }
            }

            if (procreationData != null)
            {
                if (!procreation)
                {
                    Plugin.LogDebug($"{model}.{nameof(data.Procreation)}: Adding Procreation component");
                    procreation = creature.AddComponent<Procreation>();
                }

                Plugin.LogDebug($"{model}.{nameof(data.Procreation)}: Setting Procreation values");

                procreation.m_updateInterval = procreationData.UpdateInterval;
                procreation.m_totalCheckRange = procreationData.TotalCheckRange;
                procreation.m_maxCreatures = 3; // set default for fallback, but should be handled in procreate-prepatch
                procreation.m_partnerCheckRange = procreationData.PartnerCheckRange;
                procreation.m_requiredLovePoints = procreationData.RequiredLovePoints;
                procreation.m_pregnancyChance = procreationData.PregnancyChance;
                procreation.m_pregnancyDuration = procreationData.PregnancyDuration;
                procreation.m_spawnOffset = procreationData.SpawnOffset;
                procreation.m_spawnOffsetMax = procreationData.SpawnOffsetMax;
                procreation.m_spawnRandomDirection = procreationData.SpawnRandomDirection;

                // sibling chance will be handled in ProcreationAPI
                //procreationData.siblingChance = Mathf.Clamp(procreationData.siblingChance, 0, 1);

                Plugin.LogDebug($"{model}.{nameof(data.Procreation)}: Setting effects");

                if (procreation.m_loveEffects.m_effectPrefabs.Length == 0)
                {
                    procreation.m_loveEffects = new EffectList
                    {
                        m_effectPrefabs = Helpers.PrefabHelper.GetEffects(new string[] {
                            idleSoundPrefab?.name,
                            "vfx_boar_love",
                        })
                    };

                    procreation.m_loveEffects.m_effectPrefabs[1].m_scale = true;
                    procreation.m_loveEffects.m_effectPrefabs[1].m_prefab.transform.localScale = Vector3.one * 0.5f;

                }
                if (procreation.m_birthEffects.m_effectPrefabs.Length == 0)
                {
                    procreation.m_birthEffects = new EffectList
                    {
                        m_effectPrefabs = Helpers.PrefabHelper.GetEffects(new string[] {
                            idleSoundPrefab?.name,
                            "vfx_boar_birth",
                        })
                    };
                }

                // will be handled via ProcreationAPI
                procreation.m_offspring = null;
                procreation.m_seperatePartner = null;
            }
            else
            {
                if (procreation)
                {
                    Plugin.LogDebug($"{model}.{nameof(data.Procreation)}: Removing Procreation component");
                    UnityEngine.Object.DestroyImmediate(procreation);
                }
            }

        }

    }

}

