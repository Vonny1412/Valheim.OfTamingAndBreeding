using HarmonyLib;
using OfTamingAndBreeding.Internals;
using System;
using static UnityEngine.Networking.UnityWebRequest;

namespace OfTamingAndBreeding.Patches
{
    [HarmonyPatch(typeof(AnimalAI), "UpdateAI")]
    static class AnimalAI_UpdateAI_Patch
    {
        static void Postfix(AnimalAI __instance, float dt)
        {
            // we cannot use prefix. because in vanilla the AnimalAI.UpdateAI() is calling base.UpdateAI (base=BaseAI)
            // and we cannot access and call BaseAI.UpdateAI for now because it would result in recursive calling of this patch
            // so just add the consume part at the end, animals got no big ai afterall

            // check if this animal is an animal that we are handling
            if (Internals.AnimalAIAPI.TryGet(__instance, out Internals.AnimalAIAPI api))
            {
                // custom updateAI to make the animal consume items on the ground
                api.UpdateCustomAI(dt);
            }
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
