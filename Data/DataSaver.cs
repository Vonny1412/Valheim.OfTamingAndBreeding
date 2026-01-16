using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using UnityEngine;
using System.CodeDom;

namespace OfTamingAndBreeding.Data
{
    internal sealed class DataSaver
    {

        private readonly Dictionary<string, Models.Creature> _creatures = new Dictionary<string, Models.Creature>();
        private readonly Dictionary<string, Models.Offspring> _offsprings = new Dictionary<string, Models.Offspring>();
        private readonly Dictionary<string, Models.Egg> _eggs = new Dictionary<string, Models.Egg>();




        public DataSaver()
        {

        }




        public void AddList(Dictionary<string, Models.Creature> data)
        {
            foreach (var kv in data) _creatures[kv.Key] = kv.Value;
        }
        public void AddList(Dictionary<string, Models.Offspring> data)
        {
            foreach (var kv in data) _offsprings[kv.Key] = kv.Value;
        }
        public void AddList(Dictionary<string, Models.Egg> data)
        {
            foreach (var kv in data) _eggs[kv.Key] = kv.Value;
        }






        public void WriteFiles(string toPath)
        {
            WriteYamlFiles(Path.Combine(toPath, Data.Models.Creature.GetDirectoryName()), _creatures);
            WriteYamlFiles(Path.Combine(toPath, Data.Models.Offspring.GetDirectoryName()), _offsprings);
            WriteYamlFiles(Path.Combine(toPath, Data.Models.Egg.GetDirectoryName()), _eggs);
            _creatures.Clear();
            _offsprings.Clear();
            _eggs.Clear();
        }






        internal enum ObjectType
        {
            Parent,
            Baby,
            Item,
        }

        public void AddObject(string prefabName, ObjectType what, bool recursive)
        {
            if (ZNetScene.instance == null)
                return;

            // This prevents infinite loops when traversing
            // Parent -> Procreation -> Offspring -> Growup -> Parent graphs.
            if (IsAlreadyCached(prefabName))
                return;

            var go = ZNetScene.instance.GetPrefab(prefabName);
            if (go == null)
            {
                Plugin.LogWarning($"Prefab '{prefabName}' not found.");
                return;
            }

            switch (what)
            {
                case ObjectType.Parent:
                    SaveAsParent(go, prefabName, recursive);
                    break;

                case ObjectType.Baby:
                    SaveAsBaby(go, prefabName, recursive);
                    break;

                case ObjectType.Item:
                    SaveAsItem(go, prefabName, recursive);
                    break;
            }
        }

        // -----------------------------
        // Core: Save-Routinen
        // -----------------------------

        private void SaveAsParent(GameObject parent, string parentName, bool recursive)
        {
            // Parent-Call soll mindestens "creature" speichern (MonsterAI/Tameable/...)
            _creatures[parentName] = BuildCreatureData(parent);

            if (!recursive)
                return;

            // Optional: Parent -> Procreation -> offspring chain
            var procreation = parent.GetComponent<Procreation>();
            if (procreation == null)
                return;

            var baby = procreation.m_offspring;
            if (baby == null)
            {
                Plugin.LogWarning($"Procreation offspring is null for '{parentName}'.");
                return;
            }

            // Speichere Procreation-Daten (inkl. offsprings[]), soweit möglich
            EnsureCreatureHasProcreationData(parentName, procreation, baby);

            // Traversiere ins Baby (recursive)
            AddObject(baby.name, ObjectType.Baby, recursive: true);
        }

        private void SaveAsBaby(GameObject babyPrefab, string babyName, bool recursive)
        {
            // Baby kann Egg ODER Offspring-Creature sein. Wir entscheiden anhand Komponenten.

            // 1) Egg?
            var eggGrow = babyPrefab.GetComponent<EggGrow>();
            var itemDrop = babyPrefab.GetComponent<ItemDrop>();
            if (eggGrow != null && itemDrop != null)
            //if (itemDrop != null)
            {
                // EggData speichern
                _eggs[babyName] = BuildEggData(babyPrefab, eggGrow, itemDrop);

                // Wenn eggGrow ein grownPrefab hat, dann ist "Baby" dahinter eigentlich das grown creature
                if (eggGrow != null && eggGrow.m_grownPrefab != null)
                {
                    var grownName = eggGrow.m_grownPrefab.name;

                    // grown creature als Offspring speichern (wenn möglich)
                    AddObject(grownName, ObjectType.Baby, recursive);

                    // und: "parent" speichern, weil Offspring Growup typischerweise auf grownPrefabs (Eltern) zeigt
                    if (recursive)
                        SaveParentsFromGrownPrefab(eggGrow.m_grownPrefab);
                }

                return;
            }

            // 2) Offspring creature? (Growup + Character sind ein guter Indikator)
            var growup = babyPrefab.GetComponent<Growup>();
            var character = babyPrefab.GetComponent<Character>();
            if (growup != null && character != null)
            {
                _offsprings[babyName] = BuildOffspringData(babyPrefab, growup, character);

                if (recursive)
                {
                    // Eltern aus Growup ableiten und als Parent speichern
                    SaveParentsFromGrownPrefab(babyPrefab);
                }

                return;
            }

            // 3) Fallback: kein bekanntes Baby-Format
            Plugin.LogWarning($"Unknown baby type of '{babyName}'.");
        }

