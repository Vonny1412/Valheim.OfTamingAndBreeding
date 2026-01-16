using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Patches
{
    [HarmonyPatch(typeof(Tameable), "Tame")]
    static class Tameable_Tame_Patch
    {
        [HarmonyPriority(Priority.Last)]
        static bool Prefix(Tameable __instance)
        {
            var tameableAPI = Internals.TameableAPI.GetOrCreate(__instance);

            // check if this animal is an animal that we are handling
            // this should only get set on Tameable.Awake()
            // if its not set, its not an animal we care about
            if (tameableAPI.animalAIAPI != null)
            {
                // custom Tameable.Tame()
                tameableAPI.TameAnimal();
                return false;
            }
            return true;
        }
    }

    /** original method
    private void Tame()
    {
        Game.instance.IncrementPlayerStat(PlayerStatType.CreatureTamed);
        if (m_nview.IsValid() && m_nview.IsOwner() && (bool)m_monsterAI && (bool)m_character && !IsTamed())
        {
            m_monsterAI.MakeTame();
            m_tamedEffect.Create(base.transform.position, base.transform.rotation);
            Player closestPlayer = Player.GetClosestPlayer(base.transform.position, 30f);
            if ((bool)closestPlayer)
            {
                closestPlayer.Message(MessageHud.MessageType.Center, m_character.m_name + " $hud_tamedone");
            }
        }
    }
    **/

}
