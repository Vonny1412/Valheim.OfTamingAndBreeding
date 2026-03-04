using OfTamingAndBreeding.Components.Base;
using OfTamingAndBreeding.Components.Extensions;
using System;

namespace OfTamingAndBreeding.Components.Traits
{
    public class MonsterAITrait : OTABComponent<MonsterAITrait>
    {

        // set in awake
        [NonSerialized] private ZNetView m_nview = null;
        [NonSerialized] private MonsterAI m_monsterAI = null;
        [NonSerialized] private TameableTrait m_tameableTrait = null;
        [NonSerialized] private Humanoid m_humanoid = null;

        private void Awake()
        {
            m_nview = GetComponent<ZNetView>();
            m_monsterAI = GetComponent<MonsterAI>();
            m_tameableTrait = GetComponent<TameableTrait>();
            m_humanoid = GetComponent<Humanoid>();
        }

        public bool UpdateAI(float dt)
        {

            //
            // BASE AI (1) START
            //

            if (!m_nview.IsValid())
            {
                return true;
            }
            if (!m_nview.IsOwner())
            {
                m_monsterAI.SetAlertedUnsafe(m_nview.GetZDO().GetBool(ZDOVars.s_alert));
                return true;
            }

            //
            // BASE AI (1) END
            //

            if (m_tameableTrait)
            {
                if (m_tameableTrait.IsStarving())
                {

                    /* original part:
                        if ((!IsAlerted() || (m_targetStatic == null && m_targetCreature == null)) && UpdateConsumeItem(humanoid, dt))
                        {
                            return true;
                        }
                    */

                    bool isInCombatWithTarget = m_monsterAI.IsAlerted() && (m_monsterAI.GetTargetCreature() != null || m_monsterAI.GetStaticTarget() != null);
                    if (isInCombatWithTarget)
                    {
                        if (m_monsterAI.UpdateConsumeItem(m_humanoid, dt))
                        {

                            //
                            // BASE AI (2) START
                            //

                            m_monsterAI.UpdateTakeoffLanding(dt);
                            if (m_monsterAI.m_jumpInterval > 0f)
                            {
                                m_monsterAI.SetJumpTimer(m_monsterAI.GetJumpTimer() + dt);
                            }

                            float randomMoveTimer = m_monsterAI.GetRandomMoveUpdateTimer();
                            if (randomMoveTimer > 0f)
                            {
                                m_monsterAI.SetRandomMoveUpdateTimer(randomMoveTimer - dt);
                            }

                            m_monsterAI.UpdateRegeneration(dt);
                            m_monsterAI.SetTimeSinceHurt(m_monsterAI.GetTimeSinceHurt() + dt);

                            //
                            // BASE AI (2) END
                            //

                            return true;
                        }
                    }
                }
            }

            return false;
        }

    }
}
