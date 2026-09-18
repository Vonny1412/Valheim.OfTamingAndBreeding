using OfTamingAndBreeding.Components.Core;
using OfTamingAndBreeding.Components.Extensions;
using OfTamingAndBreeding.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using YamlDotNet.Core.Tokens;

namespace OfTamingAndBreeding.Components.Traits
{
    public class BaseAITrait : OTABComponent<BaseAITrait>
    {

        // prefab values
        [SerializeField] public bool m_tamedStayNearSpawn = false;
        [SerializeField] public float m_idleSoundChanceWhenTamed = -1f;
        [SerializeField] private int m_consumeItemDataIndex = -1;












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

            Network.NetworkSessionManager.OnSessionClosed += () => {
                _consumeItemData.Clear();
            };
        }

        // instance values
        [NonSerialized] private ZNetView m_nview = null;
        [NonSerialized] private BaseAI m_baseAI = null;
        [NonSerialized] private TameableTrait m_tameableTrait = null;
        [NonSerialized] private AnimalAITrait m_animalAITrait = null;
        [NonSerialized] private CharacterTrait m_characterTrait = null;
        [NonSerialized] private AnimationClipOverlay m_consumeClip = null;

        private void Awake()
        {
            m_nview = GetComponent<ZNetView>();
            m_baseAI = GetComponent<BaseAI>();
            m_tameableTrait = GetComponent<TameableTrait>();
            m_animalAITrait = GetComponent<AnimalAITrait>();
            m_characterTrait = GetComponent<CharacterTrait>();
            m_consumeClip = GetComponent<AnimationClipOverlay>();

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

            UpdateJammedHud();

            if (m_animalAITrait && m_animalAITrait.UpdateAI(dt))
            {
                return true;
            }

            return false;
        }

        public bool IdleMovement(float dt)
        {
            if (m_consumeClip && m_consumeClip.IsPlaying())
            {
                // creature is eating - do not disturb!
                return true;
            }

            if (IdleMovementAntiJam(dt))
            {
                return true;
            }

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
        [NonSerialized] private const int m_idleMoveCheckCount = 2;
        [NonSerialized] private const float m_idleMoveCheckDistMul = 0.33333f;
        [NonSerialized] private Vector3? m_lastPosition = null;
        [NonSerialized] private List<float> m_movedList = new List<float>();
        [NonSerialized] private bool m_avoidJam = false;
        [NonSerialized] private Vector3 m_jamFleeTarget;
        [NonSerialized] private GameObject m_jammedHud;
        [NonSerialized] private GameObject m_awareHud;
        [NonSerialized] private bool m_jammed = false;

        public void SetJammedHud(GameObject jammedHud, GameObject awareHud)
        {
            m_jammedHud = jammedHud;
            m_awareHud = awareHud;
        }

        private void UpdateJammedHud()
        {
            m_jammed = m_nview.GetZDO().GetInt(Plugin.ZDOVars.z_jammed, 0) == 1;
            if (!m_jammedHud)
            {
                return;
            }

            //bool flag = m_baseAI.HaveTarget();
            bool flag2 = m_baseAI.IsAlerted();
            if (m_jammed && !flag2)
            {
                m_awareHud.SetActive(false);
                m_jammedHud.SetActive(true);
            }
            else
            {
                m_awareHud.SetActive(true);
                m_jammedHud.SetActive(false);
            }
        }






        public void ResetAntiJam()
        {
            // this is egtting called on config change

            m_checkRandomMoveTimer = 0f;
            m_lastPosition = null;
            m_movedList.Clear();
            m_avoidJam = false;
            if (m_nview.IsValid())
            {
                ZDOUtils.SetInt(m_nview.GetZDO(), Plugin.ZDOVars.z_jammed, 0);
            }
        }

        private float GetTotalMovedDistance()
        {
            return m_movedList.Sum();
        }

        public bool IsJammed()
        {
            return m_jammed;
        }
        
        private float GetMinRequiredMoveRange()
        {
            return m_baseAI.m_randomMoveRange * m_idleMoveCheckDistMul;
        }
        
        private bool CanBecomeJammed()
        {
            if (!Plugin.IsOTABMode())
            {
                return false;
            }
            if (Plugin.Configs.EnableAntiJammingSystem.Value == false)
            {
                return false;
            }
            if (!(m_characterTrait.IsTamed() || (m_tameableTrait && m_tameableTrait.IsTamingStarted())))
            {
                return false;
            }
            return true;
        }




        private bool IdleMovementAntiJam(float dt)
        {
            if (!CanBecomeJammed())
            {
                ResetAntiJam();
                return false;
            }

            if (!m_nview.IsValid())
            {
                return false;
            }
            var zdo = m_nview.GetZDO();






            m_checkRandomMoveTimer -= dt;
            if (m_checkRandomMoveTimer <= 0f)
            {
                m_checkRandomMoveTimer = m_baseAI.m_randomMoveInterval;

                var currentPosition = transform.position;

                if (m_lastPosition.HasValue)
                {
                    m_movedList.Add(Vector3.Distance(m_lastPosition.Value, currentPosition));
                }

                m_lastPosition = currentPosition;

                while (m_movedList.Count > m_idleMoveCheckCount)
                {
                    m_movedList.RemoveAt(0);
                }

                if (m_movedList.Count == m_idleMoveCheckCount)
                {
                    var movedEnough = GetTotalMovedDistance() >= GetMinRequiredMoveRange();

                    if (movedEnough)
                    {
                        m_avoidJam = false;
                        ZDOUtils.SetInt(zdo, Plugin.ZDOVars.z_jammed, 0);
                    }
                    else
                    {
                        if (!m_avoidJam)
                        {
                            m_avoidJam = true;
                            m_jamFleeTarget = transform.position;
                        }
                        else
                        {
                            ZDOUtils.SetInt(zdo, Plugin.ZDOVars.z_jammed, 1);
                        }
                    }
                }
            }

            if (m_avoidJam && !IsAlerted())
            {
                m_baseAI.Flee(dt, m_jamFleeTarget);
                return true;
            }
            return false;
        }

        public string GetAdminHoverInfoText()
        {
            if (!m_nview.IsValid())
            {
                return "";
            }

            var text = "";

            var movedDist = (float)(int)(GetTotalMovedDistance() * 10) / 10;
            var movedParts = string.Join(" + ", m_movedList.Select((d) => (float)(int)(d * 10) / 10));
            var movedText = $"Moved: {movedParts} = {movedDist} / {GetMinRequiredMoveRange()}";
            var jammedText = $"Jammed: " + (m_jammed ? "true" : "false");
            text += "\n" + Localization.instance.Localize("$otab_hover_admin_info", jammedText);
            text += "\n" + Localization.instance.Localize("$otab_hover_admin_info", movedText);

            return text;
        }















    }
}
