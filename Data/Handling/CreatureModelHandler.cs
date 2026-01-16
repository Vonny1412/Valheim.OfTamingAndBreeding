using Jotunn.Managers;
using OfTamingAndBreeding.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using UnityEngine;

namespace OfTamingAndBreeding.Data.Handling
{
    internal class CreatureModelHandler : ModelHandler<Data.Models.Creature>
    {
        public override string GetDirectoryName() => Data.Models.Creature.GetDirectoryName();

        //------------------------------------------------
        // VALIDATE DATA
        //------------------------------------------------

        public override bool ValidateData(ModelHandlerContext ctx, string creatureName, Data.Models.Creature data)
        {
            var model = $"{nameof(Models.Creature)}.{creatureName}";
            var error = false;

            var characterData = data.Character;
            var monsterAIData = data.MonsterAI;
            var tameableData = data.Tameable;
            var procreationData = data.Procreation;

            if (monsterAIData != null)
            {
                if (monsterAIData.consumeItems == null || monsterAIData.consumeItems.Length == 0)
                {
                    monsterAIData.consumeItems = new Creature.MonsterAIConsumItemData[] { };
                }
            }

            if (procreationData != null)
            {

                if (tameableData == null)
                {
                    Plugin.LogError($"{model}.{nameof(data.Procreation)} requires {nameof(data.Tameable)}");
                    error = true;
                }

                if (procreationData.partner == null || procreationData.partner.Length == 0)
                {
                    procreationData.partner = new Data.Models.Creature.ProcreationPartnerData[] {  };
                }

                foreach (var (partnerData, i) in procreationData.partner.Select((value, i) => (value, i)))
                {
                    partnerData.weight = Math.Max(0f, partnerData.weight);
                    if (partnerData.prefab == null)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Procreation)}.{nameof(procreationData.partner)}.{i}.{nameof(partnerData.prefab)} is empty");
                        error = true;
                    }
                }

                if (procreationData.offspring == null || procreationData.offspring.Length == 0)
                {
                    Plugin.LogError($"{model}.{nameof(data.Procreation)}.{nameof(procreationData.offspring)} is null or empty");
                    error = true;
                }

