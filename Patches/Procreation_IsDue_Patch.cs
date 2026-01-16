using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace OfTamingAndBreeding.Patches
{
    [HarmonyPatch(typeof(Procreation), "IsDue")]
    static class Procreation_IsDue_Patch
    {
        [HarmonyPriority(Priority.Last)]
        static bool Prefix(Procreation __instance)
        {
            // duration could have been changed by mods like Seasons
            Internals.ProcreationAPI.GetOrCreate(__instance)
                .RealPregnancyDuration.SaveValue(true);
            return true;
        }
    }

    /** original method
    private bool IsDue()
    {
        long @long = m_nview.GetZDO().GetLong(ZDOVars.s_pregnant, 0L);
        if (@long == 0L)
        {
            return false;
        }

        DateTime dateTime = new DateTime(@long);
        return (ZNet.instance.GetTime() - dateTime).TotalSeconds > (double)m_pregnancyDuration;
    }
    **/

}
