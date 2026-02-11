using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace OfTamingAndBreeding.Patches
{
    [HarmonyPatch(typeof(MonsterAI), "UpdateAI")]
    static class MonsterAI_UpdateAI_Patch
    {
        static bool Prefix(MonsterAI __instance, float dt)
        {

            //
            // BASE AI (1) START
            //

            var nview = Internals.API.BaseAI.__IAPI_m_nview_Invoker.Get(__instance);
            if (!nview.IsValid())
            {
                return false;
            }
            if (!nview.IsOwner())
            {
                Internals.API.BaseAI.__IAPI_m_alerted_Invoker.Set(__instance, nview.GetZDO().GetBool(ZDOVars.s_alert));
                return false;
            }

            //
            // BASE AI (1) END
            //

            var tameable = __instance.GetComponent<Tameable>();
            if (tameable && Internals.TameableAPI.TryGet(tameable, out Internals.TameableAPI tAPI))
            {
                if (tAPI.IsStarving())
                {
                    /* original part:
                        if ((!IsAlerted() || (m_targetStatic == null && m_targetCreature == null)) && UpdateConsumeItem(humanoid, dt))
                        {
                            return true;
                        }
                    */
                    bool isInCombatWithTarget = __instance.IsAlerted() && (__instance.GetTargetCreature() != null || __instance.GetStaticTarget() != null);
                    if (isInCombatWithTarget)
                    {
                        Humanoid humanoid = __instance.GetComponent<Humanoid>();
                        if (Internals.API.MonsterAI.__IAPI_UpdateConsumeItem_Invoker1.Invoke(__instance, new object[] { humanoid, dt }))
                        {

                            //
                            // BASE AI (2) START
                            //

                            Internals.API.BaseAI.__IAPI_UpdateTakeoffLanding_Invoker1.Invoke(__instance, new object[] { dt });
                            if (__instance.m_jumpInterval > 0f)
                            {
                                float jumpTimer = Internals.API.BaseAI.__IAPI_m_jumpTimer_Invoker.Get(__instance);
                                jumpTimer += dt;
                                Internals.API.BaseAI.__IAPI_m_jumpTimer_Invoker.Set(__instance, jumpTimer);
                            }

                            float randomMoveTimer = Internals.API.BaseAI.__IAPI_m_randomMoveUpdateTimer_Invoker.Get(__instance);
                            if (randomMoveTimer > 0f)
                            {
                                randomMoveTimer -= dt;
                                Internals.API.BaseAI.__IAPI_m_randomMoveUpdateTimer_Invoker.Set(__instance, randomMoveTimer);
                            }

                            Internals.API.BaseAI.__IAPI_UpdateRegeneration_Invoker1.Invoke(__instance, new object[] { dt });

                            float tsh = Internals.API.BaseAI.__IAPI_m_timeSinceHurt_Invoker.Get(__instance);
                            tsh += dt;
                            Internals.API.BaseAI.__IAPI_m_timeSinceHurt_Invoker.Set(__instance, tsh);

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
