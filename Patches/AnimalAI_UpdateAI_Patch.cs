using HarmonyLib;
using System;

namespace OfTamingAndBreeding.Patches
{
    [HarmonyPatch(typeof(AnimalAI), "UpdateAI")]
    static class AnimalAI_UpdateAI_Patch
    {
        static bool Prefix(AnimalAI __instance, float dt)
        {
            // check if this animal is an animal that we are handling
            if (Internals.AnimalAIAPI.TryGetAPI(__instance, out Internals.AnimalAIAPI api))
            {
                // custom updateAI to make the animal consume items on the ground
                return api.UpdateConsumeAI(dt);
            }
            return true;
        }
    }

    /** original method
    public override bool UpdateAI(float dt)
    {
        if (!base.UpdateAI(dt))
        {
            return false;
        }

        if (m_afraidOfFire && AvoidFire(dt, null, superAfraid: true))
        {
            return true;
        }

        m_updateTargetTimer -= dt;
        if (m_updateTargetTimer <= 0f)
        {
            m_updateTargetTimer = (Character.IsCharacterInRange(base.transform.position, 32f) ? 2f : 10f);
            Character character = FindEnemy();
            if ((bool)character)
            {
                m_target = character;
            }
        }

        if ((bool)m_target && m_target.IsDead())
        {
            m_target = null;
        }

        if ((bool)m_target)
        {
            bool num = CanSenseTarget(m_target);
            SetTargetInfo(m_target.GetZDOID());
            if (num)
            {
                SetAlerted(alert: true);
            }
        }
        else
        {
            SetTargetInfo(ZDOID.None);
        }

        if (IsAlerted())
        {
            m_inDangerTimer += dt;
            if (m_inDangerTimer > m_timeToSafe)
            {
                m_target = null;
                SetAlerted(alert: false);
            }
        }

        if ((bool)m_target)
        {
            Flee(dt, m_target.transform.position);
            m_target.OnTargeted(sensed: false, alerted: false);
        }
        else
        {
            IdleMovement(dt);
        }

        return true;
    }
    **/

}
