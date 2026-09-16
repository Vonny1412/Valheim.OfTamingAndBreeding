using JetBrains.Annotations;
using System;

namespace OfTamingAndBreeding.Data.Models
{

    [Serializable]
    internal class RecipeFile : DataBase<RecipeFile>
    {

        public const string DirectoryName = "Recipes";

        [Serializable]
        [CanBeNull]
        public class RecipeRequirementData
        {
            public string Prefab { get; set; } = null;
            public int Amount { get; set; } = 1;
            public int AmountPerLevel { get; set; } = 0;
        }

        public string Item { get; set; } = null;
        public int Amount { get; set; } = 1;
        public string CraftingStation { get; set; } = null;
        public int MinStationLevel { get; set; } = 1;
        public RecipeRequirementData[] Requirements { get; set; } = null;

    }
}
