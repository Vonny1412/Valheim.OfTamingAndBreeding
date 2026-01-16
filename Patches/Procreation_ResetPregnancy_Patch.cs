using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace OfTamingAndBreeding.Patches
{
    [HarmonyPatch(typeof(Procreation), "ResetPregnancy")]
    static class Procreation_ResetPregnancy_Patch
    {
        [HarmonyPriority(Priority.Last)]
        static void Postfix(Procreation __instance)
        {
            // main entry for custom breeding behaviour
            var procreationAPI = Internals.ProcreationAPI.GetOrCreate(__instance);
            procreationAPI.PrepareBreeding();
        }
    }

    /** original method
    private void ResetPregnancy()
    {
        m_nview.GetZDO().Set(ZDOVars.s_pregnant, 0L);
    }
    **/

}
