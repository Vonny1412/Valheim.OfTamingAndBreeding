using OfTamingAndBreeding.Components.Base;
using OfTamingAndBreeding.OTABUtils;
using System;

namespace OfTamingAndBreeding.Components.Traits
{
    public class ItemDropTrait : OTABComponent<ItemDropTrait>
    {

        // set in awake
        [NonSerialized] private ZNetView m_nview = null;
        [NonSerialized] private ItemDrop m_itemDrop = null;

        private void Awake()
        {
            m_nview = GetComponent<ZNetView>();
            m_itemDrop = GetComponent<ItemDrop>();
        }

        public bool TryGetValidItemDrop(out ItemDrop itemDrop)
        {
            var nview = m_nview;
            if (nview && nview.IsValid())
            {
                itemDrop = m_itemDrop;
                return true;
            }
            itemDrop = null;
            return false;
        }

        public void OnItemDropped()
        {
            var nview = m_nview;
            if (nview.IsValid() && nview.IsOwner())
            {
                // hint: we are inside Humanoid_DropItem_Patch
                // Humanoid.DropItem() is calling: ItemDrop itemDrop = ItemDrop.DropItem(...)
                var val = StaticContext.ItemDropContext.DroppedByPlayer;
                ZNetUtils.SetInt(nview.GetZDO(), Plugin.ZDOVars.z_droppedByAnyPlayer, val);
            }
        }

        public void OnOneRemoved()
        {
            var nview = m_nview;
            if (nview.IsValid())
            {
                var zdo = nview.GetZDO();
                StaticContext.ItemConsumeContext.hasValue = true;
                StaticContext.ItemConsumeContext.lastItemDroppedByAnyPlayer = zdo.GetInt(Plugin.ZDOVars.z_droppedByAnyPlayer, 0);
                StaticContext.ItemConsumeContext.lastItemInstanceId = m_itemDrop.GetInstanceID();
            }
        }

    }
}
