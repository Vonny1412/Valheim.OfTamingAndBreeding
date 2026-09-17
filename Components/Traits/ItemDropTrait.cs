using OfTamingAndBreeding.Components.Core;
using OfTamingAndBreeding.Utilities;
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

        public bool IsDroppedByPlayer()
        {
            if (!m_nview.IsValid())
            {
                return false;
            }
            var zdo = m_nview.GetZDO();
            var droppedByPlayer = zdo.GetInt(Plugin.ZDOVars.z_droppedByAnyPlayer, 0);
            return droppedByPlayer > 0;
        }

        public void SetDroppedByPlayer()
        {
            if (m_nview.IsValid() && m_nview.IsOwner())
            {
                ZDOUtils.SetInt(m_nview.GetZDO(), Plugin.ZDOVars.z_droppedByAnyPlayer, 1);
            }
        }

    }
}
