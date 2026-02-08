using OfTamingAndBreeding.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static UnityEngine.Networking.UnityWebRequest;

namespace OfTamingAndBreeding.Internals
{

    internal partial class TameableAPI : API.Tameable
    {

        private static readonly ConditionalWeakTable<Tameable, TameableAPI> instances
            = new ConditionalWeakTable<Tameable, TameableAPI>();

        public static TameableAPI GetOrCreate(Tameable __instance)
        {
            return instances.GetValue(__instance, inst =>
            {
                Lifecycle.CleanupMarks.Mark(inst.GetComponent<ZNetView>());
                return new TameableAPI(inst);
            });
        }

        public static bool TryGet(Tameable __instance, out TameableAPI api)
            => instances.TryGetValue(__instance, out api);
        public static void Remove(Tameable __instance)
            => instances.Remove(__instance);

        public TameableAPI(Tameable __instance) : base(__instance)
        {
        }

        #region tameable animals

        public AnimalAIAPI animalAIAPI;

        public bool IsAnimal()
        {
            return animalAIAPI != null;
        }

        // patch + original method: Patches/Tameable_TamingUpdate_Patch.cs
        public void TamingAnimalUpdate()
        {
            if (m_nview.IsValid() && m_nview.IsOwner() && !IsTamed() && !IsHungry() && !animalAIAPI.IsAlerted())
            {
                DecreaseRemainingTime(3f);
                if (GetRemainingTime() <= 0f)
                {
                    Tame();
                }
                else
                {
                    m_sootheEffect.Create(transform.position, transform.rotation);
                }
            }
        }

        // patch + original method: Patches/Tameable_Tame_Patch.cs
        public void TameAnimal()
        {
            Game.instance.IncrementPlayerStat(PlayerStatType.CreatureTamed);
            if (m_nview.IsValid() && m_nview.IsOwner() && (bool)m_character && !IsTamed())
            {
                animalAIAPI.MakeTame();
                m_tamedEffect.Create(transform.position, transform.rotation);
                Player closestPlayer = Player.GetClosestPlayer(transform.position, 30f);
                if ((bool)closestPlayer)
                {
                    closestPlayer.Message(MessageHud.MessageType.Center, m_character.m_name + " $hud_tamedone");
                }
            }
        }

        // patch + original method: Patches/Tameable_RPC_Command_Patch.cs
        public void RPC_CommandAnimal(long sender, ZDOID characterID, bool message)
        {
            Player player = GetPlayer(characterID);
            if (player == null)
            {
                return;
            }

            if ((bool)animalAIAPI.GetFollowTarget())
            {
                animalAIAPI.SetFollowTarget(null);
                animalAIAPI.SetPatrolPoint();
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
                animalAIAPI.ResetPatrolPoint();
                animalAIAPI.SetFollowTarget(player.gameObject);
                if (m_nview.IsOwner())
                {
                    m_nview.GetZDO().Set(ZDOVars.s_follow, player.GetPlayerName());
                }

                if (message)
                {
                    player.Message(MessageHud.MessageType.Center, GetHoverName() + " $hud_tamefollow");
                }

                // well, the following is realy hard to port to AnimalAI

                //int num = m_nview.GetZDO().GetInt(ZDOVars.s_maxInstances);
                //if (num > 0)
                //{
                //UnsummonMaxInstances(num);
                //}
            }
            //m_unsummonTime = 0f;
        }

        #endregion

    }

}
