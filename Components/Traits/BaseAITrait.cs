using OfTamingAndBreeding.Components.Base;
using OfTamingAndBreeding.Components.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OfTamingAndBreeding.Components.Traits
{
    public class BaseAITrait : OTABComponent<BaseAITrait>
    {

        // set in awake
        [NonSerialized] private ZNetView m_nview = null;
        [NonSerialized] private BaseAI m_baseAI = null;

        private void Awake()
        {
            m_nview = GetComponent<ZNetView>();
            m_baseAI = GetComponent<BaseAI>();
        }

        private static readonly int m_itemMask = LayerMask.GetMask("item");
        private static readonly Collider[] colliders = new Collider[256]; // 256 should be enough for now

        public static bool CanConsume(IReadOnlyList<ItemDrop> consumeList, string itemName)
        {
            foreach (ItemDrop item in consumeList)
            {
                if (item.m_itemData.m_shared.m_name == itemName)
                {
                    return true;
                }
            }
            return false;
        }

        public ItemDrop FindClosestConsumableItem(float maxRange, IReadOnlyList<ItemDrop> consumeList)
        {
            var pos = m_baseAI.transform.position;

            int count = Physics.OverlapSphereNonAlloc(pos, maxRange, colliders, m_itemMask);
            if (count <= 0)
                return null;

            ItemDrop chosen = null;
            float num = 999999f;

            for (int i = 0; i < count; i++)
            {
                Collider col = colliders[i];
                if (!col)
                    continue;

                var rb = col.attachedRigidbody;
                if (!rb)
                    continue;

                ItemDrop item = rb.GetComponent<ItemDrop>();
                if (!item)
                    continue;

                // Same as vanilla: ZNetView must be valid + item must be consumable
                var nview = item.GetComponent<ZNetView>();
                if (!nview || !nview.IsValid())
                    continue;

                var data = item.m_itemData;
                if (data == null || !CanConsume(consumeList, data.m_shared.m_name))
                    continue;

                float num2 = Vector3.Distance(item.transform.position, pos);
                if (chosen == null || num2 < num)
                {
                    chosen = item;
                    num = num2;
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

            int count = Physics.OverlapSphereNonAlloc(pos, maxRange, colliders, m_itemMask);
            if (count <= 0)
                return null;

            ItemDrop chosen = null;
            float totalWeight = 0f;

            for (int i = 0; i < count; i++)
            {
                Collider col = colliders[i];
                if (!col)
                    continue;

                var rb = col.attachedRigidbody;
                if (!rb)
                    continue;

                ItemDrop item = rb.GetComponent<ItemDrop>();
                if (!item)
                    continue;

                // Same as vanilla: ZNetView must be valid + item must be consumable
                var nview = item.GetComponent<ZNetView>();
                if (!nview || !nview.IsValid())
                    continue;

                var data = item.m_itemData;
                if (data == null || !CanConsume(consumeList, data.m_shared.m_name))
                    continue;

                //float dist = (item.transform.position - pos).sqrMagnitude;
                float dist = Vector3.Distance(item.transform.position, pos); // this costs more performance but z-distance could be important
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
