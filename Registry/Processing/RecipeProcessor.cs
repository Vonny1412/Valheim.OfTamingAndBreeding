using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;
using OfTamingAndBreeding.ThirdParty.Mods;
using UnityEngine;
using OfTamingAndBreeding.Data.Models;
using OfTamingAndBreeding.Data.Models.SubData;

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

        public override void Prepare(Base.PrefabRegistry reg)
        {
            foreach (var recipe in ObjectDB.instance.m_recipes)
            {
                originalRecipes.Add(recipe);
            }
        }

        public override bool ValidateData(Base.PrefabRegistry reg, string recipeName, RecipeData data)
        {
            return WackyDBBridge.IsRegistered;
        }

        public override bool ReservePrefab(Base.PrefabRegistry reg, string recipeName, RecipeData data)
        {
            return true; // i dont care
        }

        public override bool ValidatePrefab(Base.PrefabRegistry reg, string recipeName, RecipeData data)
        {
            // TODO!
            return true;
        }

        public override void RegisterPrefab(Base.PrefabRegistry reg, string recipeName, RecipeData data)
        {
            WackyDBBridge.ApplyRecipe(data);
        }

        public override void Finalize(Base.PrefabRegistry reg)
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

        public override void RestorePrefab(Base.PrefabRegistry reg, string recipeName)
        {
            foreach (var recipe in otabRecipes)
            {
                ObjectDB.instance.m_recipes.Remove(recipe);
            }
            otabRecipes.Clear();
        }

        public override void Cleanup(Base.PrefabRegistry reg)
        {
        }

    }
}
