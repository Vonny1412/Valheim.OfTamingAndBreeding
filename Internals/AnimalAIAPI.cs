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
        public static bool TryGet(AnimalAI __instance, out AnimalAIAPI api)
            => instances.TryGetValue(__instance, out api);

        public AnimalAIAPI(AnimalAI __instance) : base(__instance)
        {
            _baseAI = (BaseAI)__instance;
        }

        #region MonsterAI-stuff to make animals tameable

        private readonly BaseAI _baseAI;

        public List<ItemDrop> m_consumeItems;

        public float m_consumeRange = 2f;

        public float m_consumeSearchRange = 5f;

        public float m_consumeSearchInterval = 10f;

        private ItemDrop m_consumeTarget;

        private float m_consumeSearchTimer;

        public Action<ItemDrop> m_onConsumedItem;

        public bool UpdateConsumeAI(float dt)
        {
            if (UpdateConsumeItem(dt))
            {
                return false; // disable original method
            }
            return true; // run original method
        }

        private bool UpdateConsumeItem(float dt)
        {
            if (IsAlerted() || m_consumeItems == null || m_consumeItems.Count == 0)
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
                    m_consumeTarget = Behaviors.ConsumeBehavior.FindNearbyConsumableItem(_baseAI, m_consumeSearchRange, CanConsume);
                }
                else
                {
                    m_consumeTarget = Behaviors.ConsumeBehavior.FindClosestConsumableItem(_baseAI, m_consumeSearchRange, CanConsume);
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
