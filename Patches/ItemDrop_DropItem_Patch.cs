using HarmonyLib;
using OfTamingAndBreeding.Helpers;
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

            if (nview.IsOwner())
            {
                // hint: we are inside Humanoid_DropItem_Patch
                // Humanoid.DropItem() is calling: ItemDrop itemDrop = ItemDrop.DropItem(...)

                var val = Contexts.DropItemContext.DroppedByPlayer;
                ZNetHelper.SetInt(zdo, Plugin.ZDOVars.z_droppedByAnyPlayer, val);
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
