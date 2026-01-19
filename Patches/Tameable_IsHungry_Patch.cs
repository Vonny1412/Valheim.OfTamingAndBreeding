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
            if (!Helpers.ZNetHelper.TryGetZDO(__instance, out ZDO zdo))
            {
                return false;
            }

            // we are checking the same like in Tameable_Awake_Patch
            // the zdo val is getting set here: Tameable_OnConsumedItem_Patch
            // set custom fed duration
            var fedDuration = zdo.GetFloat(Plugin.ZDOVars.s_fedDuration, -1);
            if (fedDuration >= 0) // yes, we do allow 0, too
            {
                __instance.m_fedDuration = fedDuration;
            }

            return true;
        }
        /*
        [HarmonyPriority(Priority.First)]
        static void Postfix(Tameable __instance)
        {
            // not used
        }
        */
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