        private void SaveAsItem(GameObject itemPrefab, string itemName, bool recursive)
        {
            var itemDrop = itemPrefab.GetComponent<ItemDrop>();
            if (itemDrop != null)
            {
                // ItemData speichern

                // todo: currently we got no model for items!

                //_eggs[babyName] = BuildEggData(babyPrefab, eggGrow, itemDrop);
                return;
            }

            Plugin.LogWarning($"Unknown type of '{itemName}' (is not an item).");
        }

        // -----------------------------
        // Builders
        // -----------------------------

        private static Models.Creature BuildCreatureData(GameObject go)
        {
            var data = new Models.Creature();

            var monsterAI = go.GetComponent<MonsterAI>();
            if (monsterAI != null)
            {
                var src = monsterAI.m_consumeItems;
                var consumeItems = new List<Models.Creature.MonsterAIConsumItemData>(src?.Count ?? 0);
                if (src != null)
                {
                    foreach (var it in src)
                    {
                        if (it != null) consumeItems.Add(new Models.Creature.MonsterAIConsumItemData
                        {
                            prefab = it.name,
                            fedDurationMultiply = 1f,
                        });
                    }
                }

                data.MonsterAI = new Models.Creature.MonsterAIData
                {
                    consumeItems = consumeItems.ToArray(),
                    consumeRange = monsterAI.m_consumeRange,
                    consumeSearchRange = monsterAI.m_consumeSearchRange,
                    consumeSearchInterval = monsterAI.m_consumeSearchInterval,
                };
            }

            var tameable = go.GetComponent<Tameable>();
            if (tameable != null)
            {
                data.Tameable = new Models.Creature.TameableData
                {
                    fedDuration = tameable.m_fedDuration,
                    tamingTime = tameable.m_tamingTime,
                    commandable = tameable.m_commandable,
                };
            }

            var character = go.GetComponent<Character>();
            if (character != null)
            {
                data.Character = new Models.Creature.CharacterAIData
                {
                    group = character.m_group,
                    stickToFaction = false,
                    attacksTames = false,
                };
            }

            // Procreation wird in EnsureCreatureHasProcreationData(...) gesetzt,
            // damit wir dort auch den konkreten offspring-Prefab-Name eintragen können.

            return data;
        }

        private static Models.Egg BuildEggData(GameObject eggPrefab, EggGrow eggGrow, ItemDrop itemDrop)
        {
            var shared = itemDrop.m_itemData?.m_shared;
            if (shared == null)
            {
                Plugin.LogWarning($"Egg '{eggPrefab.name}' ItemDrop shared data missing.");
                shared = new ItemDrop.ItemData.SharedData(); // not ideal, aber verhindert NRE
            }

            return new Models.Egg
            {
                Clone = new Models.Egg.CloneData
                {
                    from = null,
                },
                Item = new Models.Egg.ItemData
                {
                    name = shared.m_name,
                    description = shared.m_description,
                    weight = shared.m_weight,
                    scale = 1,
                    scaleByQuality = shared.m_scaleByQuality,
                    scaleWeightByQuality = shared.m_scaleWeightByQuality,
                    value = shared.m_value,
                    teleportable = shared.m_teleportable,
                    maxStackSize = shared.m_maxStackSize,
                    itemTintRgb = new int[] { },
                    lightsTintRgb = new int[] { },
                    particlesTintRgb = new int[] { },
                    lightsScale = 1,
                    disableParticles = false,
                },
                EggGrow = eggGrow == null ? null : new Models.Egg.EggGrowData
                {
                    growTime = eggGrow.m_growTime,
                    updateInterval = eggGrow.m_updateInterval,
                    requireNearbyFire = eggGrow.m_requireNearbyFire,
                    requireUnderRoof = eggGrow.m_requireUnderRoof,
                    requireCoverPercentige = eggGrow.m_requireCoverPercentige,
                    grown = new Models.Egg.EggGrowGrownData[] {
                        new Models.Egg.EggGrowGrownData
                        {
                            prefab = eggGrow.m_grownPrefab.name,
                            tamed = eggGrow.m_tamed,
                            showHatchEffect = true,
                        }
                    },
                },
            };
        }

