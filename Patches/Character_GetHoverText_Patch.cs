using HarmonyLib;
using OfTamingAndBreeding.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Patches
{

    [HarmonyPatch(typeof(Character), "GetHoverText")]
    static class Character_GetHoverText_Patch
    {

        [HarmonyPriority(Priority.Last)]
        static void Postfix(Character __instance, ref string __result)
        {
            var textLines = new List<string>();

            var prefabName = Utils.GetPrefabName(__instance.gameObject.name);

            var isTamed = __instance.IsTamed();

            Tameable tameable = __instance.GetComponent<Tameable>();
            if ((bool)tameable)
            {
                if (!isTamed && Runtime.Tameable.GetTamingDisabled(prefabName))
                {
                    __result = tameable.GetName();
                }
                else
                {
                    if (Runtime.Tameable.GetIsEatingDisabled(prefabName))
                    {
                        var hungry = Localization.instance.Localize("$hud_tamehungry");
                        if (!string.IsNullOrEmpty(hungry))
                        {
                            var idx = __result.IndexOf('\n');
                            string firstLine;
                            string rest;

                            if (idx >= 0)
                            {
                                firstLine = __result.Substring(0, idx);
                                rest = __result.Substring(idx); // includes \n
                            }
                            else
                            {
                                firstLine = __result;
                                rest = "";
                            }

                            // remove token in common placements
                            firstLine = firstLine.Replace(", " + hungry, "");
                            firstLine = firstLine.Replace(hungry + ", ", "");
                            firstLine = firstLine.Replace(hungry, "");

                            // cleanup spacing / punctuation artifacts
                            firstLine = firstLine.Replace(",  ", ", ");
                            firstLine = firstLine.Replace("  )", " )");
                            firstLine = firstLine.Replace("(  ", "( ");

                            // remove empty parentheses variants
                            firstLine = firstLine.Replace(" ( )", "");
                            firstLine = firstLine.Replace("( )", "");
                            firstLine = firstLine.Replace("()", "");

                            firstLine = firstLine.TrimEnd();

                            __result = firstLine + rest;
                        }
                    }
                    else
                    {
                        // taming enabled + eating enabled -> show fed timer
                        textLines.AddRange(Internals.TameableAPI.GetOrCreate(tameable).GetFeedingHoverText());
                    }
                }
            }

            textLines.AddRange(Internals.CharacterAPI.GetOrCreate(__instance).GetConsumeHoverText());

            if (isTamed)
            {
                var procreation = __instance.GetComponent<Procreation>();
                if (procreation != null)
                {
                    textLines.AddRange(Internals.ProcreationAPI.GetOrCreate(procreation).GetProcreationHoverText());
                }
            }

            if (textLines.Count > 0)
            {
                if (!__result.EndsWith("\n"))
                    __result += "\n";
                __result += string.Join("\n", textLines.Where((string line) => line.Trim() != ""));
            }

        }
    }

    /** original method
    public virtual string GetHoverText()
    {
        Tameable component = GetComponent<Tameable>();
        if ((bool)component)
        {
            return component.GetHoverText();
        }

        return "";
    }
    **/

}
