using HarmonyLib;
using System;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {

        [HarmonyPatch(typeof(Character), "IsTamed", new Type[0])]
        [HarmonyPostfix]
        private static void Character_IsTamed_Postfix(Character __instance, ref bool __result)
        {
            if (!__result) return; // not tamed by default

            // we temporarly need to change the original returned value
            if (StaticContext.IsEnemyContext.Active && __instance == StaticContext.IsEnemyContext.TargetInstance)
            {
                __result = false; // temporary untamed to enable aggression
            }
        }

    }
}
