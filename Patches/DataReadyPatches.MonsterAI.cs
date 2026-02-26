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

        [HarmonyPatch(typeof(MonsterAI), "FindClosestConsumableItem")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        private static bool MonsterAI_FindClosestConsumableItem_Prefix(MonsterAI __instance, ref ItemDrop __result)
        {
            if (Plugin.Configs.UseBetterSearchForFood.Value == true)
            {
                __result = __instance.FindNearbyConsumableItem(__instance.m_consumeSearchRange, __instance.m_consumeItems);
            }
            else
            {
                __result = __instance.FindClosestConsumableItem(__instance.m_consumeSearchRange, __instance.m_consumeItems);
            }
            // skrew it, just replace it completly!
            // because i dont like the vanilla CanConsume-method
            return false;
        }

        [HarmonyPatch(typeof(MonsterAI), "UpdateAI")]
        [HarmonyPrefix]
        private static bool MonsterAI_UpdateAI_Prefix(MonsterAI __instance, float dt)
        {
            return __instance.UpdateAI_PatchPrefix(dt);
        }

    }
}
