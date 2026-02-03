using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Patches
{
    [HarmonyPatch(typeof(Character), "GetGroup")]
    static class Character_GetGroup_Patch
    {
        static bool Prefix(Character __instance, ref string __result)
        {
            var prefabName = Utils.GetPrefabName(__instance.gameObject.name);

            if (Contexts.DataContext.GetGroupWhenTamed(prefabName, out string group))
            {
                __result = group;
                return false;
            }

            return true;
        }
    }
}
