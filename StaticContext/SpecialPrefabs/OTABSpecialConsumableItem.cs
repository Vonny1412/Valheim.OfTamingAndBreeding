using System;

namespace OfTamingAndBreeding.StaticContext.SpecialPrefabs
{
    public abstract class OTABSpecialConsumableItem : OTABSpecialPrefabComponent
    {
        protected ItemDrop m_itemDrop = null;
        private void Awake()
        {
            m_itemDrop = GetComponent<ItemDrop>();
        }
        public abstract bool Compare(ItemDrop otherItemDrop);
    }
}
