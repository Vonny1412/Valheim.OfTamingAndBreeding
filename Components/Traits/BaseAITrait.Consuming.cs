using OfTamingAndBreeding.Components.Core;
using OfTamingAndBreeding.Components.Extensions;
using OfTamingAndBreeding.Runtime;
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

        private float m_consumeSearchTimer = 0f;
        private Container m_containerConsumeTarget = null;

        public bool IsHungry()
        {
            if (!m_tameableTrait)
            {
                return true;
            }
            return m_tameableTrait.IsHungry();
        }

        private bool CheckConsumeTimer(float dt)
        {
            m_consumeSearchTimer += dt;

            if (m_monsterAI)
            {
                if (m_consumeSearchTimer >= m_monsterAI.m_consumeSearchInterval)
                {
                    m_consumeSearchTimer = 0;
                    return true;
                }
            }
            else if (m_animalAITrait)
            {
                if (m_consumeSearchTimer >= m_animalAITrait.m_consumeSearchInterval)
                {
                    m_consumeSearchTimer = 0;
                    return true;
                }
            }
            return false;
        }

        public float GetConsumeSearchRange()
        {
            if (m_monsterAI)
            {
                return m_monsterAI.m_consumeSearchRange;
            }
            else if (m_animalAITrait)
            {
                return m_animalAITrait.m_consumeSearchRange;
            }
            return 0f;
        }

        public float GetConsumeSearchInterval()
        {
            if (m_monsterAI)
            {
                return m_monsterAI.m_consumeSearchInterval;
            }
            else if (m_animalAITrait)
            {
                return m_animalAITrait.m_consumeSearchInterval;
            }
            return 0f;
        }

        public float GetConsumeRange()
        {
            if (m_monsterAI)
            {
                return m_monsterAI.m_consumeRange;
            }
            else if (m_animalAITrait)
            {
                return m_animalAITrait.m_consumeRange;
            }
            return 1f;
        }

        private ItemDrop.ItemData FindConsumableItem(Container container)
        {
            Inventory inventory = container.GetInventory();
            if (inventory == null)
            {
                return null;
            }

            foreach (ItemDrop.ItemData item in inventory.GetAllItems())
            {
                if (CanConsume(m_consumeItems, item))
                {
                    return item;
                }
            }

            return null;
        }

        private Vector3 GetContainerPathPosition(Container container)
        {
            Vector3 position = transform.position;
            if (!ContainerFeeding.TryGetContainerColliders(container, out Collider[] colliders))
            {
                return container.transform.position;
            }

            Vector3 closest = container.transform.position;
            float closestDistanceSqr = float.MaxValue;

            foreach (Collider collider in colliders)
            {
                if (!collider || !collider.enabled || collider.isTrigger)
                {
                    continue;
                }

                Vector3 point = collider.ClosestPoint(position);
                float distanceSqr = (point - position).sqrMagnitude;
                if (distanceSqr < closestDistanceSqr)
                {
                    closest = point;
                    closestDistanceSqr = distanceSqr;
                }
            }

            Vector3 direction = position - closest;
            direction.y = 0f;
            float directionSq = direction.sqrMagnitude;
            if (directionSq > 0.001f)
            {
                direction *= 1f / Mathf.Sqrt(directionSq);
                closest += direction * 1f;
            }

            return closest;
        }

        public bool TryFindContainerConsumeTarget(float dt)
        {
            if (!CheckConsumeTimer(dt) || !IsHungry())
            {
                return false;
            }

            Vector3 position = transform.position;
            float searchRange = GetConsumeSearchRange();

            if (m_tameableTrait && m_tameableTrait.IsTamed())
            {
                searchRange *= Plugin.Configs.ContainerSearchRangeFactor.Value;
            }

            float searchRangeSq = searchRange * searchRange;
            Container nearestContainer = null;
            float nearestDistanceSq = float.MaxValue;

            foreach (Container container in Runtime.ContainerFeeding.Containers)
            {
                if (!container)
                {
                    continue;
                }

                Vector3 consumePosition = container.transform.position;
                float distanceSq = (consumePosition - position).sqrMagnitude;
                if (distanceSq > searchRangeSq ||
                    distanceSq >= nearestDistanceSq)
                {
                    continue;
                }

                ItemDrop.ItemData item = FindConsumableItem(container);
                if (item == null)
                {
                    continue;
                }

                if (!m_baseAI.HavePath(GetContainerPathPosition(container)))
                {
                    continue;
                }

                nearestContainer = container;
                nearestDistanceSq = distanceSq;
            }

            if (!nearestContainer)
            {
                return false;
            }

            m_containerConsumeTarget = nearestContainer;
            return UpdateContainerConsumeItem(dt);
        }

        public bool UpdateContainerConsumeItem(float dt)
        {
            if (!m_containerConsumeTarget)
            {
                return false;
            }

            ItemDrop.ItemData item = FindConsumableItem(m_containerConsumeTarget);
            if (item == null)
            {
                m_containerConsumeTarget = null;
                return false;
            }

            Vector3 position = m_containerConsumeTarget.transform.position;
            float consumeRange = GetConsumeRange();
            float distance = Vector3.Distance(transform.position, position);
            bool moveToResult = m_baseAI.MoveTo(dt, position, consumeRange, run: false);

            if (moveToResult)
            {
                m_baseAI.LookAt(position);
                if (m_baseAI.IsLookingAt(position, 20f))
                {
                    if (!ContainerFeeding.IsRegisteredContainerInstance(m_containerConsumeTarget))
                    {
                        m_containerConsumeTarget = null;
                        return false;
                    }

                    Inventory inventory = m_containerConsumeTarget.GetInventory();
                    if (inventory.RemoveOneItem(item))
                    {
                        Runtime.ItemConsumeContext.SetExternalFeedItem(item);
                        if (m_monsterAI)
                        {
                            m_monsterAI.m_onConsumedItem?.Invoke(null);
                        }
                        else if (m_animalAITrait)
                        {
                            m_animalAITrait.m_onConsumedItem?.Invoke(null);
                        }

                        GetComponent<ZSyncAnimation>().SetTrigger("consume");
                        m_containerConsumeTarget = null;
                        Runtime.ItemConsumeContext.Clear();
                    }
                }
            }

            return true;
        }

        public static bool CanConsume(IReadOnlyList<ConsumeItem> consumeList, ItemDrop.ItemData data)
        {
            if (consumeList == null || data == null)
            {
                return false;
            }
            string itemName = data.m_shared.m_name;
            foreach (ConsumeItem consumeItem in consumeList)
            {
                if (consumeItem.itemDrop.m_itemData.m_shared.m_name == itemName)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool CanConsume(IReadOnlyList<ItemDrop> consumeList, ItemDrop item)
        {
            var data = item.m_itemData;
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

                float dist = (item.transform.position - pos).sqrMagnitude;
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

        /*
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
        */

    }
}
