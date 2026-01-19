using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Patches
{
    [HarmonyPatch(typeof(ItemDrop), "DropItem")]
    static class ItemDrop_DropItem_Patch
    {
        static void Postfix(ItemDrop __result)
        {
            if (!__result) return; // should not happen but whatever

            if (!Helpers.ZNetHelper.TryGetZDO(__result, out ZDO zdo, out ZNetView nview))
            {
                return;
            }

            // this is used to prevent creatures consume items not dropped by player
            if (nview.IsOwner())
            {
                zdo.Set(Plugin.ZDOVars.s_droppedByAnyPlayer, Contexts.DropItemContext.DroppedByPlayer);
            }

        }
    }

    /** original method
    public static ItemDrop DropItem(ItemData item, int amount, Vector3 position, Quaternion rotation)
    {
        ItemDrop component = UnityEngine.Object.Instantiate(item.m_dropPrefab, position, rotation).GetComponent<ItemDrop>();
        component.m_itemData = item.Clone();
        if (component.m_itemData.m_quality > 1)
        {
            component.SetQuality(component.m_itemData.m_quality);
        }

        if (amount > 0)
        {
            component.m_itemData.m_stack = amount;
        }

        if (component.m_onDrop != null)
        {
            component.m_onDrop(component);
        }

        component.Save();
        return component;
    }
    **/

}
