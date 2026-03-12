using OfTamingAndBreeding.Components.Base;
using OfTamingAndBreeding.Components.Extensions;
using OfTamingAndBreeding.OTABUtils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OfTamingAndBreeding.Components.Traits
{
    public class BaseAITrait : OTABComponent<BaseAITrait>
    {
        public class ConsumeItem
        {
            internal ItemDrop itemDrop;
            public float fedDurationFactor;
        }

        static BaseAITrait()
        {
            _consumeItemData = new List<ConsumeItem[]>();

            Net.NetworkSessionManager.Instance.OnClosed((dataLoaded) => {
                _consumeItemData.Clear();
            });
        }

        private static readonly List<ConsumeItem[]> _consumeItemData;

        // set in awake
        [NonSerialized] private ZNetView m_nview = null;
        [NonSerialized] private BaseAI m_baseAI = null;
        [NonSerialized] private MonsterAI m_monsterAI = null;
        [NonSerialized] private TameableTrait m_tameableTrait = null;
        [NonSerialized] private AnimalAITrait m_animalAITrait = null;
        [NonSerialized] private Character m_character = null;
        [NonSerialized] private ConsumeAnimationClipOverlay m_consumeClip = null;

        // set in registration
        [SerializeField] public bool m_tamedStayNearSpawn = false;
        [SerializeField] private int m_consumeItemDataIndex = -1;

        private void Awake()
        {
            m_nview = GetComponent<ZNetView>();
            m_baseAI = GetComponent<BaseAI>();
            m_monsterAI = GetComponent<MonsterAI>();
            m_tameableTrait = GetComponent<TameableTrait>();
            m_animalAITrait = GetComponent<AnimalAITrait>();
            m_character = GetComponent<Character>();
            m_consumeClip = GetComponent<ConsumeAnimationClipOverlay>();
        }

        public void SetCustomConsumeItems(ConsumeItem[] consumeItems)
        {
            m_consumeItemDataIndex = _consumeItemData.Count;
            _consumeItemData.Add(consumeItems);
        }

        public bool HasCustomConsumeItems(out ConsumeItem[] consumeItems)
        {
            if (m_consumeItemDataIndex != -1)
            {
                consumeItems = _consumeItemData[m_consumeItemDataIndex];
                return true;
            }
            consumeItems = null;
            return false;
        }

        private static readonly int m_itemMask = LayerMask.GetMask("item");
        private static readonly Collider[] colliders = new Collider[256]; // 256 should be enough for now

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
                if (StaticContext.SpecialPrefabContext.IsSpecialPrefab(consumeItem.gameObject.name))
                {
                    if (consumeItem.TryGetComponent<StaticContext.SpecialPrefabs.OTABSpecialConsumableItem>(out var comparer))
                    {
                        if (comparer.Compare(checkItem) == true)
                        {
                            return true;
                        }
                    }
                }
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

            int count = Physics.OverlapSphereNonAlloc(pos, maxRange, colliders, m_itemMask);
            if (count <= 0)
                return null;

            ItemDrop chosen = null;
            float bestDist = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                Collider col = colliders[i];
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

        public bool UpdateStarvingMonsterAI(float dt)
        {
            // this is preventing monster-creatures from beeing stuck in aggression
            // if ppl dont like this behaviour they shall make sure their tames are fed!
            var monsterAI = m_monsterAI;
            var tameableTrait = m_tameableTrait;
            if (monsterAI && monsterAI.IsAlerted() && tameableTrait && tameableTrait.IsTamed() && tameableTrait.IsStarving())
            {
                bool isInCombatWithTarget = monsterAI.GetTargetCreature() != null || monsterAI.GetStaticTarget() != null;
                if (isInCombatWithTarget)
                {
                    if (monsterAI.UpdateConsumeItem((Humanoid)m_character, dt))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void UpdateInteractableAI()
        {
            var baseAI = m_baseAI;
            var monsterAI = m_monsterAI;
            var tameableTrait = m_tameableTrait;
            var animalAITrait = m_animalAITrait;
            if (tameableTrait && tameableTrait.IsTamed())
            {
                if (monsterAI)
                {
                    var isCommanded = monsterAI.GetFollowTarget() != null || baseAI.GetPatrolPoint(out _);
                    if (isCommanded && tameableTrait.IsInteractable() == false)
                    {
                        monsterAI.SetFollowTarget(null);
                        baseAI.ResetPatrolPoint();
                    }
                }
                else if (animalAITrait)
                {
                    var isCommanded = animalAITrait.GetFollowTarget() != null || baseAI.GetPatrolPoint(out _);
                    if (isCommanded && tameableTrait.IsInteractable() == false)
                    {
                        animalAITrait.SetFollowTarget(null);
                        baseAI.ResetPatrolPoint();
                    }
                }
            }
        }

        public bool IdleMovement(float dt)
        {
            if (m_consumeClip && m_consumeClip.IsPlaying())
            {
                // creature is eating - do not disturb!
                return true;
            }

            if (m_animalAITrait && m_animalAITrait.IdleMovement(dt))
            {
                // used for animals that can consume food or follow the player
                // that stuff comes always before idle movement
                return true;
            }

            /* // original:
            protected void IdleMovement(float dt)
            {
                Vector3 centerPoint = ((m_character.IsTamed() || HuntPlayer()) ? base.transform.position : m_spawnPoint);
                if (GetPatrolPoint(out var point))
                {
                    centerPoint = point;
                }

                RandomMovement(dt, centerPoint, snapToGround: true);
            }
            */

            if (m_tameableTrait && m_tameableTrait.IsTamed())
            {

                if (m_tamedStayNearSpawn == true)
                {
                    if (m_baseAI.GetPatrolPoint(out _))
                    {
                        // is commandable and currently not following
                        // => bound to partol point
                        return false; // let val do the job
                    }
                    var currentPoint = m_baseAI.transform.position;
                    var spawnPoint = m_baseAI.GetSpawnPoint();
                    var inRange = MathUtils.InRangeXZ(currentPoint, spawnPoint, m_baseAI.m_randomMoveRange * 10); // todo: factor 10 needs to be tested
                    if (inRange)
                    {
                        m_baseAI.RandomMovement(dt, spawnPoint, snapToGround: true);
                        return true;
                    }
                }
            }

            return false; // not handled
        }


    }
}
