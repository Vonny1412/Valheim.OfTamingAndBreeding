using HarmonyLib;
using OfTamingAndBreeding.Components.Traits;
using static UnityEngine.Networking.UnityWebRequest;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {

        [HarmonyPatch(typeof(EggGrow), "CanGrow")]
        [HarmonyPostfix]
        private static void EggGrow_CanGrow_Postfix(EggGrow __instance, ref bool __result)
        {
            if (__result == false)
            {
                // cannot grow afterall
                return;
            }

            if (EggGrowTrait.TryGet(__instance.gameObject, out var trait))
            {
                __result = trait.On_CanGrow();
            }
        }

        [HarmonyPatch(typeof(EggGrow), "UpdateEffects")]
        [HarmonyPostfix]
        private static void EggGrow_UpdateEffects_Postfix(EggGrow __instance, float grow)
        {
            if (EggGrowTrait.TryGet(__instance.gameObject, out var trait))
            {
                trait.On_UpdateEffects(grow);
            }
        }

        [HarmonyPatch(typeof(EggGrow), "GrowUpdate")]
        [HarmonyPrefix]
        private static bool EggGrow_GrowUpdate_Prefix(EggGrow __instance, bool __runOriginal)
        {
            if (!__runOriginal)
            {
                // cannot grow (by other mod?)
                return false;
            }

            if (EggGrowTrait.TryGet(__instance.gameObject, out var trait))
            {
                trait.On_GrowUpdate();
                return false;
            }
            return true;
        }

    }
}
