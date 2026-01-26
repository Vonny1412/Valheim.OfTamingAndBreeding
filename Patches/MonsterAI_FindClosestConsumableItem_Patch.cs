using HarmonyLib;
using OfTamingAndBreeding.Internals.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Patches
{

    [HarmonyPatch(typeof(MonsterAI), "FindClosestConsumableItem")]
    static class MonsterAI_FindClosestConsumableItem_Patch
    {
        static bool Prefix(MonsterAI __instance, ref ItemDrop __result)
        {
            if (Plugin.Configs.UseBetterSearchForFood.Value == true)
            {
                bool canConsume(ItemDrop.ItemData itemData) => Internals.API.MonsterAI.__IAPI_CanConsume_Invoker1.Invoke(__instance, new object[] { itemData });
                __result = Internals.Behaviors.ConsumeBehavior.FindNearbyConsumableItem(__instance, __instance.m_consumeSearchRange, canConsume);
                return false;
            }
            return true;
        }
    }
    
}
