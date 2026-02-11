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

        public override string GetDataKey(string filePath) => null;

        public override void Prepare(Base.DataProcessorContext ctx)
        {

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

        public override void Cleanup(Base.DataProcessorContext ctx)
        {

        }

        public override void RestorePrefab(Base.DataProcessorContext ctx, string recipeName, Models.Recipe data)
        {
            // TODO: do i need to unregister the recipe? how to do that?
        }

    }
}
