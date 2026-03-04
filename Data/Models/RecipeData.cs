using System;
using System.Collections.Generic;

namespace OfTamingAndBreeding.Data.Models
{

    [Serializable]
    internal class RecipeData : DataBase<RecipeData>
    {

        public const string DirectoryName = "Recipes";

        // need to be same structure as original
        // https://github.com/Wacky-Mole/WackysDatabase/blob/master/Datas/RecipeData.cs

        public string name;
        public string clonePrefabName;
        public string craftingStation;
        public int? minStationLevel;
        public int? maxStationLevelCap;
        public string repairStation;
        public int? amount;
        public bool? disabled;
        public bool? disabledUpgrade;
        public bool? requireOnlyOneIngredient;
        public List<string> upgrade_reqs;
        public List<string> reqs;

    }
}
