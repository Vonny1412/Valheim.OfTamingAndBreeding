using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Chat;

namespace OfTamingAndBreeding.Patches
{
    [HarmonyPatch(typeof(ItemDrop), "GetHoverText")]
    static class ItemDrop_GetHoverText_Patch
    {
        static void Postfix(ItemDrop __instance, ref string __result)
        {
            int nl = __result.IndexOf('\n');
            if (nl <= 0) return;

            var eggGrow = __instance.GetComponent<EggGrow>();
            if (!eggGrow) return;
            if (!Utils.ZNetHelper.TryGetZDO(__instance, out ZDO zdo)) return;

            string extraText;

            if (eggGrow.m_growTime > 0)
            {

                float growStart = zdo.GetFloat(ZDOVars.s_growStart);

                if (__instance.m_itemData.m_stack > 1)
                {
                    extraText = Localization.instance.Localize("$item_chicken_egg_stacked");
                }
                else if (growStart <= 0f)
                {
                    extraText = Localization.instance.Localize("$item_chicken_egg_cold");
                }
                else
                {
                    float precision = 1f / Plugin.Configs.HoverProgressPrecision.Value;
                    int decimals = Mathf.Max(0, Mathf.RoundToInt(-Mathf.Log10(precision)));

                    float remainingTime = (float)((growStart + eggGrow.m_growTime) - ZNet.instance.GetTimeSeconds());
                    float pctRaw = (1f - Mathf.Clamp01(remainingTime / eggGrow.m_growTime)) * 100f;

                    float pct = Mathf.Floor(pctRaw * precision) / precision; // no "jumping forward"
                    string pctText = pct.ToString($"F{decimals}", System.Globalization.CultureInfo.InvariantCulture);

                    extraText = $"({pctText}%)";
                }

            }
            else
            {
                extraText = $"(?)";
            }


            __result = __result.Substring(0, nl) + " " + extraText + __result.Substring(nl);
        }
    }

    /** original method
    public string GetHoverText()
    {
        Load();
        string text = m_itemData.m_shared.m_name;
        if (m_itemData.m_quality > 1)
        {
            text = text + "[" + m_itemData.m_quality + "] ";
        }

        if (m_itemData.m_shared.m_itemType == ItemData.ItemType.Consumable && IsPiece())
        {
            return Localization.instance.Localize(text + "\n[<color=yellow><b>$KEY_Use</b></color>] " + (m_itemData.m_shared.m_isDrink ? "$item_drink" : "$item_eat"));
        }

        if (m_itemData.m_stack > 1)
        {
            text = text + " x" + m_itemData.m_stack;
        }

        return Localization.instance.Localize(text + "\n[<color=yellow><b>$KEY_Use</b></color>] $inventory_pickup");
    }
    **/

}
