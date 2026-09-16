using Jotunn.Managers;
using OfTamingAndBreeding.Data.Models;
using OfTamingAndBreeding.Processing.Core;
using OfTamingAndBreeding.Integrations.Mods;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OfTamingAndBreeding.Registry.Processing
{
    internal class RecipeProcessor : DataProcessor<RecipeFile>
    {
        public override string DirectoryName => RecipeFile.DirectoryName;

        public override string PrefabTypeName => null;

        public override string GetDataKey(string filePath) => null;

        public override bool LoadFromFile(string filePath) => LoadFromYamlFile(filePath);

        //
        //
        //

        private readonly Dictionary<string, Recipe> otabRecipes = new Dictionary<string, Recipe>();

        public override void PrepareProcess()
        {
        }

        public override bool ValidateData(string recipeName, RecipeFile data)
        {
            var model = $"{nameof(RecipeFile)}.{recipeName}";
            var error = false;

            if (string.IsNullOrEmpty(data.Item))
            {
                Plugin.LogError($"{model}.{nameof(data.Item)}: Missing field");
                error = true;
            }

            if (data.Amount <= 0)
            {
                Plugin.LogWarning(
                    $"{model}.{nameof(data.Amount)}: Value must be > 0 - Setting to 1"
                );
                data.Amount = 1;
            }

            if (string.IsNullOrEmpty(data.CraftingStation))
            {
                Plugin.LogError($"{model}.{nameof(data.CraftingStation)}: Missing field");
                error = true;
            }

            if (data.MinStationLevel <= 0)
            {
                Plugin.LogWarning(
                    $"{model}.{nameof(data.MinStationLevel)}: Value must be > 0 - Setting to 1"
                );
                data.MinStationLevel = 1;
            }

            if (data.Requirements == null || data.Requirements.Length == 0)
            {
                Plugin.LogError($"{model}.{nameof(data.Requirements)}: List is null or empty");
                error = true;
            }
            else
            {
                for (int i = 0; i < data.Requirements.Length; ++i)
                {
                    var requirement = data.Requirements[i];

                    if (requirement == null)
                    {
                        Plugin.LogError(
                            $"{model}.{nameof(data.Requirements)}.{i}: Entry is null"
                        );
                        error = true;
                        continue;
                    }

                    if (string.IsNullOrEmpty(requirement.Prefab))
                    {
                        Plugin.LogError(
                            $"{model}.{nameof(data.Requirements)}.{i}.{nameof(requirement.Prefab)}: Missing field"
                        );
                        error = true;
                    }

                    if (requirement.Amount <= 0)
                    {
                        Plugin.LogWarning(
                            $"{model}.{nameof(data.Requirements)}.{i}.{nameof(requirement.Amount)}: " +
                            $"Value must be > 0 - Setting to 1"
                        );
                        requirement.Amount = 1;
                    }

                    if (requirement.AmountPerLevel < 0)
                    {
                        Plugin.LogWarning(
                            $"{model}.{nameof(data.Requirements)}.{i}.{nameof(requirement.AmountPerLevel)}: " +
                            $"Negative value not allowed - Setting to 0"
                        );
                        requirement.AmountPerLevel = 0;
                    }
                }
            }

            return error == false;
        }

        public override bool ReservePrefab(string recipeName, RecipeFile data)
        {
            return true;
        }

        public override bool ValidatePrefab(string recipeName, RecipeFile data)
        {
            var model = $"{nameof(RecipeFile)}.{recipeName}";
            var error = false;

            //
            // output item
            //

            var itemPrefab = PrefabManager.Instance.GetPrefab(data.Item);
            if (!itemPrefab)
            {
                Plugin.LogError(
                    $"{model}.{nameof(data.Item)}: Prefab '{data.Item}' not found"
                );
                error = true;
            }
            else if (!itemPrefab.GetComponent<ItemDrop>())
            {
                Plugin.LogError(
                    $"{model}.{nameof(data.Item)}: Prefab '{data.Item}' has no ItemDrop"
                );
                error = true;
            }

            //
            // crafting station
            //

            var stationPrefab = PrefabManager.Instance.GetPrefab(data.CraftingStation);
            if (!stationPrefab)
            {
                Plugin.LogError(
                    $"{model}.{nameof(data.CraftingStation)}: Prefab '{data.CraftingStation}' not found"
                );
                error = true;
            }
            else if (!stationPrefab.GetComponent<CraftingStation>())
            {
                Plugin.LogError(
                    $"{model}.{nameof(data.CraftingStation)}: " +
                    $"Prefab '{data.CraftingStation}' has no CraftingStation"
                );
                error = true;
            }

            //
            // requirements
            //

            if (data.Requirements != null)
            {
                for (int i = 0; i < data.Requirements.Length; ++i)
                {
                    var requirement = data.Requirements[i];
                    if (requirement == null)
                        continue;

                    var prefab = PrefabManager.Instance.GetPrefab(requirement.Prefab);
                    if (!prefab)
                    {
                        Plugin.LogError(
                            $"{model}.{nameof(data.Requirements)}.{i}.{nameof(requirement.Prefab)}: " +
                            $"Prefab '{requirement.Prefab}' not found"
                        );
                        error = true;
                        continue;
                    }

                    if (!prefab.GetComponent<ItemDrop>())
                    {
                        Plugin.LogError(
                            $"{model}.{nameof(data.Requirements)}.{i}.{nameof(requirement.Prefab)}: " +
                            $"Prefab '{requirement.Prefab}' has no ItemDrop"
                        );
                        error = true;
                    }
                }
            }

            return error == false;
        }

        public override void RegisterPrefab(string recipeName, RecipeFile data)
        {
            var itemPrefab = PrefabManager.Instance.GetPrefab(data.Item);
            var itemDrop = itemPrefab.GetComponent<ItemDrop>();

            var stationPrefab = PrefabManager.Instance.GetPrefab(data.CraftingStation);
            var craftingStation = stationPrefab.GetComponent<CraftingStation>();

            var requirements = new Piece.Requirement[data.Requirements.Length];

            for (int i = 0; i < data.Requirements.Length; ++i)
            {
                var requirementData = data.Requirements[i];

                var requirementPrefab =
                    PrefabManager.Instance.GetPrefab(requirementData.Prefab);

                requirements[i] = new Piece.Requirement
                {
                    m_resItem = requirementPrefab.GetComponent<ItemDrop>(),
                    m_amount = requirementData.Amount,
                    m_amountPerLevel = requirementData.AmountPerLevel,
                    m_recover = false,
                };
            }

            var recipe = ScriptableObject.CreateInstance<Recipe>();

            recipe.name = recipeName;
            recipe.m_item = itemDrop;
            recipe.m_amount = data.Amount;
            recipe.m_enabled = true;
            recipe.m_craftingStation = craftingStation;
            recipe.m_minStationLevel = data.MinStationLevel;
            recipe.m_resources = requirements;

            ObjectDB.instance.m_recipes.Add(recipe);

            otabRecipes[recipeName] = recipe;
        }

        public override void EditPrefab(string recipeName, RecipeFile data)
        {
        }

        public override void FinalizeProcess()
        {
        }

        public override void RestorePrefab(string recipeName)
        {
        }

        public override void CleanupProcess()
        {
            foreach (var recipe in otabRecipes.Values)
            {
                if (!recipe)
                {
                    continue;
                }
                ObjectDB.instance.m_recipes.Remove(recipe);
                UnityEngine.Object.Destroy(recipe);
            }
            otabRecipes.Clear();
        }

    }
}
