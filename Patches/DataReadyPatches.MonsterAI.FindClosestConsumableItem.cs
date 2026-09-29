using HarmonyLib;
using OfTamingAndBreeding.Components.Traits;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {
        [HarmonyPatch(typeof(MonsterAI), "FindClosestConsumableItem")]
        [HarmonyPrefix]
        private static bool MonsterAI_FindClosestConsumableItem_Prefix(MonsterAI __instance, bool __runOriginal, ref ItemDrop __result)
        {
            if (!__runOriginal)
            {
                return false;
            }

            // note: for animals the find-consume-item logic is handled here:
            // BaseAI_IdleMovement_Prefix -> if (trait.IdleMovement(dt))
            // -> BaseAITrait.IdleMovement() -> if (m_animalAITrait && m_animalAITrait.IdleMovement(dt))
            // -> AnimalAITrait.IdleMovement() -> if (UpdateConsumeItem(dt)) return true;

            if (BaseAITrait.TryGet(__instance.gameObject, out var trait))
            {
                __result = trait.FindClosestConsumableItem(__instance.m_consumeSearchRange, __instance.m_consumeItems);
                return false;
            }

            return true;
        }

    }
}
