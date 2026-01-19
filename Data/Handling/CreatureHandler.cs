using Jotunn.Managers;
using OfTamingAndBreeding.Data.Handling.Base;
using OfTamingAndBreeding.Data.Models;
using OfTamingAndBreeding.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
using YamlDotNet.Core;
using YamlDotNet.Core.Tokens;
using static OfTamingAndBreeding.Data.Models.Creature;

namespace OfTamingAndBreeding.Data.Handling
{
    internal class CreatureHandler : DataHandler<Models.Creature>
    {
        public override string DirectoryName => Models.Creature.DirectoryName;

        //------------------------------------------------
        // PREPARE
        //------------------------------------------------

        public override void Prepare(DataHandlerContext ctx)
        {

        }

        //------------------------------------------------
        // VALIDATE DATA
        //------------------------------------------------

        public override bool ValidateData(DataHandlerContext ctx, string creatureName, Models.Creature data)
        {
            var model = $"{nameof(Models.Creature)}.{creatureName}";
            var error = false;

            var characterData = data.Character;
            var monsterAIData = data.MonsterAI;
            var tameableData = data.Tameable;
            var procreationData = data.Procreation;

            if (monsterAIData != null)
            {
                if (monsterAIData.ConsumeItems == null)
                {
                    //monsterAIData.ConsumeItems = new Models.Creature.MonsterAIConsumItemData[] { };
                }
            }

            if (procreationData != null)
            {

                if (tameableData == null)
                {
                    Plugin.LogError($"{model}.{nameof(data.Procreation)} requires {nameof(data.Tameable)}");
                    error = true;
                }

                if (procreationData.MaxCreaturesExplicite == null)
                {
                    procreationData.MaxCreaturesExplicite = new Dictionary<string, int>();
                }

                if (procreationData.Partner == null)
                {
                    // for no-partner procreation
                    procreationData.Partner = new Models.Creature.ProcreationPartnerData[] { };
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
                else
                {
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

            }

            return error == false;
        }

        //------------------------------------------------
        // PREPARE PREFAB
        //------------------------------------------------

        public override bool PreparePrefab(DataHandlerContext ctx, string creatureName, Models.Creature data)
        {
            var model = $"{nameof(Models.Creature)}.{creatureName}";

            var creature = ctx.GetPrefab(creatureName);
            if (!creature)
            {
                Plugin.LogError($"{model}: Prefab not found");
                return false;
            }

            ctx.MakeBackup(creatureName, creature);

            ctx.CachePrefab(creatureName, creature);
            return true;
        }

        //------------------------------------------------
        // VALIDATE PREFAB
        //------------------------------------------------

        public override bool ValidatePrefab(DataHandlerContext ctx, string creatureName, Models.Creature data)
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
                if (data.MonsterAI.ConsumeItems != null)
                {
                    foreach (var (foodData, i) in data.MonsterAI.ConsumeItems.Select((value, i) => (value, i)))
                    {
                        if (!ctx.PrefabExists(foodData.Prefab))
                        {
                            Plugin.LogFatal($"{model}.{nameof(data.MonsterAI)}.{nameof(data.MonsterAI.ConsumeItems)}.{i}.{nameof(foodData.Prefab)}: '{foodData.Prefab}' not found");
                            error = true;
                        }
                        else
                        {
                            if (GetItemDropByPrefab(foodData.Prefab) == null)
                            {
                                Plugin.LogFatal($"{model}.{nameof(data.MonsterAI)}.{nameof(data.MonsterAI.ConsumeItems)}.{i}.{nameof(foodData.Prefab)}: '{foodData.Prefab}' has no ItemDrop component");
                                error = true;
                            }
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
                        Plugin.LogFatal($"{model}.{nameof(data.Procreation)}.{nameof(data.Procreation.Partner)}.{i}.{nameof(partnerData.Prefab)}: '{partnerData.Prefab}' not found");
                        error = true;
                    }
                }

                foreach (var (offspringData, i) in data.Procreation.Offspring.Select((value, i) => (value, i)))
                {

                    if (!ctx.PrefabExists(offspringData.Prefab))
                    {
                        Plugin.LogFatal($"{model}.{nameof(data.Procreation)}.{nameof(data.Procreation.Offspring)}.{i}.{nameof(offspringData.Prefab)}: '{offspringData.Prefab}' not found");
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
                            Plugin.LogFatal($"{model}.{nameof(data.Procreation)}.{nameof(data.Procreation.Offspring)}.{i}.{nameof(offspringData.NeedPartnerPrefab)}: '{offspringData.NeedPartnerPrefab}' not found");
                            error = true;
                        }
                    }
                }

                foreach (var (kv, i) in data.Procreation.MaxCreaturesExplicite.Select((value, i) => (value, i)))
                {
                    if (!ctx.PrefabExists(kv.Key))
                    {
                        Plugin.LogFatal($"{model}.{nameof(data.Procreation)}.{nameof(data.Procreation.MaxCreaturesExplicite)}.{i}: '{kv.Key}' not found");
                        error = true;
                    }
                }
                

            }

            return error == false;
        }

        //------------------------------------------------
        // REGISTER PREFAB
        //------------------------------------------------

