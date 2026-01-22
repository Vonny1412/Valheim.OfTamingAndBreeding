using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Patches
{

    [HarmonyPatch(typeof(Humanoid), "DropItem")]
    static class Humanoid_DropItem_Patch
    {
        static void Prefix(Humanoid __instance)
        {

            Contexts.DropItemContext.DroppedByPlayer = __instance.IsPlayer() ? 1 : 0;
        }
        static void Finalizer()
        {
            Contexts.DropItemContext.Clear();
        }
    }

    /** original method
    public bool DropItem(Inventory inventory, ItemDrop.ItemData item, int amount)
    {
        if (inventory == null)
        {
            inventory = m_inventory;
        }

        if (amount == 0)
        {
            return false;
        }

        if (item.m_shared.m_questItem)
        {
            Message(MessageHud.MessageType.Center, "$msg_cantdrop");
            return false;
        }

        if (amount > item.m_stack)
        {
            amount = item.m_stack;
        }

        RemoveEquipAction(item);
        UnequipItem(item, triggerEquipEffects: false);
        if (m_hiddenLeftItem == item)
        {
            m_hiddenLeftItem = null;
            SetupVisEquipment(m_visEquipment, isRagdoll: false);
        }

        if (m_hiddenRightItem == item)
        {
            m_hiddenRightItem = null;
            SetupVisEquipment(m_visEquipment, isRagdoll: false);
        }

        if (amount == item.m_stack)
        {
            ZLog.Log("drop all " + amount + "  " + item.m_stack);
            if (!inventory.RemoveItem(item))
            {
                ZLog.Log("Was not removed");
                return false;
            }
        }
        else
        {
            ZLog.Log("drop some " + amount + "  " + item.m_stack);
            inventory.RemoveItem(item, amount);
        }

        ItemDrop itemDrop = ItemDrop.DropItem(item, amount, base.transform.position + base.transform.forward + base.transform.up, base.transform.rotation);
        if (IsPlayer())
        {
            itemDrop.OnPlayerDrop();
        }

        float num = 5f;
        if (item.GetWeight() >= 300f)
        {
            num = 0.5f;
        }

        itemDrop.GetComponent<Rigidbody>().linearVelocity = (base.transform.forward + Vector3.up) * num;
        m_zanim.SetTrigger("interact");
        m_dropEffects.Create(base.transform.position, Quaternion.identity);
        Message(MessageHud.MessageType.TopLeft, "$msg_dropped " + itemDrop.m_itemData.m_shared.m_name, itemDrop.m_itemData.m_stack, itemDrop.m_itemData.GetIcon());
        return true;
    }
    **/

}
