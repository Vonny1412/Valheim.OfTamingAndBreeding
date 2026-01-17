
using BepInEx.Bootstrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.ThirdParty
{

    internal static class ThirdPartyUtil
    {
        public static bool TryGetPluginAssembly(string pluginGuid, out Assembly asm)
        {
            asm = null;
            if (Chainloader.PluginInfos.TryGetValue(pluginGuid, out var info))
            {
                asm = info.Instance.GetType().Assembly;
                return asm != null;
            }
            return false;
        }
    }

    internal static class ThirdPartyIntegration
    {
        public static bool HasWackysDatabase { get; private set; }

        public static void Init()
        {
            HasWackysDatabase =
                BepInEx.Bootstrap.Chainloader.PluginInfos
                    .ContainsKey("WackyMole.WackysDatabase");
        }
    }


    /*
     * 
     * 
     *  warning: this feature is not implemented yet
     *  i still dont know if i gonna implement it at all
     * 
     * 
     * 
     * 
     * */




    /*
namespace wackydatabase.Datas
{
[Serializable]
[CanBeNull]
public class RecipeData
{
public string? name; //must have
public string? clonePrefabName;
//public string cloneColor;
public string? craftingStation;
public int? minStationLevel;
public int? maxStationLevelCap;
public string? repairStation;
public int? amount;
public bool? disabled;
public bool? disabledUpgrade;
public bool? requireOnlyOneIngredient;
public List<string>? upgrade_reqs = new List<string>(); // Only for upgrades
public List<string>? reqs = new List<string>(); // must have // First time and upgrades if upgrade_reqs is not set


}


}
    */


    internal static class WackysDatabaseBridge
    {
        private const string PluginGuid = "WackyMole.WackysDatabase";
        private static Assembly _asm;
        private static Type _recipeDataType;
        private static MethodInfo _setRecipeData;

        internal sealed class OtabsRecipeData
        {
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

        public static void CopyToWacky(object wackyRecipeObj, OtabsRecipeData src)
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







        public static bool TryInit()
        {
            if (_setRecipeData != null) return true;

            if (!Chainloader.PluginInfos.TryGetValue(PluginGuid, out var info))
                return false;

            _asm = info.Instance.GetType().Assembly;

            _recipeDataType = _asm.GetType("wackydatabase.Datas.RecipeData", false);
            if (_recipeDataType == null) return false;

            var setDataType = _asm.GetType("wackydatabase.SetData.SetData", false);
            if (setDataType == null) return false;

            //internal static void SetRecipeData(RecipeData data, ObjectDB Instant)
            _setRecipeData = setDataType.GetMethod(
                    "SetRecipeData",
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public
                );

            return _setRecipeData != null;
        }

        public static object CreateRecipeData() => Activator.CreateInstance(_recipeDataType);

        public static void ApplyRecipe(object recipeData, object objectDb)
        {
            // static method => target null
            _setRecipeData.Invoke(null, new[] { recipeData, objectDb });
        }
    }







}
