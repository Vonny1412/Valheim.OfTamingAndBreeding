using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Components.Extensions
{
    internal static class MonsterAIExtensions
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ZNetView GetZNetView(this MonsterAI that)
            => ValheimAPI.MonsterAI.__IAPI_m_nview_Invoker.Get(that);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool UpdateConsumeItem(this MonsterAI that, Humanoid humanoid, float dt)
            => ValheimAPI.MonsterAI.__IAPI_UpdateConsumeItem_Invoker1.Invoke(that, humanoid, dt);

        public static bool UpdateAI_PatchPrefix(this MonsterAI monsterAI, float dt)
        {

            //
            // BASE AI (1) START
            //

            var m_nview = monsterAI.GetZNetView();
            if (!m_nview.IsValid())
            {
                return false;
            }
            if (!m_nview.IsOwner())
            {
                monsterAI.SetAlertedUnsafe(m_nview.GetZDO().GetBool(ZDOVars.s_alert));
                return false;
            }

            //
            // BASE AI (1) END
            //

            var tameable = monsterAI.GetComponent<Tameable>();
            if (tameable)
            {
                if (tameable.IsStarving())
                {

                    /* original part:
                        if ((!IsAlerted() || (m_targetStatic == null && m_targetCreature == null)) && UpdateConsumeItem(humanoid, dt))
                        {
                            return true;
                        }
                    */

                    bool isInCombatWithTarget = monsterAI.IsAlerted() && (monsterAI.GetTargetCreature() != null || monsterAI.GetStaticTarget() != null);
                    if (isInCombatWithTarget)
                    {
                        Humanoid humanoid = monsterAI.GetComponent<Humanoid>();
                        if (monsterAI.UpdateConsumeItem(humanoid, dt))
                        {

                            //
                            // BASE AI (2) START
                            //

                            monsterAI.UpdateTakeoffLanding(dt);
                            if (monsterAI.m_jumpInterval > 0f)
                            {
                                monsterAI.SetJumpTimer(monsterAI.GetJumpTimer() + dt);
                            }

                            float randomMoveTimer = monsterAI.GetRandomMoveUpdateTimer();
                            if (randomMoveTimer > 0f)
                            {
                                monsterAI.SetRandomMoveUpdateTimer(randomMoveTimer - dt);
                            }

                            monsterAI.UpdateRegeneration(dt);
                            monsterAI.SetTimeSinceHurt(monsterAI.GetTimeSinceHurt() + dt);

                            //
                            // BASE AI (2) END
                            //

                            return false;
                        }
                    }
                }
            }
            return true;
        }

    }
}
