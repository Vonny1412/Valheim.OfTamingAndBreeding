using Jotunn.Managers;
using OfTamingAndBreeding.Data.Handling.Base;
using OfTamingAndBreeding.ThirdParty.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Data.Handling
{
    internal class RecipeHandler : DataHandler<Models.Recipe>
    {
        public override string DirectoryName => Models.Recipe.DirectoryName;

        public override string GetDataKey(string filePath) => null;

        public override void Prepare(DataHandlerContext ctx)
        {

        }

        public override bool ValidateData(DataHandlerContext ctx, string recipeName, Models.Recipe data)
        {
            return WackyDBBridge.IsRegistered;
        }

        public override bool PreparePrefab(DataHandlerContext ctx, string recipeName, Models.Recipe data)
        {
            return true; // i dont care
        }

        public override bool ValidatePrefab(DataHandlerContext ctx, string recipeName, Models.Recipe data)
        {
            return true; // i dont care
        }

        public override void RegisterPrefab(DataHandlerContext ctx, string recipeName, Models.Recipe data)
        {
            WackyDBBridge.ApplyRecipe(data);
        }

        public override void Cleanup(DataHandlerContext ctx)
        {

        }

        public override void RestorePrefab(DataHandlerContext ctx, string recipeName, Models.Recipe data)
        {
            // TODO: do i need to unregister the recipe? how to do that?
        }

    }
}
