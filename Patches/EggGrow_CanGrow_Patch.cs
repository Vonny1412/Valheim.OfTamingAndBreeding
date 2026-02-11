using HarmonyLib;
using OfTamingAndBreeding.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Patches
{



    [HarmonyPatch(typeof(EggGrow), "CanGrow")]
    static class EggGrow_CanGrow_Patch
    {
        [HarmonyPriority(Priority.Last)]
        static void Postfix(EggGrow __instance, ref bool __result)
        {
            // postfix = less calls
            if (__result == false)
            {
                return;
            }

            var eggPosition = __instance.transform.position;
            var eggPrefabName = Utils.GetPrefabName(__instance.gameObject.name);

            var needsAnyBiome = Runtime.EggGrow.GetEggNeedsAnyBiome(eggPrefabName);
            if (needsAnyBiome != Heightmap.Biome.None && !Helpers.EnvironmentHelper.IsInBiome(eggPosition, needsAnyBiome))
            {
                __result = false;
                return;
            }

            var needsLiquid = Runtime.EggGrow.GetEggNeedsLiquid(eggPrefabName);
            if (needsLiquid != null)
            {
                switch (needsLiquid.Type)
                {
                    case Helpers.EnvironmentHelper.LiquidTypeEx.Water:
                        if (!Helpers.EnvironmentHelper.IsInWater(eggPosition, needsLiquid.Depth))
                        {
                            __result = false;
                            return;
                        }
                        break;
                    case Helpers.EnvironmentHelper.LiquidTypeEx.Tar:
                        if (!Helpers.EnvironmentHelper.IsInTar(eggPosition, needsLiquid.Depth))
                        {
                            __result = false;
                            return;
                        }
                        break;

                        // todo: lava
                }
            }


            // ... maybe more to come



        }
    }
    /** original method
    private bool CanGrow()
    {
        if (m_item.m_itemData.m_stack > 1)
        {
            return false;
        }

        if (m_requireNearbyFire && !EffectArea.IsPointInsideArea(base.transform.position, EffectArea.Type.Heat, 0.5f))
        {
            return false;
        }

        if (m_requireUnderRoof)
        {
            Cover.GetCoverForPoint(base.transform.position, out var coverPercentage, out var underRoof, 0.1f);
            if (!underRoof || coverPercentage < m_requireCoverPercentige)
            {
                return false;
            }
        }

        return true;
    }
    **/




}