                foreach (var (offspringData, i) in procreationData.offspring.Select((value, i) => (value, i)))
                {
                    offspringData.weight = Math.Max(0f, offspringData.weight);
                    if (offspringData.prefab == null)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Procreation)}.{nameof(procreationData.offspring)}.{i}.{nameof(offspringData.prefab)} is empty");
                        error = true;
                    }
                }
            }

            return error == false;
        }

        //------------------------------------------------
        // PREPARE PREFAB
        //------------------------------------------------

        public override bool PreparePrefab(ModelHandlerContext ctx, string creatureName, Data.Models.Creature data)
        {
            var model = $"{nameof(Models.Creature)}.{creatureName}";

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

        public override bool ValidatePrefab(ModelHandlerContext ctx, string creatureName, Data.Models.Creature data)
        {
            var model = $"{nameof(Models.Creature)}.{creatureName}";
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
                foreach (var (foodData, i) in data.MonsterAI.consumeItems.Select((value, i) => (value, i)))
                {
                    if (!ctx.PrefabExists(foodData.prefab))
                    {
                        Plugin.LogFatal($"{model}.{nameof(data.MonsterAI)}.{nameof(data.MonsterAI.consumeItems)}.{i}.{nameof(foodData.prefab)} '{foodData.prefab}' not found");
                        error = true;
                    }
                }
            }

            if (data.Procreation != null)
            {

                foreach (var (partnerData, i) in data.Procreation.partner.Select((value, i) => (value, i)))
                {
                    if (!ctx.PrefabExists(partnerData.prefab))
                    {
                        Plugin.LogFatal($"{model}.{nameof(data.Procreation)}.{nameof(data.Procreation.partner)}.{i}.{nameof(partnerData.prefab)} '{partnerData.prefab}' not found");
                        error = true;
                    }
                }

                foreach (var (offspringData, i) in data.Procreation.offspring.Select((value, i) => (value, i)))
                {

                    if (!ctx.PrefabExists(offspringData.prefab))
                    {
                        Plugin.LogFatal($"{model}.{nameof(data.Procreation)}.{nameof(data.Procreation.offspring)}.{i}.{nameof(offspringData.prefab)} '{offspringData.prefab}' not found");
                        error = true;
                    }

                    if (offspringData.needPartnerPrefab != null)
                    {
                        if (!ctx.PrefabExists(offspringData.needPartnerPrefab))
                        {
                            Plugin.LogFatal($"{model}.{nameof(data.Procreation)}.{nameof(data.Procreation.offspring)}.{i}.{nameof(offspringData.needPartnerPrefab)} '{offspringData.needPartnerPrefab}' not found");
                            error = true;
                        }
                    }
                    /*
                    if (offspringData.needFoodPrefab != null)
                    {
                        if (!ctx.PrefabExists(offspringData.needFoodPrefab))
                        {
                            Plugin.Log.LogFatal($"{model}.{nameof(data.Procreation)}.{nameof(data.Procreation.offspring)}.{i}.{nameof(offspringData.needFoodPrefab)} '{offspringData.needFoodPrefab}' not found");
                            error = true;
                        }
                    }
                    */
                }
            }

            return error == false;
        }

        //------------------------------------------------
        // REGISTER PREFAB
        //------------------------------------------------

        public override void RegisterPrefab(ModelHandlerContext ctx, string creatureName, Data.Models.Creature data)
        {
            var model = $"{nameof(Models.Creature)}.{creatureName}";

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

                    monsterAI.m_consumeRange = (float)monsterAIData.consumeRange;
                    monsterAI.m_consumeSearchRange = (float)monsterAIData.consumeSearchRange;
                    monsterAI.m_consumeSearchInterval = (float)monsterAIData.consumeSearchInterval;
                    monsterAI.m_consumeItems = new List<ItemDrop>();
                    foreach (var entry in monsterAIData.consumeItems)
                    {
                        var itemDrop = Helpers.PrefabHelper.GetItemDropByPrefabId(entry.prefab);
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
                        prefabAnimalAIAPI.m_consumeRange = (float)monsterAIData.consumeRange;
                        prefabAnimalAIAPI.m_consumeSearchRange = (float)monsterAIData.consumeSearchRange;
                        prefabAnimalAIAPI.m_consumeSearchInterval = (float)monsterAIData.consumeSearchInterval;
                        prefabAnimalAIAPI.m_consumeItems = new List<ItemDrop>();

                        foreach (var entry in monsterAIData.consumeItems)
                        {
                            var itemDrop = Helpers.PrefabHelper.GetItemDropByPrefabId(entry.prefab);
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
                if (characterData.group != null)
                {
                    character.m_group = characterData.group;
                }

                if (data.Character.stickToFaction)
                {
                    Patches.Contexts.IsEnemyContext.prefabSticksToFaction.Add(creatureName.GetStableHashCode());
                }
                if (data.Character.attacksTames)
                {
                    Patches.Contexts.IsEnemyContext.prefabAttacksTames.Add(creatureName.GetStableHashCode());
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
                tameable.m_fedDuration = tameableData.fedDuration;
                tameable.m_tamingTime = tameableData.tamingTime;
                tameable.m_commandable = tameableData.commandable;

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

                procreation.m_updateInterval = procreationData.updateInterval;
                procreation.m_totalCheckRange = procreationData.totalCheckRange;
                procreation.m_maxCreatures = 3; // set default for fallback, but should be handled in procreate-prepatch
                procreation.m_partnerCheckRange = procreationData.partnerCheckRange;
                procreation.m_requiredLovePoints = procreationData.requiredLovePoints;
                procreation.m_pregnancyChance = procreationData.pregnancyChance;
                procreation.m_pregnancyDuration = procreationData.pregnancyDuration;
                procreation.m_spawnOffset = procreationData.spawnOffset;
                procreation.m_spawnOffsetMax = procreationData.spawnOffsetMax;
                procreation.m_spawnRandomDirection = procreationData.spawnRandomDirection;

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

