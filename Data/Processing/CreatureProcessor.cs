using OfTamingAndBreeding.Helpers;
using OfTamingAndBreeding.ValheimAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace OfTamingAndBreeding.Data.Processing
{
    internal class CreatureProcessor : Base.DataProcessor<Models.Creature>
    {
        public override string DirectoryName => Models.Creature.DirectoryName;

        public override string GetDataKey(string filePath) => null;


        //------------------------------------------------
        // PREPARE
        //------------------------------------------------

        public override void Prepare(Base.DataProcessorContext ctx)
        {

        }

        //------------------------------------------------
        // VALIDATE DATA
        //------------------------------------------------

        public override bool ValidateData(Base.DataProcessorContext ctx, string creatureName, Models.Creature data)
        {
            var model = $"{nameof(Models.Creature)}.{creatureName}";
            var error = false;

            switch (data.Components.Character)
            {
                case Models.SubData.ComponentBehavior.Remove:
                    Plugin.LogDebug($"{model}.{nameof(data.Components)}.{nameof(data.Components.Character)}({nameof(Models.SubData.ComponentBehavior.Remove)}): Component cannot be removed");
                    break;
                case Models.SubData.ComponentBehavior.Patch:
                    if (data.Character == null)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Components)}.{nameof(data.Components.Character)}({nameof(Models.SubData.ComponentBehavior.Patch)}): Missing component data");
                        error = true;
                    }
                    break;
                case Models.SubData.ComponentBehavior.Inherit:
                    if (data.Character != null)
                    {
                        Plugin.LogDebug($"{model}.{nameof(data.Components)}.{nameof(data.Components.Character)}({nameof(Models.SubData.ComponentBehavior.Inherit)}): Component data will be ignored");
                    }
                    break;
            }

            switch (data.Components.MonsterAI)
            {
                case Models.SubData.ComponentBehavior.Remove:
                    Plugin.LogDebug($"{model}.{nameof(data.Components)}.{nameof(data.Components.MonsterAI)}({nameof(Models.SubData.ComponentBehavior.Remove)}): Component cannot be removed");
                    break;
                case Models.SubData.ComponentBehavior.Patch:
                    if (data.MonsterAI == null)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Components)}.{nameof(data.Components.MonsterAI)}({nameof(Models.SubData.ComponentBehavior.Patch)}): Missing component data");
                        error = true;
                    }
                    break;
                case Models.SubData.ComponentBehavior.Inherit:
                    if (data.MonsterAI != null)
                    {
                        Plugin.LogDebug($"{model}.{nameof(data.Components)}.{nameof(data.Components.MonsterAI)}({nameof(Models.SubData.ComponentBehavior.Inherit)}): Component data will be ignored");
                    }
                    break;
            }

            switch (data.Components.Tameable)
            {
                case Models.SubData.ComponentBehavior.Patch:
                    if (data.Tameable == null)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Components)}.{nameof(data.Components.Tameable)}({nameof(Models.SubData.ComponentBehavior.Patch)}): Missing component data");
                        error = true;   
                    }
                    break;
                case Models.SubData.ComponentBehavior.Inherit:
                    if (data.Tameable != null)
                    {
                        Plugin.LogDebug($"{model}.{nameof(data.Components)}.{nameof(data.Components.Tameable)}({nameof(Models.SubData.ComponentBehavior.Inherit)}): Component data will be ignored");
                    }
                    break;
            }

            switch (data.Components.Procreation)
            {
                case Models.SubData.ComponentBehavior.Patch:
                    if (data.Procreation == null)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Components)}.{nameof(data.Components.Procreation)}({nameof(Models.SubData.ComponentBehavior.Patch)}): Missing component data");
                        error = true;
                    }
                    break;
                case Models.SubData.ComponentBehavior.Inherit:
                    if (data.Procreation != null)
                    {
                        Plugin.LogDebug($"{model}.{nameof(data.Components)}.{nameof(data.Components.Procreation)}({nameof(Models.SubData.ComponentBehavior.Inherit)}): Component data will be ignored");
                    }
                    break;
            }

            if (data.Character != null && data.Components.Character == Models.SubData.ComponentBehavior.Patch)
            {
                // nothing to validate here
            }

            if (data.MonsterAI != null && data.Components.MonsterAI == Models.SubData.ComponentBehavior.Patch)
            {
                if (data.MonsterAI.ConsumeItems == null)
                {
                    // null = keep original
                    // empty list = clear: wont eat anything
                }
            }

            if (data.Tameable != null && data.Components.Tameable == Models.SubData.ComponentBehavior.Patch)
            {
                // nothing to validate here
            }

            if (data.Procreation != null && data.Components.Procreation == Models.SubData.ComponentBehavior.Patch)
            {

                if (data.Tameable == null)
                {
                    // we gonna check this in prefab validation
                }

                if (data.Procreation.MaxCreaturesCountPrefabs == null)
                {
                    // if == null then this feature is just disabled
                }

                if (data.Procreation.Partner == null || data.Procreation.Partner.Length == 0)
                {
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

                if (data.Procreation.Offspring == null || data.Procreation.Offspring.Length == 0)
                {
                    Plugin.LogError($"{model}.{nameof(data.Procreation)}.{nameof(data.Procreation.Offspring)}: Field is null or empty");
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
                    }
                }

            }

            return error == false;
        }

        //------------------------------------------------
        // PREPARE PREFAB
        //------------------------------------------------

        public override bool PreparePrefab(Base.DataProcessorContext ctx, string creatureName, Models.Creature data)
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

        public override bool ValidatePrefab(Base.DataProcessorContext ctx, string creatureName, Models.Creature data)
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

                var hasProcreation = (bool)creature.GetComponent<Procreation>();
                var hasTameable = (bool)creature.GetComponent<Tameable>();
                bool wantProcreationActive = data.Components.Procreation == Models.SubData.ComponentBehavior.Patch || (hasProcreation && data.Components.Procreation != Models.SubData.ComponentBehavior.Remove);
                bool wantTameableActive = data.Components.Tameable != Models.SubData.ComponentBehavior.Remove && (hasTameable || data.Components.Tameable == Models.SubData.ComponentBehavior.Patch);
                if (wantProcreationActive && !wantTameableActive)
                {
                    Plugin.LogWarning($"{model}.{nameof(data.Procreation)}: Component requires {nameof(data.Tameable)}");
                    //error = true;
                }

            }

            if (data.MonsterAI != null && data.Components.MonsterAI == Models.SubData.ComponentBehavior.Patch)
            {
                if (data.MonsterAI.ConsumeItems != null)
                {
                    foreach (var (foodData, i) in data.MonsterAI.ConsumeItems.Select((value, i) => (value, i)))
                    {
                        if (!ctx.PrefabExists(foodData.Prefab))
                        {
                            Plugin.LogError($"{model}.{nameof(data.MonsterAI)}.{nameof(data.MonsterAI.ConsumeItems)}.{i}.{nameof(foodData.Prefab)}: '{foodData.Prefab}' not found");
                            error = true;
                        }
                        else
                        {
                            if (Runtime.MonsterAI.GetItemDropByPrefab(foodData.Prefab) == null)
                            {
                                Plugin.LogError($"{model}.{nameof(data.MonsterAI)}.{nameof(data.MonsterAI.ConsumeItems)}.{i}.{nameof(foodData.Prefab)}: '{foodData.Prefab}' has no ItemDrop component");
                                error = true;
                            }
                        }
                    }
                }
            }

            if (data.Procreation != null && data.Components.Procreation == Models.SubData.ComponentBehavior.Patch)
            {

                if (data.Procreation.Partner != null)
                {
                    foreach (var (partnerData, i) in data.Procreation.Partner.Select((value, i) => (value, i)))
                    {
                        if (!ctx.PrefabExists(partnerData.Prefab))
                        {
                            Plugin.LogError($"{model}.{nameof(data.Procreation)}.{nameof(data.Procreation.Partner)}.{i}.{nameof(partnerData.Prefab)}: '{partnerData.Prefab}' not found");
                            error = true;
                        }
                    }
                }

                foreach (var (offspringData, i) in data.Procreation.Offspring.Select((value, i) => (value, i)))
                {

                    if (!ctx.PrefabExists(offspringData.Prefab))
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
                        if (!ctx.PrefabExists(offspringData.NeedPartnerPrefab))
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
                        if (!ctx.PrefabExists(prefabName))
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

        public override void RegisterPrefab(Base.DataProcessorContext ctx, string creatureName, Models.Creature data)
        {
            var model = $"{nameof(Models.Creature)}.{creatureName}";
            var creature = ctx.GetPrefab(creatureName);

            var idleSoundPrefab = Helpers.EffectsHelper.FindEffectPrefab<BaseAI>(creatureName, "m_idleSound", 0);

            if (data.Components.Character == Models.SubData.ComponentBehavior.Patch)
            {
                var character = creature.GetComponent<Character>();
                if (data.Character != null)
                {
                    Plugin.LogDebug($"{model}.{nameof(data.Character)}: Setting Character values");
                    if (data.Character.Group != null) character.m_group = data.Character.Group;
                    if (data.Character.GroupWhenTamed != null) Runtime.Character.SetGroupWhenTamed(creatureName, data.Character.GroupWhenTamed);
                    if (data.Character.TamesStickToFaction) Runtime.Character.SetSticksToFaction(creatureName);
                    Runtime.Character.SetCanAttackTames(creatureName, data.Character.TamesCanAttackTames);
                    Runtime.Character.SetCanBeAttackedByTames(creatureName, data.Character.TamesCanBeAttackedByTames);
                    Runtime.Character.SetCanAttackPlayer(creatureName, data.Character.TamesCanAttackPlayer);
                }
            }
            else if (data.Components.Character == Models.SubData.ComponentBehavior.Remove)
            {
                // ignore, cannot be removed
            }

            if (data.Components.MonsterAI == Models.SubData.ComponentBehavior.Patch)
            {
                if (data.MonsterAI != null)
                {
                    var monsterAI = creature.GetComponent<MonsterAI>();
                    var animalAI = creature.GetComponent<AnimalAI>();

                    if (monsterAI != null)
                    {

                        Plugin.LogDebug($"{model}.{nameof(data.MonsterAI)}: Setting MonsterAI values");

                        if (data.MonsterAI.ConsumeRange != null) monsterAI.m_consumeRange = (float)data.MonsterAI.ConsumeRange;
                        if (data.MonsterAI.ConsumeSearchRange != null) monsterAI.m_consumeSearchRange = (float)data.MonsterAI.ConsumeSearchRange;
                        if (data.MonsterAI.ConsumeSearchInterval != null) monsterAI.m_consumeSearchInterval = (float)data.MonsterAI.ConsumeSearchInterval;

                        if (data.MonsterAI.ConsumeItems != null)
                        {
                            data.MonsterAI.ConsumeItems = data.MonsterAI.ConsumeItems
                                .OrderByDescending(i => i.FedDurationFactor)
                                .ToArray();

                            Runtime.MonsterAI.SetCustomConsumeItems(creatureName, data.MonsterAI.ConsumeItems);

                            monsterAI.m_consumeItems = new List<ItemDrop>();
                            foreach (var entry in data.MonsterAI.ConsumeItems)
                            {
                                var itemDrop = Runtime.MonsterAI.GetItemDropByPrefab(entry.Prefab);
                                if (itemDrop != null)
                                {
                                    monsterAI.m_consumeItems.Add(itemDrop);
                                }
                            }
                        }
                    }
                    else
                    {
                        // we need to make pseudo MonsterAI
                        if (animalAI != null)
                        {
                            Plugin.LogDebug($"{model}.{nameof(data.MonsterAI)}: Setting custom AnimalAI values");

                            var prefabAnimalAIData = animalAI.CreateExtraData();

                            if (data.MonsterAI.ConsumeRange != null) prefabAnimalAIData.m_consumeRange = (float)data.MonsterAI.ConsumeRange;
                            if (data.MonsterAI.ConsumeSearchRange != null) prefabAnimalAIData.m_consumeSearchRange = (float)data.MonsterAI.ConsumeSearchRange;
                            if (data.MonsterAI.ConsumeSearchInterval != null) prefabAnimalAIData.m_consumeSearchInterval = (float)data.MonsterAI.ConsumeSearchInterval;

                            if (data.MonsterAI.ConsumeItems != null)
                            {
                                prefabAnimalAIData.m_consumeItems = new List<ItemDrop>();
                                foreach (var entry in data.MonsterAI.ConsumeItems)
                                {
                                    var itemDrop = Runtime.MonsterAI.GetItemDropByPrefab(entry.Prefab);
                                    if (itemDrop != null)
                                    {
                                        prefabAnimalAIData.m_consumeItems.Add(itemDrop);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (data.Components.MonsterAI == Models.SubData.ComponentBehavior.Remove)
            {
                // ignore, cannot be removed
            }


            if (data.Components.Tameable == Models.SubData.ComponentBehavior.Patch)
            {
                if (data.Tameable != null)
                {
                    var tameable = ctx.GetOrAddComponent<Tameable>(creatureName, creature);
                    var pet = ctx.GetOrAddComponent<Pet>(creatureName, creature); // aso neccessary
                    Plugin.LogDebug($"{model}.{nameof(data.Tameable)}: Setting Tameable values");

                    if (data.Tameable.Commandable != null) tameable.m_commandable = (bool)data.Tameable.Commandable;
                    if (data.Tameable.TamingTime != null)
                    {
                        var tamingTime = (float)data.Tameable.TamingTime;
                        if (tamingTime >= 0)
                        {
                            // tameable (even if its 0, maybe any other addon uses instant taming?)
                            Runtime.Tameable.SetBaseTamingTime(creatureName, tamingTime);
                        }
                        else
                        {
                            // not tameable -> hide statustext
                            Runtime.Tameable.SetTamingDisabled(creatureName);
                        }
                        tameable.m_tamingTime = tamingTime >= 0 ? tamingTime : 0; // better clamp. dunno if other mods can handle negative values
                    }
                    if (data.Tameable.FedDuration != null)
                    {
                        var fedDuration = (float)data.Tameable.FedDuration;
                        if (fedDuration >= 0)
                        {
                            // can eat
                            Runtime.Tameable.SetBaseFedDuration(creatureName, fedDuration);
                        }
                        else
                        {
                            // cannot eat, hide all feeding info in hover
                            Runtime.Tameable.SetIsFedTimerDisabled(creatureName);
                        }
                        tameable.m_fedDuration = fedDuration >= 0 ? fedDuration : 0; // better clamp. dunno if other mods can handle negative values
                    }

                    if (data.Tameable.StarvingGraceFactor != null)
                    {
                        Runtime.Tameable.SetStarvingGraceFactor(creatureName, (float)data.Tameable.StarvingGraceFactor);
                    }

                    Plugin.LogDebug($"{model}.{nameof(data.Tameable)}: Setting effects");
                    if (tameable.m_sootheEffect.m_effectPrefabs.Length == 0)
                    {
                        tameable.m_sootheEffect = new EffectList
                        {
                            m_effectPrefabs = Helpers.EffectsHelper.GetEffectList(new string[] {
                                "vfx_creature_soothed",
                            })
                        };
                    }
                    if (tameable.m_tamedEffect.m_effectPrefabs.Length == 0)
                    {
                        tameable.m_tamedEffect = new EffectList
                        {
                            m_effectPrefabs = Helpers.EffectsHelper.GetEffectList(new string[] {
                                "fx_creature_tamed",
                            })
                        };
                    }
                    if (tameable.m_petEffect.m_effectPrefabs.Length == 0)
                    {
                        tameable.m_petEffect = new EffectList
                        {
                            m_effectPrefabs = Helpers.EffectsHelper.GetEffectList(new GameObject[] {
                                EffectsHelper.GetVisualOnlyEffect("fx_boar_pet", "otab_vfx_pet"),
                                idleSoundPrefab,
                            })
                        };
                    }
                }
            }
            else if (data.Components.Tameable == Models.SubData.ComponentBehavior.Remove)
            {
                Plugin.LogDebug($"{model}.{nameof(Tameable)}: Removing Tameable component (if exist)");
                ctx.DestroyComponentIfExists<Tameable>(creatureName, creature);
            }

            if (data.Components.Procreation == Models.SubData.ComponentBehavior.Patch)
            {
                if (data.Procreation != null)
                {
                    var procreation = ctx.GetOrAddComponent<Procreation>(creatureName, creature);
                    Plugin.LogDebug($"{model}.{nameof(data.Procreation)}: Setting Procreation values");

                    if (data.Procreation.UpdateInterval != null) procreation.m_updateInterval = (float)data.Procreation.UpdateInterval;
                    if (data.Procreation.TotalCheckRange != null) procreation.m_totalCheckRange = (float)data.Procreation.TotalCheckRange;

                    if (data.Procreation.PartnerCheckRange != null) procreation.m_partnerCheckRange = (float)data.Procreation.PartnerCheckRange;
                    if (data.Procreation.RequiredLovePoints != null) procreation.m_requiredLovePoints = (int)data.Procreation.RequiredLovePoints;

                    if (data.Procreation.PregnancyChance != null) procreation.m_pregnancyChance = (float)data.Procreation.PregnancyChance;
                    if (data.Procreation.PregnancyDuration != null) procreation.m_pregnancyDuration = (float)data.Procreation.PregnancyDuration;

                    if (data.Procreation.SpawnOffset != null) procreation.m_spawnOffset = (float)data.Procreation.SpawnOffset;
                    if (data.Procreation.SpawnOffsetMax != null) procreation.m_spawnOffsetMax = (float)data.Procreation.SpawnOffsetMax;
                    if (data.Procreation.SpawnRandomDirection != null) procreation.m_spawnRandomDirection = (bool)data.Procreation.SpawnRandomDirection;

                    if (data.Procreation.MaxCreatures != null) procreation.m_maxCreatures = (int)data.Procreation.MaxCreatures;

                    Runtime.Procreation.SetPartnerRecheckTicks(creatureName, data.Procreation.PartnerRecheckSeconds);

                    // sibling chance will be handled in ProcreationAPI
                    //procreationData.siblingChance = Mathf.Clamp(procreationData.siblingChance, 0, 1);

                    Plugin.LogDebug($"{model}.{nameof(data.Procreation)}: Setting effects");

                    if (procreation.m_loveEffects.m_effectPrefabs.Length == 0)
                    {
                        procreation.m_loveEffects = new EffectList
                        {
                            m_effectPrefabs = Helpers.EffectsHelper.GetEffectList(new string[] {
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
                            m_effectPrefabs = Helpers.EffectsHelper.GetEffectList(new string[] {
                            idleSoundPrefab?.name,
                            "vfx_boar_birth",
                        })
                        };
                    }

                    // will be handled via ProcreationAPI
                    procreation.m_offspring = null;
                    procreation.m_seperatePartner = null;

                }
            }
            else if (data.Components.Procreation == Models.SubData.ComponentBehavior.Remove)
            {
                Plugin.LogDebug($"{model}.{nameof(Procreation)}: Removing Procreation component (if exist)");
                ctx.DestroyComponentIfExists<Procreation>(creatureName, creature);
            }

        }

        //------------------------------------------------
        // CLEANUP
        //------------------------------------------------

        public override void Cleanup(Base.DataProcessorContext ctx)
        {
        }

        //------------------------------------------------
        // UNREGISTER PREFAB
        //------------------------------------------------

        public override void RestorePrefab(Base.DataProcessorContext ctx, string creatureName, Models.Creature data)
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

    }

}

