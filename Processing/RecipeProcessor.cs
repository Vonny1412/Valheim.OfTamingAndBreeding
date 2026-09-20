using Jotunn.Managers;
using OfTamingAndBreeding.Data.Models;
using OfTamingAndBreeding.Processing.Core;
using System.Collections.Generic;
using UnityEngine;

namespace OfTamingAndBreeding.Processing
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
            var valid = true;

            if (string.IsNullOrEmpty(data.Item))
            {
                Plugin.LogError($"{model}.{nameof(data.Item)}: Missing field");
                valid = false;
            }

            if (data.Amount <= 0)
            {
                Plugin.LogWarning($"{model}.{nameof(data.Amount)}: Value must be > 0 - Setting to 1");
                data.Amount = 1;
            }

            if (string.IsNullOrEmpty(data.CraftingStation))
            {
                Plugin.LogError($"{model}.{nameof(data.CraftingStation)}: Missing field");
                valid = false;
            }

            if (data.MinStationLevel <= 0)
            {
                Plugin.LogWarning($"{model}.{nameof(data.MinStationLevel)}: Value must be > 0 - Setting to 1");
                data.MinStationLevel = 1;
            }

            if (data.Requirements == null || data.Requirements.Length == 0)
            {
                Plugin.LogError($"{model}.{nameof(data.Requirements)}: List is null or empty");
                valid = false;
            }
            else
            {
                for (int i = 0; i < data.Requirements.Length; ++i)
                {
                    var requirement = data.Requirements[i];

                    if (requirement == null)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Requirements)}.{i}: Entry is null");
                        valid = false;
                        continue;
                    }

                    if (string.IsNullOrEmpty(requirement.Prefab))
                    {
                        Plugin.LogError($"{model}.{nameof(data.Requirements)}.{i}.{nameof(requirement.Prefab)}: Missing field");
                        valid = false;
                    }

                    if (requirement.Amount <= 0)
                    {
                        Plugin.LogWarning($"{model}.{nameof(data.Requirements)}.{i}.{nameof(requirement.Amount)}: Value must be > 0 - Setting to 1");
                        requirement.Amount = 1;
                    }

                    if (requirement.AmountPerLevel < 0)
                    {
                        Plugin.LogWarning($"{model}.{nameof(data.Requirements)}.{i}.{nameof(requirement.AmountPerLevel)}: Negative value not allowed - Setting to 0");
                        requirement.AmountPerLevel = 0;
                    }
                }
            }

            return valid;
        }

        public override bool ReservePrefab(string recipeName, RecipeFile data)
        {
            return true;
        }

        public override bool ValidatePrefab(string recipeName, RecipeFile data)
        {
            var model = $"{nameof(RecipeFile)}.{recipeName}";
            var valid = true;

            //
            // output item
            //

            var itemPrefab = PrefabManager.Instance.GetPrefab(data.Item);
            if (!itemPrefab)
            {
                Plugin.LogError($"{model}.{nameof(data.Item)}: Prefab '{data.Item}' not found");
                valid = false;
            }
            else if (!itemPrefab.GetComponent<ItemDrop>())
            {
                Plugin.LogError($"{model}.{nameof(data.Item)}: Prefab '{data.Item}' has no ItemDrop");
                valid = false;
            }

            //
            // crafting station
            //

            var stationPrefab = PrefabManager.Instance.GetPrefab(data.CraftingStation);
            if (!stationPrefab)
            {
                Plugin.LogError($"{model}.{nameof(data.CraftingStation)}: Prefab '{data.CraftingStation}' not found");
                valid = false;
            }
            else if (!stationPrefab.GetComponent<CraftingStation>())
            {
                Plugin.LogError($"{model}.{nameof(data.CraftingStation)}: Prefab '{data.CraftingStation}' has no CraftingStation");
                valid = false;
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
                        Plugin.LogError($"{model}.{nameof(data.Requirements)}.{i}.{nameof(requirement.Prefab)}: Prefab '{requirement.Prefab}' not found");
                        valid = false;
                        continue;
                    }

                    if (!prefab.GetComponent<ItemDrop>())
                    {
                        Plugin.LogError($"{model}.{nameof(data.Requirements)}.{i}.{nameof(requirement.Prefab)}: Prefab '{requirement.Prefab}' has no ItemDrop");
                        valid = false;
                    }
                }
            }

            return valid;
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

        public override bool ProcessPrefab(string recipeName, RecipeFile data)
        {
            return true;
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
