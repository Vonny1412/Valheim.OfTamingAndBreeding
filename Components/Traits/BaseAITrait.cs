using OfTamingAndBreeding.Components.Core;
using OfTamingAndBreeding.Components.Extensions;
using OfTamingAndBreeding.Utilities;
using System;
using System.Linq;
using UnityEngine;
using YamlDotNet.Core.Tokens;

namespace OfTamingAndBreeding.Components.Traits
{
    public partial class BaseAITrait : OTABComponent<BaseAITrait>
    {

        [SerializeField] internal bool m_tamedStayNearSpawn = false;
        [SerializeField] internal float m_idleSoundChanceWhenTamed = -1f;

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
            
            s_consumeItemsStore.TryGet(m_consumeItemsStoreIndex, out m_consumeItems);

            Register(this);
        }

        private void Start()
        {
            if (!CanBecomeConfined())
            {
                if (m_nview.IsValid())
                {
                    ResetAntiExploitState();
                }
            }
        }








        private void OnDestroy()
        {
            Unregister(this);
        }

        internal bool OnUpdateAI(float dt)
        {
            m_characterTrait.UpdateHostilities();

            UpdateConfinedHud();

            if (m_animalAITrait && m_animalAITrait.OnUpdateAI(dt))
            {
                // Update monsterAI-like stuff
                return true;
            }

            return false;
        }

        internal bool OnIdleMovement(float dt)
        {
            if (m_consumeClip && m_consumeClip.IsPlaying())
            {
                // eating - do not disturb!
                return true;
            }

            if (UpdateAntiExploit(dt))
            {
                return true;
            }

            if (m_animalAITrait && m_animalAITrait.OnIdleMovement(dt))
            {
                // used for animals that can consume food or follow the player
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

        public bool TamedStayNearSpawn()
        {
            return m_tamedStayNearSpawn;
        }

        public GameObject GetFollowTarget()
        {
            if (m_animalAITrait)
            {
                return m_animalAITrait.GetFollowTarget();
            }
            return m_monsterAI.GetFollowTarget();
        }

        public string GetAdminHoverInfoText()
        {
            if (!m_nview.IsValid())
            {
                return "";
            }

            var text = "";

            var movedDist = (float)(int)(GetAntiExploitMovedDistance() * 10) / 10;
            var movedParts = string.Join(" + ", m_antiExploitMoveDistances.Select((d) => (float)(int)(d * 10) / 10));
            var movedText = $"Moved: {movedParts} = {movedDist} / {GetMinRequiredMoveDistance()}";
            var jammedText = $"Confined: " + (m_confined ? "true" : "false") + (m_checkConfinement ? " (avoiding)" : "");
            text += "\n" + Localization.instance.Localize("$otab_hover_admin_info", jammedText);
            text += "\n" + Localization.instance.Localize("$otab_hover_admin_info", movedText);

            return text;
        }

    }
}
