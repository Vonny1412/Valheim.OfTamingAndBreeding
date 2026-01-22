using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OfTamingAndBreeding.Internals
{
    internal class AnimalAIAPI : API.AnimalAI
    {

        private static readonly ConditionalWeakTable<AnimalAI, AnimalAIAPI> instances
            = new ConditionalWeakTable<AnimalAI, AnimalAIAPI>();
        public static AnimalAIAPI GetOrCreate(AnimalAI __instance)
            => instances.GetValue(__instance, (AnimalAI inst) => new AnimalAIAPI(inst));
        public static bool TryGetAPI(AnimalAI __instance, out AnimalAIAPI api)
            => instances.TryGetValue(__instance, out api);

        private readonly BaseAI _baseAI;

        public AnimalAIAPI(AnimalAI __instance) : base(__instance)
        {
            _baseAI = (BaseAI)__IAPI_GetInstance();
        }

        #region MonsterAI-stuff to make animals tameable

        public List<ItemDrop> m_consumeItems;

        public float m_consumeRange = 2f;

        public float m_consumeSearchRange = 5f;

        public float m_consumeSearchInterval = 10f;

        private ItemDrop m_consumeTarget;

        private float m_consumeSearchTimer;

        public Action<ItemDrop> m_onConsumedItem;

        public bool UpdateConsumeAI(float dt)
        {
            if (!IsAlerted() && UpdateConsumeItem(dt))
            {
                return true;
            }
            return true;
        }

        private bool UpdateConsumeItem(float dt)
        {
            if (m_consumeItems == null || m_consumeItems.Count == 0)
            {
                return false;
            }

            m_consumeSearchTimer += dt;
            if (m_consumeSearchTimer > m_consumeSearchInterval)
            {
                m_consumeSearchTimer = 0f;
                if ((bool)m_tamable && !m_tamable.IsHungry())
                {
                    return false;
                }

                if (Plugin.Configs.UseBetterSearchForFood.Value == true)
                {
                    m_consumeTarget = Behaviors.ConsumeBehavior.FindClosestConsumableItem(_baseAI, m_consumeSearchRange, CanConsume);
                }
                else
                {
                    m_consumeTarget = FindClosestConsumableItem(m_consumeSearchRange);
                }
            }

            if ((bool)m_consumeTarget)
            {
                if (MoveTo(dt, m_consumeTarget.transform.position, m_consumeRange, run: false))
                {
                    LookAt(m_consumeTarget.transform.position);
                    if (IsLookingAt(m_consumeTarget.transform.position, 20f) && m_consumeTarget.RemoveOne())
                    {
                        m_onConsumedItem?.Invoke(m_consumeTarget);

                        //humanoid.m_consumeItemEffects.Create(base.transform.position, Quaternion.identity);
                        m_animator.SetTrigger("consume");
                        m_consumeTarget = null;
                    }
                }

                return true;
            }

            return false;
        }

        private static int m_itemMask = 0;

        private ItemDrop FindClosestConsumableItem(float maxRange)
        {
            if (m_itemMask == 0)
            {
                m_itemMask = LayerMask.GetMask("item");
            }

            Collider[] array = Physics.OverlapSphere(transform.position, maxRange, m_itemMask);
            ItemDrop itemDrop = null;
            float num = 999999f;
            Collider[] array2 = array;
            foreach (Collider collider in array2)
            {
                if (!collider.attachedRigidbody)
                {
                    continue;
                }

                ItemDrop component = collider.attachedRigidbody.GetComponent<ItemDrop>();
                if (!(component == null) && component.GetComponent<ZNetView>().IsValid() && CanConsume(component.m_itemData))
                {
                    float num2 = Vector3.Distance(component.transform.position, transform.position);
                    if (itemDrop == null || num2 < num)
                    {
                        itemDrop = component;
                        num = num2;
                    }
                }
            }

            if ((bool)itemDrop && HavePath(itemDrop.transform.position))
            {
                return itemDrop;
            }

            return null;
        }
        
        private bool CanConsume(ItemDrop.ItemData item)
        {
            foreach (ItemDrop consumeItem in m_consumeItems)
            {
                if (consumeItem.m_itemData.m_shared.m_name == item.m_shared.m_name)
                {
                    return true;
                }
            }

            return false;
        }

        public void MakeTame()
        {
            m_character.SetTamed(tamed: true);
            SetAlerted(alert: false);
        }

        #endregion

    }
    

}
