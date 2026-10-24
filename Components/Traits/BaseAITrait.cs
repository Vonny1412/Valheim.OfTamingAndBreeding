using OfTamingAndBreeding.Components.Core;
using OfTamingAndBreeding.Components.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OfTamingAndBreeding.Components.Traits
{
    public partial class BaseAITrait : OTABComponent<BaseAITrait>
    {

        [SerializeField] internal bool m_tamedIdleNearSpawn = false;
        [SerializeField] internal bool m_consumeRandomItem = false;

        [NonSerialized] private ZNetView m_nview = null;
        [NonSerialized] private BaseAI m_baseAI = null;
        [NonSerialized] private MonsterAI m_monsterAI = null;
        [NonSerialized] private ProcreationTrait m_procreationTrait = null;
        [NonSerialized] private TameableTrait m_tameableTrait = null;
        [NonSerialized] private AnimalAITrait m_animalAITrait = null;
        [NonSerialized] private CharacterTrait m_characterTrait = null;
        [NonSerialized] private AnimationClipOverlay m_consumeClip = null;




        private void Awake()
        {
            m_nview = GetComponent<ZNetView>();
            m_baseAI = GetComponent<BaseAI>();
            m_monsterAI = GetComponent<MonsterAI>();
            m_procreationTrait = GetComponent<ProcreationTrait>();
            m_tameableTrait = GetComponent<TameableTrait>();
            m_animalAITrait = GetComponent<AnimalAITrait>();
            m_characterTrait = GetComponent<CharacterTrait>();
            m_consumeClip = GetComponent<AnimationClipOverlay>();

            // randomize for more immersion
            m_consumeSearchTimer = UnityEngine.Random.Range(0, GetConsumeSearchInterval());

            Register();
        }

        private void Start()
        {
            s_consumeItemsStore.TryGet(m_consumeItemsStoreIndex, out m_consumeItems);

            if (m_consumeRandomItem && m_consumeItems != null && m_consumeItems.Length > 1)
            {
                var zdo = m_nview.GetZDO();
                long spawnTime = zdo.GetLong(ZDOVars.s_spawnTime, 0L);
                if (spawnTime != 0L)
                {
                    uint hash = unchecked((uint)(spawnTime ^ (spawnTime >> 32)));
                    int index = (int)(hash % (uint)m_consumeItems.Length);
                    m_consumeItems = new[] { m_consumeItems[index] };
                    if (m_monsterAI)
                    {
                        m_monsterAI.m_consumeItems = new List<ItemDrop> { m_consumeItems[0].itemDrop };
                    }
                    else if (m_animalAITrait)
                    {
                        m_animalAITrait.m_consumeItems = new List<ItemDrop> { m_consumeItems[0].itemDrop };
                    }
                }
            }
        }

        private void OnDestroy()
        {
            Unregister();
        }

        public BaseAI GetBaseAI()
        {
            return m_baseAI;
        }










        internal bool On_UpdateAI(float dt)
        {
            if (m_tameableTrait)
            {
                m_tameableTrait.On_UpdateAI(dt);
            }

            // UpdateReactiveHostilities() is only getting called here, mask default is None
            // so even right after awake a change would go from None to Attack and abort following
            var tamedVersusPlayer1 = m_characterTrait.TamedVersusPlayerMask;
            m_characterTrait.UpdateReactiveHostilities();
            var tamedVersusPlayer2 = m_characterTrait.TamedVersusPlayerMask;

            if (tamedVersusPlayer1 != tamedVersusPlayer2 && tamedVersusPlayer2 == CharacterTrait.HostilityMask.Attack)
            {
                if (m_animalAITrait)
                {
                    if (m_animalAITrait.GetFollowTarget())
                    {
                        m_animalAITrait.SetFollowTarget(null);
                        m_animalAITrait.SetPatrolPoint();
                    }
                }
                else
                {
                    if (m_monsterAI.GetFollowTarget())
                    {
                        m_monsterAI.SetFollowTarget(null);
                        m_monsterAI.SetPatrolPoint();
                    }
                }
            }

            UpdateFrustratedHud();

            if (m_animalAITrait && m_animalAITrait.On_UpdateAI(dt))
            {
                // Update monsterAI-like stuff
                return true;
            }

            return false;
        }

        internal bool On_IdleMovement(float dt)
        {
            if (m_consumeClip && m_consumeClip.IsPlaying())
            {
                // eating - do not disturb!
                return true;
            }

            if (m_animalAITrait && m_animalAITrait.On_IdleMovement(dt))
            {
                // used for animals that can consume food or follow the player
                return true;
            }
            
            return false; // not handled
        }

        public bool IsAlerted()
        {
            return m_baseAI.IsAlerted();
        }

        public void StopPlayerHunt()
        {
            if (m_baseAI && m_baseAI.HuntPlayer())
            {
                m_baseAI.SetHuntPlayer(hunt: false);
                m_baseAI.SetAlerted(alerted: false);
            }
        }

        public void SetSpawnPoint()
        {
            var point = transform.position;
            m_baseAI.SetSpawnPoint(point);
            if (m_nview.IsValid() && m_nview.IsOwner())
            {
                m_nview.GetZDO().Set(ZDOVars.s_spawnPoint, point);
            }
        }

        public GameObject GetFollowTarget()
        {
            if (m_animalAITrait)
            {
                return m_animalAITrait.GetFollowTarget();
            }
            return m_monsterAI.GetFollowTarget();
        }



    }
}
