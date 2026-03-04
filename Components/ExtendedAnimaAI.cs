using OfTamingAndBreeding.Components.Base;
using OfTamingAndBreeding.Components.Extensions;
using OfTamingAndBreeding.Components.Traits;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OfTamingAndBreeding.Components
{
    public class ExtendedAnimaAI : OTABComponent<ExtendedAnimaAI>
    {

        // set by registry processor
        [SerializeField] public List<ItemDrop> m_consumeItems = null;
        [SerializeField] public float m_consumeRange = 2f;
        [SerializeField] public float m_consumeSearchRange = 5f;
        [SerializeField] public float m_consumeSearchInterval = 10f;

        // used by TameableTrait
        [NonSerialized] public Action<ItemDrop> m_onConsumedItem = null;

        // set in awake
        [NonSerialized] private ZNetView m_nview = null;
        [NonSerialized] private AnimalAI m_animalAI = null;
        [NonSerialized] private Tameable m_tameable = null;
        [NonSerialized] private Character m_character = null;
        [NonSerialized] private BaseAITrait m_baseAITrait = null;
        [NonSerialized] private ZSyncAnimation m_animator = null;
        [NonSerialized] private ItemDrop m_consumeTarget = null;
        [NonSerialized] private float m_consumeSearchTimer = 0;
        [NonSerialized] private GameObject m_follow = null;

        private void Awake()
        {
            m_nview = GetComponent<ZNetView>();
            m_animalAI = GetComponent<AnimalAI>();
            m_tameable = GetComponent<Tameable>();
            m_character = GetComponent<Character>();
            m_baseAITrait = GetComponent<BaseAITrait>();
            m_animator = GetComponent<ZSyncAnimation>();
        }
        
        public bool IsAlerted()
        {
            return m_animalAI.IsAlerted();
        }

        public void SetPatrolPoint()
        {
            m_animalAI.SetPatrolPoint();
        }

        public void ResetPatrolPoint()
        {
            m_animalAI.ResetPatrolPoint();
        }

        public bool IdleMovement(float dt)
        {
            if (!m_nview.IsValid() || !m_nview.IsOwner())
            {
                return false;
            }

            if (UpdateConsumeItem(dt)) return true;
            if (UpdateFollowTarget(dt)) return true;
  
            return false;
        }

        public bool UpdateConsumeItem(float dt)
        {
            if (m_animalAI.IsAlerted() || m_consumeItems == null || m_consumeItems.Count == 0)
            {
                return false;
            }

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
                    m_consumeTarget = m_baseAITrait.FindNearbyConsumableItem(m_consumeSearchRange, m_consumeItems);
                }
                else
                {
                    m_consumeTarget = m_baseAITrait.FindClosestConsumableItem(m_consumeSearchRange, m_consumeItems);
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

                        m_animator.SetTrigger("consume");
                        m_consumeTarget = null;
                    }
                }

                return true;
            }

            return false;
        }

        public bool UpdateFollowTarget(float dt)
        {
            if (m_animalAI.IsAlerted() || m_follow == null)
            {
                return false;
            }
            m_animalAI.Follow(m_follow, dt);
            return true;
        }

        public void MakeTame()
        {
            m_character.SetTamed(tamed: true);
            m_animalAI.SetAlerted(alert: false);
        }

        public GameObject GetFollowTarget()
        {
            return m_follow;
        }

        public void SetFollowTarget(GameObject go)
        {
            m_follow = go;
        }

    }
}
