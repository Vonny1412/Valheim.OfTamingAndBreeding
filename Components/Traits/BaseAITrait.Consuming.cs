using OfTamingAndBreeding.Common;
using OfTamingAndBreeding.Components.Core;
using OfTamingAndBreeding.Components.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OfTamingAndBreeding.Components.Traits
{
    public partial class BaseAITrait : OTABComponent<BaseAITrait>
    {

        public class ConsumeItem
        {
            internal ItemDrop itemDrop;
            public float fedDurationFactor;
        }

        private static readonly int s_consumeItemMask = LayerMask.GetMask("item");
        private static readonly Collider[] s_consumeColliders = new Collider[64];

        internal static readonly IndexedDataStore<ConsumeItem[]> s_consumeItemsStore = new IndexedDataStore<ConsumeItem[]>();
        [SerializeField] internal int m_consumeItemsStoreIndex = -1;
        [NonSerialized] public ConsumeItem[] m_consumeItems = null;

        public static bool CanConsume(IReadOnlyList<ItemDrop> consumeList, ItemDrop checkItem)
        {
            var data = checkItem.m_itemData;
            if (data == null)
            {
                return false;
            }

            string checkItemName = data.m_shared.m_name;
            foreach (ItemDrop consumeItem in consumeList)
            {
                if (consumeItem.m_itemData.m_shared.m_name == checkItemName)
                {
                    return true;
                }
            }

            return false;
        }

        public ItemDrop FindClosestConsumableItem(float maxRange, IReadOnlyList<ItemDrop> consumeList)
        {

            var pos = m_baseAI.transform.position;

            int count = Physics.OverlapSphereNonAlloc(pos, maxRange, s_consumeColliders, s_consumeItemMask);
            if (count <= 0)
                return null;

            ItemDrop chosen = null;
            float bestDist = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                Collider col = s_consumeColliders[i];
                if (!col)
                    continue;

                var rb = col.attachedRigidbody;
                if (!rb)
                    continue;

                if (!rb.TryGetComponent<ItemDropTrait>(out var trait))
                    continue;
                if (trait.TryGetValidItemDrop(out var item) == false)
                    continue;
                if (!CanConsume(consumeList, item))
                    continue;

                //float dist = MathUtils.DistanceSqrXZ(item.transform.position, pos);
                var itemPos = item.transform.position;
                float dx = itemPos.x - pos.x;
                float dz = itemPos.z - pos.z;
                var dist = dx * dx + dz * dz;
                if (chosen == null || dist < bestDist)
                {
                    chosen = item;
                    bestDist = dist;
                }
            }

            if (chosen != null)
            {
                if (m_baseAI.HavePath(chosen.transform.position))
                {
                    return chosen;
                }
            }

            return null;
        }

        public ItemDrop FindNearbyConsumableItem(float maxRange, IReadOnlyList<ItemDrop> consumeList)
        {
            var pos = m_baseAI.transform.position;

            int count = Physics.OverlapSphereNonAlloc(pos, maxRange, s_consumeColliders, s_consumeItemMask);
            if (count <= 0)
                return null;

            ItemDrop chosen = null;
            float totalWeight = 0f;

            for (int i = 0; i < count; i++)
            {
                Collider col = s_consumeColliders[i];
                if (!col)
                    continue;

                var rb = col.attachedRigidbody;
                if (!rb)
                    continue;

                if (!rb.TryGetComponent<ItemDropTrait>(out var trait))
                    continue;
                if (trait.TryGetValidItemDrop(out var item) == false)
                    continue;
                if (!CanConsume(consumeList, item))
                    continue;

                //float dist = Vector3.Distance(item.transform.position, pos);
                var itemPos = item.transform.position;
                float dx = itemPos.x - pos.x;
                float dz = itemPos.z - pos.z;
                float dist = Mathf.Sqrt(dx * dx + dz * dz);
                if (dist > maxRange)
                    continue;

                // Higher weight when closer (linear)
                float w = maxRange - dist;
                if (w <= 0f)
                    continue;

                // One-pass weighted selection (roulette/reservoir)
                totalWeight += w;
                if (UnityEngine.Random.value * totalWeight <= w)
                {
                    chosen = item;
                }
            }

            if (chosen != null)
            {
                if (m_baseAI.HavePath(chosen.transform.position))
                {
                    return chosen;
                }
            }

            return null;
        }

    }
}
