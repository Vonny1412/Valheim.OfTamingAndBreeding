using System;
using System.Collections.Generic;
using OfTamingAndBreeding.ThirdParty.Mods;
using OfTamingAndBreeding.Data.Models;

namespace OfTamingAndBreeding.Registry.Processing
{
    internal class RecipeProcessor : Base.DataProcessor<RecipeData>
    {
        public override string DirectoryName => RecipeData.DirectoryName;

        public override string PrefabTypeName => null;

        public override string GetDataKey(string filePath) => null;

        public override bool LoadFromFile(string filePath) => LoadFromYamlFile(filePath);

        //
        //
        //

        private readonly HashSet<Recipe> originalRecipes = new HashSet<Recipe>();
        private readonly HashSet<Recipe> otabRecipes = new HashSet<Recipe>();

        public override void PrepareProcess()
        {
            foreach (var recipe in ObjectDB.instance.m_recipes)
            {
                originalRecipes.Add(recipe);
            }
        }

        public override bool ValidateData(string recipeName, RecipeData data)
        {
            return WackyDBBridge.IsRegistered;
        }

        public override bool ReservePrefab(string recipeName, RecipeData data)
        {
            return true;
        }

        public override bool ValidatePrefab(string recipeName, RecipeData data)
        {
            // TODO?
            return true;
        }

        public override void RegisterPrefab(string recipeName, RecipeData data)
        {
            WackyDBBridge.ApplyRecipe(data);
        }

        public override void EditPrefab(string recipeName, RecipeData data)
        {
        }

        public override void FinalizeProcess()
        {
            foreach (var recipe in ObjectDB.instance.m_recipes)
            {
                if (!originalRecipes.Contains(recipe))
                {
                    otabRecipes.Add(recipe);
                }
            }
            originalRecipes.Clear();
        }

        public override void RestorePrefab(string recipeName)
        {
        }

        public override void CleanupProcess()
        {
            foreach (var recipe in otabRecipes)
            {
                ObjectDB.instance.m_recipes.Remove(recipe);
            }
            otabRecipes.Clear();
        }

    }
}
