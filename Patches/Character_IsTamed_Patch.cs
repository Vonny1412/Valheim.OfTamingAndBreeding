using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Patches
{
    [HarmonyPatch(typeof(Character), "IsTamed", new Type[0])]
    static class Character_IsTamed_Patch
    {
        static void Postfix(Character __instance, ref bool __result)
        {
            if (!__result) return; // already not tamed
            if (!Contexts.IsEnemyContext.Active) return;
            if (__instance != Contexts.IsEnemyContext.TargetInstance) return;
            __result = false; // temporary untamed
        }
    }
    /** original method too long to show here **/
}
