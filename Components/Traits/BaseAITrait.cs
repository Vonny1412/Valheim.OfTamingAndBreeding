using OfTamingAndBreeding.Components.Base;
using OfTamingAndBreeding.Components.Extensions;
using OfTamingAndBreeding.Components.SpecialPrefabs;
using OfTamingAndBreeding.OTABUtils;
using OfTamingAndBreeding.ValheimAPI;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YamlDotNet.Core;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

namespace OfTamingAndBreeding.Components.Traits
{
    public class BaseAITrait : OTABComponent<BaseAITrait>
    {
        public class ConsumeItem
        {
            internal ItemDrop itemDrop;
            public float fedDurationFactor;
        }

        private static readonly int m_itemMask = LayerMask.GetMask("item");
        private static readonly Collider[] colliders = new Collider[64];
        private static readonly List<ConsumeItem[]> _consumeItemData;

        static BaseAITrait()
        {
            _consumeItemData = new List<ConsumeItem[]>();

            Net.NetworkSessionManager.Instance.OnSessionClosed += (netsess, dataLoaded) => {
                _consumeItemData.Clear();
            };
        }

        // set in awake
        [NonSerialized] private ZNetView m_nview = null;
        [NonSerialized] private BaseAI m_baseAI = null;
        [NonSerialized] private MonsterAI m_monsterAI = null;
        [NonSerialized] private TameableTrait m_tameableTrait = null;
        [NonSerialized] private AnimalAITrait m_animalAITrait = null;
        [NonSerialized] private CharacterTrait m_characterTrait = null;
        [NonSerialized] private AnimationClipOverlay m_consumeClip = null;

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
            m_characterTrait = GetComponent<CharacterTrait>();
            m_consumeClip = GetComponent<AnimationClipOverlay>();

            m_lastPosition = null;

            Register(this);
        }