        private static Models.Offspring BuildOffspringData(GameObject babyPrefab, Growup growup, Character character)
        {

            var grownEntries = new List<Models.Offspring.GrowupGrownData>();
            if (growup.m_grownPrefab != null)
            {
                grownEntries.Add(new Models.Offspring.GrowupGrownData
                {
                    weight = 1,
                    prefab = growup.m_grownPrefab.name,
                });
            }
            if (growup.m_altGrownPrefabs != null)
            {
                foreach (var entry in growup.m_altGrownPrefabs)
                {
                    if (entry?.m_prefab == null) continue;
                    //todo: checked if already in list?
                    grownEntries.Add(new Models.Offspring.GrowupGrownData
                    {
                        weight = entry.m_weight,
                        prefab = entry.m_prefab.name,
                    });
                }
            }
            return new Models.Offspring
            {
                Clone = new Models.Offspring.CloneData
                {
                    from = null,
                },
                Character = new Models.Offspring.CharacterData
                {
                    name = character.m_name,
                    scale = 1,
                    group = character.m_group,
                    stickToFaction = false,
                },
                Growup = new Models.Offspring.GrowupData
                {
                    growTime = growup.m_growTime,
                    inheritTame = growup.m_inheritTame,
                    grown = grownEntries.Count > 0
                        ? grownEntries.ToArray()
                        : Array.Empty<Models.Offspring.GrowupGrownData>(),
                },
            };
        }

        // -----------------------------
        // Parent-Procreation Helper
        // -----------------------------

        private void EnsureCreatureHasProcreationData(string parentName, Procreation procreation, GameObject babyPrefab)
        {
            if (!_creatures.TryGetValue(parentName, out var creature))
            {
                creature = new Models.Creature();
                _creatures[parentName] = creature;
            }

            // Set/Overwrite Procreation
            creature.Procreation = new Models.Creature.ProcreationData
            {
                updateInterval = procreation.m_updateInterval,
                totalCheckRange = procreation.m_totalCheckRange,
                partner = new Models.Creature.ProcreationPartnerData[] {
                    new Models.Creature.ProcreationPartnerData {
                        weight = 1,
                        prefab = parentName,
                    },
                },
                partnerCheckRange = procreation.m_partnerCheckRange,
                partnerRecheckSeconds = 60,
                requiredLovePoints = procreation.m_requiredLovePoints,
                pregnancyChance = procreation.m_pregnancyChance,
                pregnancyDuration = procreation.m_pregnancyDuration,
                spawnOffset = procreation.m_spawnOffset,
                spawnOffsetMax = procreation.m_spawnOffsetMax,
                spawnRandomDirection = procreation.m_spawnRandomDirection,

                procreateWhileSwimming = true,

                extraOffspringChance = 0,
                maxOffspringsPerPregnancy = 1,

                offspring = new[]
                {
                    new Models.Creature.ProcreationOffspringData
                    {
                        weight = 1,
                        prefab = babyPrefab != null ? babyPrefab.name : null,
                        needPartner = true,
                        needPartnerPrefab = null,
                        //needFoodPrefab = null,
                        maxCreatures = procreation.m_maxCreatures,
                        levelUpChance = 0,
                        maxLevel = 3,
                    }
                }
            };
        }

        // -----------------------------
        // Graph Traversal Helpers
        // -----------------------------

        private void SaveParentsFromGrownPrefab(GameObject grownCreaturePrefab)
        {
            var growup = grownCreaturePrefab.GetComponent<Growup>();
            if (growup == null)
                return;

            if (growup.m_grownPrefab != null)
                AddObject(growup.m_grownPrefab.name, ObjectType.Parent, recursive: false);

            if (growup.m_altGrownPrefabs != null)
            {
                foreach (var entry in growup.m_altGrownPrefabs)
                {
                    if (entry?.m_prefab == null) continue;
                    AddObject(entry.m_prefab.name, ObjectType.Parent, recursive: false);
                }
            }
        }

        private bool IsAlreadyCached(string prefabName)
        {
            return _creatures.ContainsKey(prefabName)
                || _offsprings.ContainsKey(prefabName)
                || _eggs.ContainsKey(prefabName);
        }

        private static void WriteYamlFiles<T>(string folderPath, Dictionary<string, T> items)
            where T : DataBase<T>
        {
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
            foreach (var kvp in items)
            {
                var yaml = DataBase.Serialize(kvp.Value);
                File.WriteAllText(Path.Combine(folderPath, $"{kvp.Key}.yml"), yaml);
            }
        }
    }
}
