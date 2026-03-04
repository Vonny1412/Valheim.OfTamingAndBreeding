using HarmonyLib;
using OfTamingAndBreeding.Components.Traits;
using System.Collections.Generic;
using System.Linq;

namespace OfTamingAndBreeding.Patches
{
    internal partial class AlwaysActivePatches
    {

        private static readonly List<string> hoverTextLines = new List<string>();

        [HarmonyPatch(typeof(Character), "GetHoverText")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void Character_GetHoverText_Postfix(Character __instance, ref string __result)
        {
            hoverTextLines.Clear();

            var isTamed = __instance.IsTamed();

            if (__instance.TryGetComponent<TameableTrait>(out var tameableTrait))
            {
                var tamingDisabled = tameableTrait.IsTamingDisabled();
                var fedTimerDisabled = tameableTrait.IsFedTimerDisabled();

                if (!isTamed && tamingDisabled == true)
                {
                    // dont overwrite original text... for now
                    //text = tameable.GetName();
                }
                else
                {
                    if (fedTimerDisabled == true)
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
                        tameableTrait.GetFedTimerHoverText(hoverTextLines);
                    }
                }
            }

            if (__instance.TryGetComponent<CharacterTrait>(out var characterTrait))
            {
                var consumeText = characterTrait.GetConsumeHoverText();
                if (string.IsNullOrEmpty(consumeText) == false)
                {
                    hoverTextLines.Add(consumeText);
                }
            }

            if (isTamed && __instance.TryGetComponent<ProcreationTrait>(out var procreationTrait))
            {
                var procreationText = procreationTrait.GetProcreationHoverText();
                if (string.IsNullOrEmpty(procreationText) == false)
                {
                    hoverTextLines.Add(procreationText);
                }
            }

            if (hoverTextLines.Count > 0)
            {
                if (!__result.EndsWith("\n"))
                {
                    __result += "\n";
                }
                __result += string.Join("\n", hoverTextLines.Where((string line) => line.Trim() != ""));
                hoverTextLines.Clear();
            }

        }
    }
}
