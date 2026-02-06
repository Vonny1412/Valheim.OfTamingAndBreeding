using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Patches
{
    [HarmonyPatch(typeof(Tameable), "RPC_Command")]
    static class Tameable_RPC_Command_Patch
    {
        static bool Prefix(Tameable __instance, long sender, ZDOID characterID, bool message)
        {
            var tameableAPI = Internals.TameableAPI.GetOrCreate(__instance);
            if (tameableAPI.IsAnimal())
            {
                tameableAPI.RPC_CommandAnimal(sender, characterID, message);
                return false;
            }
            return true;
        }
    }
    /** original method
    private void RPC_Command(long sender, ZDOID characterID, bool message)
    {
        Player player = GetPlayer(characterID);
        if (player == null || !m_monsterAI)
        {
            return;
        }

        if ((bool)m_monsterAI.GetFollowTarget())
        {
            m_monsterAI.SetFollowTarget(null);
            m_monsterAI.SetPatrolPoint();
            if (m_nview.IsOwner())
            {
                m_nview.GetZDO().Set(ZDOVars.s_follow, "");
            }

            if (message)
            {
                player.Message(MessageHud.MessageType.Center, GetHoverName() + " $hud_tamestay");
            }
        }
        else
        {
            m_monsterAI.ResetPatrolPoint();
            m_monsterAI.SetFollowTarget(player.gameObject);
            if (m_nview.IsOwner())
            {
                m_nview.GetZDO().Set(ZDOVars.s_follow, player.GetPlayerName());
            }

            if (message)
            {
                player.Message(MessageHud.MessageType.Center, GetHoverName() + " $hud_tamefollow");
            }

            int num = m_nview.GetZDO().GetInt(ZDOVars.s_maxInstances);
            if (num > 0)
            {
                UnsummonMaxInstances(num);
            }
        }

        m_unsummonTime = 0f;
    }
    **/
}
