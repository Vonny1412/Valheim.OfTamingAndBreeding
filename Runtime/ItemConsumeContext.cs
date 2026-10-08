using OfTamingAndBreeding.Components.Traits;
using System;

namespace OfTamingAndBreeding.Runtime
{
    public static class ItemConsumeContext
    {
        [ThreadStatic] private static bool hasValue;
        [ThreadStatic] private static bool lastItemDroppedByPlayer;
        [ThreadStatic] private static int lastItemInstanceID;


        [ThreadStatic] private static ItemDrop externalFeedItem;

        public static void Clear()
        {
            hasValue = false;
            lastItemDroppedByPlayer = false;
            lastItemInstanceID = 0;
            externalFeedItem = null;
        }

        public static void SetItem(ItemDrop item)
        {
            if (ItemDropTrait.TryGet(item.gameObject, out var trait))
            {
                hasValue = true;
                lastItemDroppedByPlayer = trait.IsDroppedByPlayer();
                lastItemInstanceID = item.GetInstanceID();
            }
        }

        public static void SetExternalFeedItem(ItemDrop.ItemData item)
        {
            externalFeedItem = item?.m_dropPrefab
                ? item.m_dropPrefab.GetComponent<ItemDrop>()
                : null;
        }

        public static void ResolveExternalFeedItem(ref ItemDrop item)
        {
            if (item || !externalFeedItem)
            {
                return;
            }

            item = externalFeedItem;

            hasValue = true;
            lastItemDroppedByPlayer = true;
            lastItemInstanceID = item.GetInstanceID();
        }

        public static bool CheckItem(ItemDrop item, out bool droppedByPlayer)
        {
            droppedByPlayer = false;
            if (item && hasValue && lastItemInstanceID == item.GetInstanceID())
            {
                droppedByPlayer = lastItemDroppedByPlayer;
                return true;
            }
            return false;
        }
    }
}
