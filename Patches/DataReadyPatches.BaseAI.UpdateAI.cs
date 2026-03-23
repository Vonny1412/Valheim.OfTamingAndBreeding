using HarmonyLib;
using OfTamingAndBreeding.Components.Traits;
using System;
using System.Collections.Generic;
using System.Text;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {
        /*
    // original method
    public virtual bool UpdateAI(float dt)
    {
        if (!m_nview.IsValid())
        {
            return false;
        }

        if (!m_nview.IsOwner())
        {
            m_alerted = m_nview.GetZDO().GetBool(ZDOVars.s_alert);
            return false;
        }

        UpdateTakeoffLanding(dt);
        if (m_jumpInterval > 0f)
        {
            m_jumpTimer += dt;
        }

        if (m_randomMoveUpdateTimer > 0f)
        {
            m_randomMoveUpdateTimer -= dt;
        }

        UpdateRegeneration(dt);
        m_timeSinceHurt += dt;
        return true;
    }

        */
        [HarmonyPatch(typeof(BaseAI), "UpdateAI")]
        [HarmonyPostfix] // this makes sure the rest of baseai is getting run
        [HarmonyPriority(Priority.Last)]
        private static void BaseAI_UpdateAI_Postfix(BaseAI __instance, float dt, ref bool __result)
        {
            if (__result == false)
            {
                return; // invalid afterall
            }

            //var trait = __instance.GetComponent<BaseAITrait>();
            var trait = BaseAITrait.GetUnsafe(__instance.gameObject);
            if (trait.UpdateAI(dt))
            {
                __result = false;
            }
        }
    }
}
