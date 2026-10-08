using HarmonyLib;
using OfTamingAndBreeding.Network;
using System;
using System.Reflection;

namespace OfTamingAndBreeding.Integrations.Mods
{
    internal static class ValheimPlusCompatibility
    {
        public const string PluginGUID = "org.bepinex.plugins.valheim_plus";

        public static bool IsRegistered { get; private set; }

        private static Type configType;

        public class Registrator : ThirdPartyPluginRegistrator
        {
            public override string PluginGUID => ValheimPlusCompatibility.PluginGUID;

            public override void OnRegistered(string guid, Assembly asm)
            {
                IsRegistered = true;
                configType = asm.GetType("ValheimPlus.Configurations.Configuration");
                NetworkSessionManager.OnSessionValidate += ValidateSession;

                Harmony harmony = new Harmony($"{PluginGUID}.OTAB-compatibility");

                // need to prepatch this one because they are using:
                // var humanoid = __instance.m_grownPrefab.GetComponent<Humanoid>();
                // and the field m_grownPrefab is null
                Patches.PatchUniversalThirdPartyMethod(harmony, asm, "ValheimPlus.GameClasses.Growup_Start_Patch", "Prefix");
            }
        }

        private static bool ValidateSession()
        {
            if (!NetworkSessionManager.IsServer())
            {
                return true;
            }

            object config = configType?.GetProperty("Current", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
            if (config == null)
            {
                Plugin.LogFatal("ValheimPlus configuration could not be loaded.");
                return false;
            }

            foreach (string sectionName in new[] { "Tameable", "Procreation", "Egg" })
            {
                object section = configType.GetProperty(sectionName)?.GetValue(config);
                object enabled = section?.GetType().GetProperty("IsEnabled")?.GetValue(section);
                if (!(enabled is bool isEnabled))
                {
                    Plugin.LogFatal($"ValheimPlus configuration '{sectionName}' could not be validated.");
                    return false;
                }
                if (isEnabled)
                {
                    Plugin.LogFatal($"ValheimPlus configuration '{sectionName}' conflicts with OTAB.");
                    return false;
                }
            }

            return true;
        }
    }
}