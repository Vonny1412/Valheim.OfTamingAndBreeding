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

        [HarmonyPatch(typeof(Character), "Awake")]
        [HarmonyPostfix]
        private static void Character_Awake_Postfix(Character __instance)
        {
            __instance.SetCharacterStuffIfTamed();
        }

        [HarmonyPatch(typeof(Character), "IsTamed", new Type[0])]
        [HarmonyPostfix]
        private static void Character_IsTamed_Postfix(Character __instance, ref bool __result)
        {
            if (!__result) return; // not tamed by default

            // we temporarly need to change the original returned value
            if (BaseAIExtensions.IsEnemyContext.Active && __instance == BaseAIExtensions.IsEnemyContext.TargetInstance)
            {
                __result = false; // temporary untamed
            }
        }

    }
}
