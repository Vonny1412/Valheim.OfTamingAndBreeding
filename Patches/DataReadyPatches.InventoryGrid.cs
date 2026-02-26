using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {

        [HarmonyPatch(typeof(InventoryGrid), "DropItem", new[] { typeof(Inventory), typeof(ItemDrop.ItemData), typeof(int), typeof(Vector2i) })]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        private static bool InventoryGrid_DropItem_Prefix(InventoryGrid __instance, Inventory fromInventory, ItemDrop.ItemData item, int amount, Vector2i pos, ref bool __result)
        {
            var m_inventory = __instance.GetInventory();
            ItemDrop.ItemData itemAt = m_inventory.GetItemAt(pos.x, pos.y);
            if (itemAt == null) return true;
            if (item?.m_shared == null || itemAt?.m_shared == null) return true;
            if (item.m_shared.m_name != itemAt.m_shared.m_name) return true;

            if (StaticContext.EggDataContext.IsRegisteredEggSharedName(item.m_shared.m_name))
            {
                // both are otab-eggs
                if (itemAt.m_quality != item.m_quality)
                {
                    // default behavior
                    // we are just removing the limitation of quality>1
                    // maybe a user has forgotten to change egg max quality
                    // this is just a saveguard to prevent stacking two eggs of different quality
                    fromInventory.RemoveItem(item);
                    fromInventory.MoveItemToThis(m_inventory, itemAt, itemAt.m_stack, item.m_gridPos.x, item.m_gridPos.y);
                    m_inventory.MoveItemToThis(fromInventory, item, amount, pos.x, pos.y);

                    __result = true;
                    return false;
                }
            }

            return true;
        }

    }
}
