using System;
using UnityEngine;

namespace OfTamingAndBreeding.Components.SpecialPrefabs
{
    public class OTABSpecialConsumeAnyItem : OTABSpecialConsumableItem
    {

        public override bool Compare(ItemDrop otherItemDrop)
        {
            return true;
        }

        internal static OTABSpecialConsumeAnyItem AddComponentToSpecialPrefab(GameObject prefab, string sharedName)
        {
            var itemDrop = prefab.AddComponent<ItemDrop>();
            itemDrop.m_itemData.m_shared = new ItemDrop.ItemData.SharedData
            {
                m_name = sharedName,
            };
            return prefab.AddComponent<OTABSpecialConsumeAnyItem>();
        }

    }
}
