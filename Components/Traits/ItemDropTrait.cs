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

            Register(this);
        }

        private void OnDestroy()
        {
            Unregister(this);
        }

        public bool TryGetValidItemDrop(out ItemDrop itemDrop)
        {
            if (m_nview && m_nview.IsValid())
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
            if (m_nview.IsValid())
            {
                var zdo = m_nview.GetZDO();
                StaticContext.ItemConsumeContext.hasValue = true;
                StaticContext.ItemConsumeContext.lastItemDroppedByAnyPlayer = zdo.GetInt(Plugin.ZDOVars.z_droppedByAnyPlayer, 0);
                StaticContext.ItemConsumeContext.lastItemInstanceId = m_itemDrop.GetInstanceID();
            }
        }

    }
}
