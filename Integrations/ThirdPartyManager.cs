using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using OfTamingAndBreeding.Integrations.Mods;
using OfTamingAndBreeding.Processing.Core;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace OfTamingAndBreeding.Integrations
{

    internal static class Patches
    {
        public static void PatchUniversalThirdPartyMethod(Harmony harmony, Assembly assembly, string typeName, string methodName)
        {
            Type type = assembly.GetType(typeName);
            if (type == null)
            {
                return;
            }
            MethodInfo method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            PatchUniversalThirdPartyMethod(harmony, method);
        }


        public static void PatchUniversalThirdPartyMethod(Harmony harmony, MethodInfo method)
        {
            if (method == null)
            {
                return;
            }
            harmony.Patch(method, prefix: new HarmonyMethod(typeof(Patches), nameof(OTAB_ThirdParty_Prefix)));
            Plugin.LogInfo($"Patched third-party method: {method.DeclaringType?.FullName}.{method.Name}");
        }

        [HarmonyPrefix]
        public static bool OTAB_ThirdParty_Prefix()
        {
            return !DataProcessingManager.IsDataLoaded;
        }

    }

    internal interface IThirdPartyPluginRegistrator
    {
        string PluginGUID { get; }
        void OnRegistered(string guid, Assembly asm);
    }

    internal abstract class ThirdPartyPluginRegistrator : IThirdPartyPluginRegistrator
    {
        public abstract string PluginGUID { get; }

        public abstract void OnRegistered(string guid, Assembly asm);

    }

    internal static class ThirdPartyManager
    {

        public static bool TryGetPluginMetadata(string GUID, out BepInPlugin meta)
        {
            if (Chainloader.PluginInfos.TryGetValue(GUID, out var info))
            {
                meta = info.Metadata;
                return true;
            }
            meta = null;
            return false;
        }

        private static bool TryGetPluginAssembly(string GUID, out Assembly asm)
        {
            if (Chainloader.PluginInfos.TryGetValue(GUID, out var info))
            {
                asm = info.Instance.GetType().Assembly;
                return asm != null;
            }
            asm = null;
            return false;
        }

        public static void RegisterBridges()
        {
            var regs = new List<IThirdPartyPluginRegistrator>()
            {
                new CllCBridge.Registrator(),
                new ValheimPlusCompatibility.Registrator(),
                new CoreWoodExtrasCompatibility.Registrator(),
            };
            foreach (IThirdPartyPluginRegistrator reg in regs)
            {
                string guid = reg.PluginGUID;
                Action<string, Assembly> cb = reg.OnRegistered;
                if (TryGetPluginAssembly(guid, out Assembly asm))
                {
                    cb(guid, asm);
                }
            }
        }

    }
}
