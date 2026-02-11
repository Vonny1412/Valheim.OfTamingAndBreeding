using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace OfTamingAndBreeding.Patches
{
    [HarmonyPatch(typeof(Tameable), "IsHungry")]
    static class Tameable_IsHungry_Patch
    {

        [HarmonyPriority(Priority.Last)]
        static bool Prefix(Tameable __instance)
        {
            var tameableAPI = Internals.TameableAPI.GetOrCreate(__instance);
            tameableAPI.UpdateFedDuration();

            return true;
        }
    }

    /** original method
    public bool IsHungry()
    {
        if (!m_character)
        {
            return false;
        }

        if (m_nview == null)
        {
            return false;
        }

        ZDO zDO = m_nview.GetZDO();
        if (zDO == null)
        {
            return false;
        }

        DateTime dateTime = new DateTime(zDO.GetLong(ZDOVars.s_tameLastFeeding, 0L));
        return (ZNet.instance.GetTime() - dateTime).TotalSeconds > (double)m_fedDuration;
    }
    **/

}
