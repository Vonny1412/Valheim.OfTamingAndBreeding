using HarmonyLib;
using OfTamingAndBreeding.Components;
using OfTamingAndBreeding.Components.Traits;
using OfTamingAndBreeding.Utils;
using UnityEngine;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {

        [HarmonyPatch(typeof(EggGrow), "CanGrow")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void EggGrow_CanGrow_Postfix(EggGrow __instance, ref bool __result)
        {
            // postfix = less calls
            if (__result == false)
            {
                // cannot grow afterall
                return;
            }

            var eggPosition = __instance.transform.position;

            if (__instance.TryGetComponent<OTABEgg>(out var component))
            {
                if (component.m_requireBiome != Heightmap.Biome.None && !Utils.EnvironmentUtils.IsInBiome(eggPosition, component.m_requireBiome))
                {
                    __result = false;
                }

                if (component.m_requireLiquid != EnvironmentUtils.LiquidTypeEx.None)
                {
                    var liquidType = component.m_requireLiquid;
                    var liquidDepth = component.m_requireLiquidDepth;
                    switch (liquidType)
                    {
                        case Utils.EnvironmentUtils.LiquidTypeEx.Water:
                            if (!Utils.EnvironmentUtils.IsInWater(eggPosition, liquidDepth))
                            {
                                __result = false;
                            }
                            break;
                        case Utils.EnvironmentUtils.LiquidTypeEx.Tar:
                            if (!Utils.EnvironmentUtils.IsInTar(eggPosition, liquidDepth))
                            {
                                __result = false;
                            }
                            break;

                            // todo: lava?
                    }
                }

                // ... maybe more to come
            }
        }

        [HarmonyPatch(typeof(EggGrow), "UpdateEffects")]
        [HarmonyPostfix]
        private static void EggGrow_UpdateEffects_Postfix(EggGrow __instance, float grow)
        {
            foreach (var r in __instance.GetComponentsInChildren<ParticleSystemRenderer>(true))
            {
                r.enabled = grow == 0;
            }
        }

        [HarmonyPatch(typeof(EggGrow), "GrowUpdate")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        private static bool EggGrow_GrowUpdate_Prefix(EggGrow __instance)
        {
            if (__instance.TryGetComponent<EggGrowTrait>(out var trait) && trait.GrowUpdate())
            {
                return false;
            }
            return true;
        }

    }
}
