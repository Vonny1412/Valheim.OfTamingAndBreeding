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

        [HarmonyPatch(typeof(Humanoid), "DropItem")]
        [HarmonyPrefix]
        private static void Humanoid_DropItem_Prefix(Humanoid __instance)
        {
            // entry point for RequireFoodDroppedByPlayer-feature
            ItemDropExtensions.DropContext.DroppedByPlayer = __instance.IsPlayer() ? 1 : 0;
        }

        [HarmonyPatch(typeof(Humanoid), "DropItem")]
        [HarmonyFinalizer]
        private static void Humanoid_DropItem_Finalizer()
        {
            ItemDropExtensions.DropContext.Clear();
        }

    }
}