        private void OnDestroy()
        {
            Unregister(this);
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
                if (OTABSpecialConsumeAnyItem.TryGet(consumeItem.gameObject, out var component))
                {
                    return component.Compare(checkItem);
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

                // valheim is using GetComponent x2 per loop
                // using ItemDropTrait it can be atleast reduced to 1x
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

                // valheim is using GetComponent x2 per loop
                // using ItemDropTrait it can be atleast reduced to 1x
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

        public bool UpdateAI(float dt)
        {
            m_characterTrait.UpdateHostilities();
            UpdateCommandableAI();

            if (m_animalAITrait && m_animalAITrait.UpdateAI(dt))
            {
                return true;
            }

            if (UpdateStarvingMonsterAI(dt))
            {
                return true;
            }

            return false;
        }

        private bool UpdateStarvingMonsterAI(float dt)
        {
            if (!m_characterTrait.IsTamed() || !m_tameableTrait || !m_tameableTrait.IsStarving())
            {
                return false;
            }

            // this is preventing monster-creatures from beeing stuck in aggression
            // if ppl dont like this behaviour they shall make sure their tames are fed!
            var monsterAI = m_monsterAI;
            if (monsterAI && monsterAI.IsAlerted())
            {
                bool isInCombatWithTarget = monsterAI.GetTargetCreature() != null || monsterAI.GetStaticTarget() != null;
                if (isInCombatWithTarget)
                {
                    if (monsterAI.UpdateConsumeItem((Humanoid)m_characterTrait.GetCharacter(), dt))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void UpdateCommandableAI()
        {
            var tameableTrait = m_tameableTrait;
            if (tameableTrait && tameableTrait.IsCommandable() && tameableTrait.IsStarving())
            {
                var baseAI = m_baseAI;
                var monsterAI = m_monsterAI;
                var animalAITrait = m_animalAITrait;
                if (monsterAI)
                {
                    if ((bool)monsterAI.GetFollowTarget())
                    {
                        monsterAI.SetFollowTarget(null);
                        m_baseAI.SetPatrolPoint();
                    }
                }
                else if (animalAITrait)
                {
                    if ((bool)animalAITrait.GetFollowTarget())
                    {
                        animalAITrait.SetFollowTarget(null);
                        m_baseAI.SetPatrolPoint();
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

            IdleMovementAntiJam(dt);

            if (m_animalAITrait && m_animalAITrait.IdleMovement(dt))
            {
                // used for animals that can consume food or follow the player
                // that stuff comes always before idle movement
                return true;
            }

            if (RandomMovementNearSpawn(dt))
            {
                return true;
            }

            return false; // not handled
        }

        private bool RandomMovementNearSpawn(float dt)
        {
            if (!m_tamedStayNearSpawn)
            {
                return false;
            }

            if (m_baseAI.GetRandomMoveUpdateTimer() > 0)
            {
                return false;
            }

            if (!m_characterTrait.IsTamed())
            {
                return false;
            }

            if (m_baseAI.GetPatrolPoint(out _))
            {
                return false;
            }

            var currentPoint = m_baseAI.transform.position;
            var spawnPoint = m_baseAI.GetSpawnPoint();
            var inRange = MathUtils.InRangeXZ(currentPoint, spawnPoint, m_baseAI.m_randomMoveRange * 10); // todo: factor 10 needs to be tested
            if (inRange)
            {
                m_baseAI.RandomMovement(dt, spawnPoint, snapToGround: false);
                return true;
            }

            return false;
        }


        public bool IsAlerted()
        {
            return m_baseAI.IsAlerted();
        }

        public void StopPlayerHunt()
        {
            // i dunno if this is neccessary
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








        [NonSerialized] private float m_checkRandomMoveTimer = 0;
        [NonSerialized] private const int m_idleMoveCheckCount = 4; // todo: add config option for interval-count
        [NonSerialized] private Vector3? m_lastPosition;
        [NonSerialized] private List<float> m_movedList = new List<float>();
        [NonSerialized] private bool m_isJammed = false;
        [NonSerialized] private float m_totalMovedDistance = 0f;

        public bool IsJammed()
        {
            return m_isJammed;
        }

        public bool CheckIsJammed()
        {
            return m_totalMovedDistance < GetMinRequiredMoveRange();
        }

        private float GetMinRequiredMoveRange()
        {
            return m_baseAI.m_randomMoveRange;
        }

        private bool CanBecomeJammed()
        {
            if (!Plugin.IsServerDataLoaded())
            {
                return false;
            }
            if (Plugin.Configs.EnableAntiJammingSystem.Value == false)
            {
                return false;
            }
            //if (!m_tameableTrait || !(m_tameableTrait.IsTamed() || m_tameableTrait.IsTamingStarted()))
            if (!m_characterTrait.IsTamed())
            {
                return false;
            }
            return true;
        }

        private static readonly EffectList m_emptyEffect = new EffectList();

        public void AlertSilent()
        {
            if (m_baseAI.IsAlerted())
            {
                return;
            }
            var eff = m_baseAI.m_alertedEffects;
            m_baseAI.m_alertedEffects = m_emptyEffect;
            try
            {
                m_baseAI.SetAlerted(alerted: true);
            }
            finally
            {
                m_baseAI.m_alertedEffects = eff;
            }
        }

        private void IdleMovementAntiJam(float dt)
        {
            if (!CanBecomeJammed())
            {
                m_isJammed = false;
                return;
            }

            m_checkRandomMoveTimer -= dt;
            if (m_checkRandomMoveTimer <= 0)
            {
                m_checkRandomMoveTimer = m_baseAI.m_randomMoveInterval;

                var curPosition = transform.position;
                if (m_lastPosition.HasValue)
                {
                    float curDist = Vector3.Distance(m_lastPosition.Value, curPosition);
                    m_movedList.Add(curDist);
                }
                m_lastPosition = curPosition;
                while (m_movedList.Count > m_idleMoveCheckCount)
                {
                    m_movedList.RemoveAt(0);
                }
                m_totalMovedDistance = 0f;
                foreach (var v in m_movedList)
                {
                    m_totalMovedDistance += v;
                }
                if (m_movedList.Count == m_idleMoveCheckCount)
                {
                    var wasJammed = m_isJammed;
                    m_isJammed = CheckIsJammed();
                    if (m_isJammed)
                    {
                        AlertSilent();
                    }
                    else if (wasJammed)
                    {
                        m_baseAI.SetAlerted(alerted: false);
                    }
                }
            }
        }
        
        public string GetAdminHoverInfoText()
        {
            if (!m_nview.IsValid())
            {
                return "";
            }

            var text = "";

            var movedDist = (float)(int)(m_totalMovedDistance * 10) / 10;
            var movedParts = string.Join(" + ", m_movedList.Select((d) => (float)(int)(d * 10) / 10));
            var movedText = $"Moved: {movedParts} = {movedDist} / {GetMinRequiredMoveRange()}";
            text += "\n" + Localization.instance.Localize("$otab_hover_admin_info", movedText);

            return text;
        }









    }
}
