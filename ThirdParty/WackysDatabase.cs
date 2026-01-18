using BepInEx.Bootstrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.ThirdParty
{
    internal static class ThirdPartyManager
    {
        private static readonly Dictionary<string, Action<string, Assembly>> callbacks = new Dictionary<string, Action<string, Assembly>>();

        public static void RegisterPlugin(string GUID, Action<string, Assembly> cb)
        {
            callbacks.Add(GUID, cb);
        }

        public static void TryGetAssemblies()
        {
            foreach(var kv in callbacks)
            {
                string guid = kv.Key;
                Action<string, Assembly> cb = kv.Value;
                if (TryGetPluginAssembly(guid, out Assembly asm))
                {
                    cb(guid, asm);
                }
            }
        }

        public static bool TryGetPluginAssembly(string GUID, out Assembly asm)
        {
            asm = null;
            if (Chainloader.PluginInfos.TryGetValue(GUID, out var info))
            {
                asm = info.Instance.GetType().Assembly;
                return asm != null;
            }
            return false;
        }

    }














    internal static class WackysDatabaseBridge
    {
        public const string PluginGuid = "WackyMole.WackysDatabase";

        private static bool registered = false;

        private static Type recipeDataType = null;
        private static MethodInfo setRecipeData = null;

        public static void Register()
        {
            ThirdPartyManager.RegisterPlugin(PluginGuid, OnRegistered);
        }

        private static void OnRegistered(string guid, Assembly asm)
        {
            var _recipeDataType = asm.GetType("wackydatabase.Datas.RecipeData", false);
            if (_recipeDataType == null) return;

            var _setDataType = asm.GetType("wackydatabase.SetData.SetData", false);
            if (_setDataType == null) return;

            recipeDataType = _recipeDataType;
            setRecipeData = _setDataType.GetMethod("SetRecipeData", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            registered = true;
        }












        // currently unused
        public static void CopyToWacky(object wackyRecipeObj, Data.Models.Recipe src)
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


        public static void ApplyRecipe(Data.Models.Recipe src)
        {
            if (!registered)
            {
                Plugin.LogFatal($"Cannot register recipe - WackyDB not registered");
                return;
            }
            var wackyRecipe = Activator.CreateInstance(recipeDataType);
            CopyToWacky(wackyRecipe, src);
            setRecipeData.Invoke(null, new object[] { wackyRecipe, ObjectDB.instance });
        }

    }







}
