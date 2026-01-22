using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OfTamingAndBreeding.Patches
{
    [HarmonyPatch(typeof(Tameable), "DecreaseRemainingTime")]
    static class Tameable_DecreaseRemainingTime_Patch
    {
        [HarmonyPriority(Priority.Last)]
        static void Prefix(Tameable __instance, ref float time)
        {
            var character = __instance.GetComponent<Character>();
            if (!character) return;

            int stars = Mathf.Max(0, character.GetLevel() - 1);

            float slowdown = Plugin.Configs.TamingSlowdownPerStar.Value;
            if (slowdown <= 0f || stars == 0) return; // 0 disables, 0★ no change

            float divisor = 1f + (stars * slowdown);
            if (divisor <= 0f) return; // safety

            time /= divisor;
        }
    }
    /** original method
    private void DecreaseRemainingTime(float time)
    {
        if (!m_nview.IsValid())
        {
            return;
        }

        float remainingTime = GetRemainingTime();
        s_nearbyPlayers.Clear();
        Player.GetPlayersInRange(base.transform.position, m_tamingSpeedMultiplierRange, s_nearbyPlayers);
        foreach (Player s_nearbyPlayer in s_nearbyPlayers)
        {
            if (s_nearbyPlayer.GetSEMan().HaveStatusAttribute(StatusEffect.StatusAttribute.TamingBoost))
            {
                time *= m_tamingBoostMultiplier;
            }
        }

        remainingTime -= time;
        if (remainingTime < 0f)
        {
            remainingTime = 0f;
        }

        m_nview.GetZDO().Set(ZDOVars.s_tameTimeLeft, remainingTime);
    }
    **/
}
