using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace OfTamingAndBreeding.Patches
{

    /*
    [HarmonyPatch(typeof(Tameable), "GetHoverText")]
    static class Tameable_GetHoverText_Patch
    {
        [HarmonyPriority(Priority.Last)]
        static void Postfix(Tameable __instance, ref string __result)
        {
            // currently not used
        }
    }
    */

    /** original method
    public string GetHoverText()
    {
        if (!m_nview.IsValid())
        {
            return "";
        }

        string text = GetName();
        if (IsTamed())
        {
            if ((bool)m_character)
            {
                text += Localization.instance.Localize(" ( $hud_tame, " + GetStatusString() + " )");
            }

            text += Localization.instance.Localize("\n[<color=yellow><b>$KEY_Use</b></color>] $hud_pet");
            if (ZInput.IsNonClassicFunctionality() && ZInput.IsGamepadActive())
            {
                return text + Localization.instance.Localize("\n[<color=yellow><b>$KEY_AltKeys + $KEY_Use</b></color>] $hud_rename");
            }

            return text + Localization.instance.Localize("\n[<color=yellow><b>$KEY_AltPlace + $KEY_Use</b></color>] $hud_rename");
        }

        int tameness = GetTameness();
        if (tameness <= 0)
        {
            return text + Localization.instance.Localize(" ( $hud_wild, " + GetStatusString() + " )");
        }

        return text + Localization.instance.Localize(" ( $hud_tameness  " + tameness + "%, " + GetStatusString() + " )");
    }
    **/

}
