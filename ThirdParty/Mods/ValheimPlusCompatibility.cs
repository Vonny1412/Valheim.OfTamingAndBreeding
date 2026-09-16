using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;

namespace OfTamingAndBreeding.ThirdParty.Mods
{
    internal static class ValheimPlusCompatibility
    {
        public const string PluginGUID = "org.bepinex.plugins.valheim_plus";

        public static bool IsRegistered { get; private set; }

        private static Assembly assembly;
        private static Harmony harmony;

        public class Registrator : ThirdPartyPluginRegistrator
        {
            public override string PluginGUID => ValheimPlusCompatibility.PluginGUID;

            public override void OnRegistered(string guid, Assembly asm)
            {
                assembly = asm;
                IsRegistered = true;

                harmony = new Harmony($"{PluginGUID}.OTAB-compatibility");
                harmony.PatchAll(typeof(ValheimPlusPatchAllPatch));
                DisableConflictingPatches();
            }
        }

        [HarmonyPatch]
        private static class ValheimPlusPatchAllPatch
        {
            private static MethodBase TargetMethod()
            {
                if (!IsRegistered)
                    return null;
                Type type = assembly.GetType("ValheimPlus.ValheimPlusPlugin");
                return AccessTools.Method(type, "PatchAll");
            }

            [HarmonyPostfix]
            private static void Postfix()
            {
                DisableConflictingPatches();
            }
        }

        private static void DisableConflictingPatches()
        {
            Plugin.LogInfo("Valheim+ detected. Disabling conflicting patches.");

            Unpatch("ValheimPlus.GameClasses.Tameable_GetHoverText_Patch");
            Unpatch("ValheimPlus.GameClasses.Tameable_IsHungry_Patch");
            Unpatch("ValheimPlus.GameClasses.Tameable_Awake_Patch");
            Unpatch("ValheimPlus.GameClasses.Tameable_Alerted_Patches");

            Unpatch("ValheimPlus.GameClasses.Procreation_Awake_Patch");
            Unpatch("ValheimPlus.GameClasses.Procreation_Procreate_Patch");

            Unpatch("ValheimPlus.GameClasses.Character_Damage_Patch");
            Unpatch("ValheimPlus.GameClasses.Character_GetHoverText_Patch");

            Unpatch("ValheimPlus.GameClasses.EggGrow_Start_Patch");
            Unpatch("ValheimPlus.GameClasses.EggGrow_CanGrow_Transpiler");
            Unpatch("ValheimPlus.GameClasses.EggGrow_GrowUpdate_Transpiler");
            Unpatch("ValheimPlus.GameClasses.EggGrow_GetHoverText_Patch");

            Unpatch("ValheimPlus.GameClasses.Growup_Start_Patch");

            Unpatch("ValheimPlus.GameClasses.MonsterAI_UpdateAI_Transpiler");
            Unpatch("ValheimPlus.GameClasses.MonsterAI_UpdateSleep_Patch");
        }

        private static void Unpatch(string patchTypeName)
        {
            Type patchType = assembly.GetType(patchTypeName);

            if (patchType == null)
            {
                Plugin.LogWarning($"  Not found: {patchTypeName}");
                return;
            }

            int count = 0;

            foreach (MethodBase original in Harmony.GetAllPatchedMethods())
            {
                HarmonyLib.Patches patchInfo = Harmony.GetPatchInfo(original);

                if (patchInfo == null)
                    continue;

                Patch[] patches = patchInfo.Prefixes
                    .Concat(patchInfo.Postfixes)
                    .Concat(patchInfo.Transpilers)
                    .Concat(patchInfo.Finalizers)
                    .Where(p => p.PatchMethod?.DeclaringType == patchType)
                    .ToArray();

                foreach (Patch patch in patches)
                {
                    harmony.Unpatch(original, patch.PatchMethod);
                    count++;
                }
            }

            if (count > 0)
            {
                Plugin.LogInfo($"  Unpatched: {patchTypeName} ({count})");
            }
            else
            {
                Plugin.LogWarning($"  No active patches found: {patchTypeName}");
            }
        }
    }
}