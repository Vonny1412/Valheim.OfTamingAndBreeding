using HarmonyLib;
using OfTamingAndBreeding.Components.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {

        [HarmonyPatch(typeof(Growup), "GrowUpdate")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        private static bool Growup_GrowUpdate_Prefix(Growup __instance)
        {
            return __instance.GrowUpdate_PatchPrefix();
        }

        [HarmonyPatch(typeof(Growup), "Start")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void Growup_Start_Postfix(Growup __instance)
        {
            __instance.Start_PatchPostfix();
        }

    }
}
