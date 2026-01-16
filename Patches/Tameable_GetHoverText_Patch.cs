using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace OfTamingAndBreeding.Patches
{

    [HarmonyPatch(typeof(Tameable), "GetHoverText")]
    static class Tameable_GetHoverText_Patch
    {
        [HarmonyPriority(Priority.Last)]
        static void Postfix(Tameable __instance, ref string __result)
        {
            if (!__instance.IsTamed())
            {
                return;
            }
            
            if (__instance.m_commandable)
            {
                // we are using custom behaviour for commanding
                // key tap = pet
                // key hold for 1 sec = command
                var search = Localization.instance.Localize("$hud_pet");
                // todo: add translation for "Command"
                var replace = Localization.instance.Localize("$hud_pet\n[<color=yellow><b>$ui_hold $KEY_Use</b></color>] Command");
                __result = __result.Replace(search, replace);
            }

            var text = Internals.TameableAPI.GetOrCreate(__instance).GetFeedingHoverText();
            if (text.Length > 0)
            {
                __result += "\n" + text;
            }
            var procreation = __instance.GetComponent<Procreation>();
            if (procreation != null)
            {
                text = Internals.ProcreationAPI.GetOrCreate(procreation).GetProcreationHoverText();
                if (text.Length > 0)
                {
                    __result += "\n" + text;
                }
            }
        }
    }

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
