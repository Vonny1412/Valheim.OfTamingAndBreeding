using HarmonyLib;
using OfTamingAndBreeding.Components.Traits;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {

        [HarmonyPatch(typeof(MonsterAI), "FindClosestConsumableItem")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        private static bool MonsterAI_FindClosestConsumableItem_Prefix(MonsterAI __instance, ref ItemDrop __result)
        {
            if (__instance.TryGetComponent<BaseAITrait>(out var trait))
            {
                if (Plugin.Configs.UseBetterSearchForFood.Value == true)
                {
                    __result = trait.FindNearbyConsumableItem(__instance.m_consumeSearchRange, __instance.m_consumeItems);
                }
                else
                {
                    __result = trait.FindClosestConsumableItem(__instance.m_consumeSearchRange, __instance.m_consumeItems);
                }
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(MonsterAI), "UpdateAI")]
        [HarmonyPrefix]
        private static bool MonsterAI_UpdateAI_Prefix(MonsterAI __instance, float dt)
        {
            if (__instance.TryGetComponent<MonsterAITrait>(out var trait) && trait.UpdateAI(dt))
            {
                return false;
            }
            return true;
        }

    }
}
