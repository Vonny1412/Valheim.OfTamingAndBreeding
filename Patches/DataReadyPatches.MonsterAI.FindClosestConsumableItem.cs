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
            // note: for animals the find-consume-item logic is handled here:
            // BaseAI_IdleMovement_Prefix -> if (trait.IdleMovement(dt))
            // -> BaseAITrait.IdleMovement() -> if (m_animalAITrait && m_animalAITrait.IdleMovement(dt))
            // -> AnimalAITrait.IdleMovement() -> if (UpdateConsumeItem(dt)) return true;

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

    }
}
