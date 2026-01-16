using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;

namespace OfTamingAndBreeding.Patches
{

    [HarmonyPatch(typeof(Character), "GetHoverName")]
    static class Character_GetHoverName_Patch
    {
        [HarmonyPriority(Priority.Last)]
        static void Postfix(Character __instance, ref string __result)
        {

            var precision = 1f / Plugin.Configs.HoverProgressPrecision.Value;
            int decimals = Mathf.Max(0, Mathf.RoundToInt(-Mathf.Log10(precision)));

            var tameable = __instance.GetComponent<Tameable>(); // wild creature beeing tamed
            var growup = __instance.GetComponent<Growup>(); // offspring growing up
            
            var isTamed = __instance.IsTamed();
            if (!isTamed)
            {
                if (tameable && Plugin.Configs.ShowTamingProgress.Value)
                {
                    var remainingTime = Internals.API.Tameable.__IAPI_GetRemainingTime_Invoker1.Invoke(tameable, new object[] { });
                    if (remainingTime < tameable.m_tamingTime)
                    {
                        var percent = (float)(int)((1f - Mathf.Clamp01(remainingTime / tameable.m_tamingTime)) * 100f * precision) / precision;
                        string percentText = percent.ToString($"F{decimals}", System.Globalization.CultureInfo.InvariantCulture);
                        __result += Localization.instance.Localize($" ({percentText}%)");
                    }
                }
            }
            else
            {
                if (growup && Plugin.Configs.ShowOffspringGrowProgress.Value)
                {
                    var growupAPI = Internals.GrowupAPI.GetOrCreate(growup);
                    if (growupAPI.m_baseAI != null)
                    {
                        var remainingTime = growup.m_growTime - (float)growupAPI.m_baseAI.GetTimeSinceSpawned().TotalSeconds;
                        var percent = (float)(int)((1f - Mathf.Clamp01(remainingTime / growup.m_growTime)) * 100f * precision) / precision;
                        string percentText = percent.ToString($"F{decimals}", System.Globalization.CultureInfo.InvariantCulture);
                        __result += Localization.instance.Localize($" ({percentText}%)");
                    }
                }
            }

        }
    }

    /** original method
    public string GetHoverName()
    {
        if (IsTamed())
        {
            string text = GetText().RemoveRichTextTags();
            if (text.Length > 0)
            {
                return text;
            }

            return GetName();
        }

        return GetName();
    }
    **/

}
