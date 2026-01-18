using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Data.Handling
{
    internal class RecipeModelHandler : ModelHandler<Models.Recipe>
    {
        public override string DirectoryName => Models.Recipe.DirectoryName;

        public override bool ValidateData(ModelHandlerContext ctx, string offspringName, Models.Recipe data)
        {
            return true; // i dont care
        }

        public override bool PreparePrefab(ModelHandlerContext ctx, string offspringName, Models.Recipe data)
        {
            return true; // i dont care
        }

        public override bool ValidatePrefab(ModelHandlerContext ctx, string offspringName, Models.Recipe data)
        {
            return true; // i dont care
        }

        public override void RegisterPrefab(ModelHandlerContext ctx, string offspringName, Models.Recipe data)
        {
            ThirdParty.WackysDatabaseBridge.ApplyRecipe(data);
        }

    }
}
