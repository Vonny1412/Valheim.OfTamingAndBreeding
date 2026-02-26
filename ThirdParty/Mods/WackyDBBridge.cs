using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OfTamingAndBreeding.ThirdParty.Mods
{

    internal static class WackyDBBridge
    {
        public const string PluginGUID = "WackyMole.WackysDatabase";

        public static bool IsRegistered { get; private set; } = false;

        private static Type recipeDataType = null;
        private static MethodInfo setRecipeData = null;

        public class Registrator : ThirdPartyPluginRegistrator
        {
            public override string PluginGUID => WackyDBBridge.PluginGUID;
            public override void OnRegistered(string guid, Assembly asm)
            {
                var _recipeDataType = asm.GetType("wackydatabase.Datas.RecipeData", false);
                if (_recipeDataType == null) return;

                var _setDataType = asm.GetType("wackydatabase.SetData.SetData", false);
                if (_setDataType == null) return;

                recipeDataType = _recipeDataType;
                setRecipeData = _setDataType.GetMethod("SetRecipeData", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                IsRegistered = true;
            }
        }
        
        public static void CopyToWacky(object wackyRecipeObj, Data.Models.RecipeData src)
        {
            var t = wackyRecipeObj.GetType();

            SetField(t, wackyRecipeObj, "name", src.name);
            SetField(t, wackyRecipeObj, "clonePrefabName", src.clonePrefabName);
            SetField(t, wackyRecipeObj, "craftingStation", src.craftingStation);
            SetField(t, wackyRecipeObj, "minStationLevel", src.minStationLevel);
            SetField(t, wackyRecipeObj, "maxStationLevelCap", src.maxStationLevelCap);
            SetField(t, wackyRecipeObj, "repairStation", src.repairStation);
            SetField(t, wackyRecipeObj, "amount", src.amount);
            SetField(t, wackyRecipeObj, "disabled", src.disabled);
            SetField(t, wackyRecipeObj, "disabledUpgrade", src.disabledUpgrade);
            SetField(t, wackyRecipeObj, "requireOnlyOneIngredient", src.requireOnlyOneIngredient);
            SetField(t, wackyRecipeObj, "upgrade_reqs", src.upgrade_reqs);
            SetField(t, wackyRecipeObj, "reqs", src.reqs);
        }

        private static void SetField(Type t, object obj, string fieldName, object value)
        {
            if (value == null) return;
            var f = t.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public);
            if (f == null) return;
            f.SetValue(obj, value);
        }

        public static void ApplyRecipe(Data.Models.RecipeData src)
        {
            if (!IsRegistered)
            {
                Plugin.LogError($"Cannot register recipe - WackyDB not registered");
                return;
            }
            var wackyRecipe = Activator.CreateInstance(recipeDataType);
            CopyToWacky(wackyRecipe, src);
            setRecipeData.Invoke(null, new object[] { wackyRecipe, ObjectDB.instance });
        }

        /*

        internal static void SetRecipeData(RecipeData data, ObjectDB Instant)
        {
            ...
        }

        ...
        ObjectDB Instant = ObjectDB.instance;
        ...

        foreach (var data in WMRecipeCust.recipeDatasYml)
        {
            try
            {
                SetData.SetRecipeData(data, Instant);
            }
            ...
        }


        */

    }

}