        public override void RegisterPrefab(DataHandlerContext ctx, string creatureName, Models.Creature data)
        {
            var model = $"{nameof(Models.Creature)}.{creatureName}";

            var creature = ctx.GetPrefab(creatureName);
            // no need to register, already registered

            var monsterAI = creature.GetComponent<MonsterAI>();
            var animalAI = creature.GetComponent<AnimalAI>();
            var character = creature.GetComponent<Character>();

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

                    monsterAI.m_consumeRange = monsterAIData.ConsumeRange;
                    monsterAI.m_consumeSearchRange = monsterAIData.ConsumeSearchRange;
                    monsterAI.m_consumeSearchInterval = monsterAIData.ConsumeSearchInterval;
                    if (monsterAIData.ConsumeItems != null)
                    {
                        monsterAI.m_consumeItems = new List<ItemDrop>();
                        foreach (var entry in monsterAIData.ConsumeItems)
                        {
                            var itemDrop = GetItemDropByPrefab(entry.Prefab);
                            if (itemDrop != null)
                            {
                                monsterAI.m_consumeItems.Add(itemDrop);
                            }
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

                        prefabAnimalAIAPI.m_consumeRange = monsterAIData.ConsumeRange;
                        prefabAnimalAIAPI.m_consumeSearchRange = monsterAIData.ConsumeSearchRange;
                        prefabAnimalAIAPI.m_consumeSearchInterval = monsterAIData.ConsumeSearchInterval;
                        if (monsterAIData.ConsumeItems != null)
                        {
                            prefabAnimalAIAPI.m_consumeItems = new List<ItemDrop>();
                            foreach (var entry in monsterAIData.ConsumeItems)
                            {
                                var itemDrop = GetItemDropByPrefab(entry.Prefab);
                                if (itemDrop != null)
                                {
                                    prefabAnimalAIAPI.m_consumeItems.Add(itemDrop);
                                }
                            }
                        }
                    }
                }
            }

            if (characterData != null)
            {

                if (characterData.Group != null) character.m_group = characterData.Group;
                
                if (data.Character.StickToFaction) Patches.Contexts.DataContext.SetObjectSticksToFaction(creatureName);
                if (data.Character.CanAttackTames) Patches.Contexts.DataContext.SetObjectCanAttackTames(creatureName);
                if (data.Character.CanBeAttackedByTames) Patches.Contexts.DataContext.SetObjectCanBeAttackedByTames(creatureName);
                
            }

            if (tameableData != null)
            {
                var tameable = ctx.GetOrAddComponent<Tameable>(creatureName, creature);
                var pet = ctx.GetOrAddComponent<Pet>(creatureName, creature);
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
                            "fx_boar_pet",
                            idleSoundPrefab?.name,
                        })
                    };
                }
                
            }
            else
            {
                Plugin.LogDebug($"{model}.{nameof(Tameable)}: Removing Tameable component (if exist)");
                ctx.DestroyComponentIfExists<Tameable>(creatureName, creature);
            }

            if (procreationData != null)
            {
                var procreation = ctx.GetOrAddComponent<Procreation>(creatureName, creature);
                Plugin.LogDebug($"{model}.{nameof(data.Procreation)}: Setting Procreation values");

                procreation.m_updateInterval = (float)procreationData.UpdateInterval;
                procreation.m_totalCheckRange = (float)procreationData.TotalCheckRange;
                procreation.m_partnerCheckRange = (float)procreationData.PartnerCheckRange;
                procreation.m_requiredLovePoints = (int)procreationData.RequiredLovePoints;
                procreation.m_pregnancyChance = (float)procreationData.PregnancyChance;
                procreation.m_pregnancyDuration = (float)procreationData.PregnancyDuration;
                procreation.m_spawnOffset = (float)procreationData.SpawnOffset;
                procreation.m_spawnOffsetMax = (float)procreationData.SpawnOffsetMax;
                procreation.m_spawnRandomDirection = (bool)procreationData.SpawnRandomDirection;
                procreation.m_maxCreatures = (int)procreationData.MaxCreatures;

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
                Plugin.LogDebug($"{model}.{nameof(Procreation)}: Removing Procreation component (if exist)");
                ctx.DestroyComponentIfExists<Procreation>(creatureName, creature);
            }

        }

        //------------------------------------------------
        // CLEANUP
        //------------------------------------------------

        public override void Cleanup(DataHandlerContext ctx)
        {
            prefabItemDrops.Clear(); // free some memory
        }

        //------------------------------------------------
        // UNREGISTER PREFAB
        //------------------------------------------------

        public override void RestorePrefab(DataHandlerContext ctx, string creatureName, Models.Creature data)
        {
            ctx.Restore(creatureName, (GameObject backup, GameObject current) => {

                RestoreHelper.RestoreComponent<Character>(backup, current);
                RestoreHelper.RestoreComponent<MonsterAI>(backup, current);
                RestoreHelper.RestoreComponent<AnimalAI>(backup, current);
                RestoreHelper.RestoreComponent<Tameable>(backup, current);
                RestoreHelper.RestoreComponent<Pet>(backup, current);
                RestoreHelper.RestoreComponent<Procreation>(backup, current);

            });
        }

        //------------------------------------------------
        // HELPERS
        //------------------------------------------------

        private static readonly Dictionary<int, ItemDrop> prefabItemDrops = new Dictionary<int, ItemDrop>();

        private static ItemDrop GetItemDropByPrefab(string prefabName)
        {
            var hash = prefabName.GetStableHashCode();
            if (prefabItemDrops.TryGetValue(hash, out ItemDrop itemDrop))
            {
                return itemDrop;
            }
            GameObject prefab = ObjectDB.instance.GetItemPrefab(prefabName);
            if (prefab == null)
            {
                prefabItemDrops.Add(hash, null);
                return null;
            }
            itemDrop = prefab.GetComponent<ItemDrop>();
            if (itemDrop == null)
            {
                prefabItemDrops.Add(hash, null);
                return null;
            }
            prefabItemDrops.Add(hash, itemDrop);
            return itemDrop;
        }

    }

}

