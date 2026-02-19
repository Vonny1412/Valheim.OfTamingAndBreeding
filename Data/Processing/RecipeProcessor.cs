using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OfTamingAndBreeding.ThirdParty.Mods;
namespace OfTamingAndBreeding.Data.Processing
{
    internal class RecipeProcessor : Base.DataProcessor<Models.Recipe>
    {
        public override string DirectoryName => Models.Recipe.DirectoryName;

        private readonly HashSet<Recipe> originalRecipes = new HashSet<Recipe>();
        private readonly HashSet<Recipe> otabRecipes = new HashSet<Recipe>();

        public override string GetDataKey(string filePath) => null;

        public override void Prepare(Base.DataProcessorContext ctx)
        {
            foreach (var recipe in ObjectDB.instance.m_recipes)
            {
                originalRecipes.Add(recipe);
            }
        }

        public override bool ValidateData(Base.DataProcessorContext ctx, string recipeName, Models.Recipe data)
        {
            return WackyDBBridge.IsRegistered;
        }

        public override bool PreparePrefab(Base.DataProcessorContext ctx, string recipeName, Models.Recipe data)
        {
            return true; // i dont care
        }

        public override bool ValidatePrefab(Base.DataProcessorContext ctx, string recipeName, Models.Recipe data)
        {
            return true; // i dont care
        }

        public override void RegisterPrefab(Base.DataProcessorContext ctx, string recipeName, Models.Recipe data)
        {
            WackyDBBridge.ApplyRecipe(data);
        }

        public override void Finalize(Base.DataProcessorContext ctx)
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

        public override void RestorePrefab(Base.DataProcessorContext ctx, string recipeName, Models.Recipe data)
        {
            foreach (var recipe in otabRecipes)
            {
                ObjectDB.instance.m_recipes.Remove(recipe);
            }
            otabRecipes.Clear();
        }

        public override void Cleanup(Base.DataProcessorContext ctx)
        {
        }

    }
}
