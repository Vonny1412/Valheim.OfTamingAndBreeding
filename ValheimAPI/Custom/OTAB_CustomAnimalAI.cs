using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OfTamingAndBreeding.ValheimAPI.Custom
{
    public sealed class OTAB_CustomAnimalAI : MonoBehaviour
    {
        [SerializeField] internal List<ItemDrop> m_consumeItems = null;
        [SerializeField] internal float m_consumeRange = 2f;
        [SerializeField] internal float m_consumeSearchRange = 5f;
        [SerializeField] internal float m_consumeSearchInterval = 10f;

        [NonSerialized] private AnimalAI m_animalAI = null;
        [NonSerialized] internal ItemDrop m_consumeTarget = null;
        [NonSerialized] internal float m_consumeSearchTimer = 0;
        [NonSerialized] internal Action<ItemDrop> m_onConsumedItem = null;
        [NonSerialized] internal GameObject m_follow = null;

        private void Awake()
        {
            m_animalAI = GetComponent<AnimalAI>();
        }

        internal AnimalAI GetAnimalAI()
        {
            return m_animalAI;
        }

        internal bool UpdateConsumeItem(Humanoid humanoid, float dt)
        {
            if (m_animalAI.IsAlerted() || m_consumeItems == null || m_consumeItems.Count == 0)
            {
                return false;
            }

            var m_tameable = m_animalAI.GetTameable();

            m_consumeSearchTimer += dt;
            if (m_consumeSearchTimer > m_consumeSearchInterval)
            {
                m_consumeSearchTimer = 0f;
                if ((bool)m_tameable && !m_tameable.IsHungry())
                {
                    return false;
                }

                if (Plugin.Configs.UseBetterSearchForFood.Value == true)
                {
                    m_consumeTarget = m_animalAI.FindNearbyConsumableItem(m_consumeSearchRange, m_consumeItems);
                }
                else
                {
                    m_consumeTarget = m_animalAI.FindClosestConsumableItem(m_consumeSearchRange, m_consumeItems);
                }
            }

            if ((bool)m_consumeTarget)
            {
                if (m_animalAI.MoveTo(dt, m_consumeTarget.transform.position, m_consumeRange, run: false))
                {
                    m_animalAI.LookAt(m_consumeTarget.transform.position);
                    if (m_animalAI.IsLookingAt(m_consumeTarget.transform.position, 20f) && m_consumeTarget.RemoveOne())
                    {
                        m_onConsumedItem?.Invoke(m_consumeTarget);

                        m_animalAI.GetZSyncAnimation()?.SetTrigger("consume");
                        m_consumeTarget = null;
                    }
                }

                return true;
            }

            return false;
        }

        internal bool UpdateFollowTarget(float dt)
        {
            if (m_animalAI.IsAlerted() || m_follow == null)
            {
                return false;
            }
            m_animalAI.Follow(m_follow, dt);
            return true;
        }

        internal void MakeTame()
        {
            m_animalAI.GetCharacter()?.SetTamed(tamed: true);
            m_animalAI.SetAlerted(alert: false);
        }

    }
}
