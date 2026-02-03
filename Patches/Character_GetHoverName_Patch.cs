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
            
            if (tameable && Plugin.Configs.ShowTamingProgress.Value)
            {
                if (__instance.IsTamed() == false && tameable.m_tamingTime > 0) // 0 = disable taming
                {
                    if (Helpers.ZNetHelper.TryGetZDO(tameable, out ZDO zdo))
                    {
                        var remainingTime = zdo.GetFloat(ZDOVars.s_tameTimeLeft, tameable.m_tamingTime);
                        if (remainingTime < tameable.m_tamingTime)
                        {
                            var percent = (float)(int)((1f - Mathf.Clamp01(remainingTime / tameable.m_tamingTime)) * 100f * precision) / precision;
                            string percentText = percent.ToString($"F{decimals}", System.Globalization.CultureInfo.InvariantCulture);
                            __result += Localization.instance.Localize($" (T:{percentText}%)");
                        }
                        // hotfix
                        else if (remainingTime > tameable.m_tamingTime)
                        {
                            zdo.Set(ZDOVars.s_tameTimeLeft, tameable.m_tamingTime);
                        }
                    }
                }
            }
            
            if (growup && Plugin.Configs.ShowOffspringGrowProgress.Value)
            {
                var growupAPI = Internals.GrowupAPI.GetOrCreate(growup);
                if (growupAPI.m_baseAI != null)
                {
                    var remainingTime = growup.m_growTime - (float)growupAPI.m_baseAI.GetTimeSinceSpawned().TotalSeconds;
                    var percent = (float)(int)((1f - Mathf.Clamp01(remainingTime / growup.m_growTime)) * 100f * precision) / precision;
                    string percentText = percent.ToString($"F{decimals}", System.Globalization.CultureInfo.InvariantCulture);
                    __result += Localization.instance.Localize($" (G:{percentText}%)");
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
