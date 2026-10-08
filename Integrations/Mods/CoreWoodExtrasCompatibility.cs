using HarmonyLib;
using OfTamingAndBreeding.Runtime;
using System;
using System.Reflection;

namespace OfTamingAndBreeding.Integrations.Mods
{
    internal static partial class CoreWoodExtrasCompatibility
    {

        public const string PluginGUID = "MagicMike.CoreWoodExtras";

        public static bool IsRegistered { get; private set; }

        private static Assembly assembly;
        private static Harmony harmony;

        public class Registrator : ThirdPartyPluginRegistrator
        {
            public override string PluginGUID => CoreWoodExtrasCompatibility.PluginGUID;

            public override void OnRegistered(string guid, Assembly asm)
            {
                assembly = asm;
                IsRegistered = true;

                harmony = new Harmony($"{PluginGUID}.OTAB-compatibility");
                PatchConflictingMethods();

                ContainerFeeding.RegisterCustomContainerPrefab("mm_corewood_trough");
            }
        }

        private static void PatchConflictingMethods()
        {
            Type patchType = Array.Find(assembly.GetTypes(), type => type.Name == "Tameable_IsHungry_Patch");
            if (patchType == null)
            {
                Plugin.LogWarning("CoreWoodExtras Tameable_IsHungry_Patch not found.");
                return;
            }

            MethodInfo method = AccessTools.Method(patchType, "Postfix");
            if (method == null)
            {
                Plugin.LogWarning("CoreWoodExtras Tameable_IsHungry_Patch.Postfix not found.");
                return;
            }

            Patches.PatchUniversalThirdPartyMethod(harmony, method);
        }

    }
}
