using OfTamingAndBreeding.Data;
using OfTamingAndBreeding.Helpers;
using OfTamingAndBreeding.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Contexts;
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
            if (m_nview.IsValid())
            {
                m_nview.Register<float>("RPC_SetFedDuration", RPC_SetFedDuration);
            }
        }

        private void RPC_SetFedDuration(long sender, float duration)
        {
            m_fedDuration = duration;
            if (m_nview.IsOwner())
            {
                Helpers.ZNetHelper.SetFloat(m_nview.GetZDO(), Plugin.ZDOVars.z_fedDuration, duration);
            }
        }

        public void UpdateFedDuration()
        {
            var fedDuration = m_nview.GetZDO().GetFloat(Plugin.ZDOVars.z_fedDuration, -1);
            if (fedDuration >= 0) // yes, we do allow 0, too
            {
                m_fedDuration = fedDuration;
            }
        }

        public void UpdateStarvingTimePoint()
        {
            if (m_nview.IsOwner())
            {
                var prefabName = Utils.GetPrefabName(gameObject.name);
                if (Runtime.Tameable.GetIsEatingDisabled(prefabName))
                {
                    // creatures that do not eat at all wont get starving!
                    return;
                }

                var starvingDelayMul = Runtime.Tameable.GetStarvingGraceMultiplier(prefabName);
                var starvingAfter = ZNet.instance.GetTime().AddSeconds(m_fedDuration + m_fedDuration*starvingDelayMul);
                ZNetHelper.SetLong(m_nview.GetZDO(), Plugin.ZDOVars.z_starvingAfter, starvingAfter.Ticks);
            }
        }

        public bool IsStarving()
        {
            var l = m_nview.GetZDO().GetLong(Plugin.ZDOVars.z_starvingAfter, -1);
            if (l == -1)
            {
                // either not set yet or eating is disabled
                return false;
            }
            return l < ZNet.instance.GetTime().Ticks;
        }

        // hint: Tameable.OnConsumedItem is only called for owner
        public bool OnConsumedItem_Prefix(ItemDrop item)
        {

            var prefabName = Utils.GetPrefabName(gameObject.name);
            if (Runtime.Tameable.GetIsEatingDisabled(prefabName))
            {
                // block ResetFeedingTimer()
                return false;
            }

            var itemName = Utils.GetPrefabName(item.gameObject.name);
            //zdo.Set(Plugin.ZDO.s_lastConsumedItem, itemName);

            // handle multiplied fedDuration
            if (Runtime.MonsterAI.TryGetCustomConsumeItems(prefabName, out Data.Models.Creature.MonsterAIConsumItemData[] consumeItems))
            {
                if (!Runtime.Tameable.TryGetBaseFedDuration(prefabName, out float fedDuration))
                {
                    fedDuration = m_fedDuration;
                    // save base duration because we are changing the value
                    Runtime.Tameable.SetBaseFedDuration(prefabName, fedDuration);
                }

                foreach (var entry in consumeItems)
                {
                    if (entry.Prefab == itemName)
                    {
                        fedDuration *= entry.FedDurationMultiply;
                        // what if the same prefab exists multiple times in list?
                        // just keep going, maybe one day we gonna expand the feature with more options or values
                        //break;
                    }
                }
                if (fedDuration >= 0)
                {
                    // Intentionally allow 0:
                    // This means the creature will never become fed by this item.
                    // that way we can keep using taming system and can have items that do not trigger taming/procreation

                    m_nview.InvokeRPC(ZNetView.Everybody, "RPC_SetFedDuration", fedDuration);
                    m_fedDuration = fedDuration;
                }
            }

            UpdateStarvingTimePoint();
            // calling UpdateStarvingTimePoint before RequireFoodDroppedByPlayer-check
            // because every valid food should stop starvation
            // RequireFoodDroppedByPlayer is designed to prevent unwanted taming/breeding

            if (Plugin.Configs.RequireFoodDroppedByPlayer.Value)
            {
                int droppedByAnyPlayer = -1;
                if (item)
                {
                    var item_nview = item.GetComponent<ZNetView>();
                    if (item_nview && item_nview.IsValid())
                    {
                        var item_zdo = item_nview.GetZDO();
                        if (item_zdo != null)
                        {
                            droppedByAnyPlayer = item_zdo.GetInt(Plugin.ZDOVars.z_droppedByAnyPlayer, 0);
                        }
                    }
                }
                if (droppedByAnyPlayer < 0 && Patches.Contexts.ConsumeItemContext.HasValue && item && Patches.Contexts.ConsumeItemContext.LastItemInstanceId == item.GetInstanceID())
                {
                    droppedByAnyPlayer = Patches.Contexts.ConsumeItemContext.lastItemDroppedByAnyPlayer;
                }
                Patches.Contexts.ConsumeItemContext.Clear();
                if (droppedByAnyPlayer < 0)
                {
                    // we dont know if it has been dropped by player or not
                    // let valheim handle
                }
                if (droppedByAnyPlayer == 0)
                {
                    // definitly not dropped by player
                    // prevent ResetFeedingTimer
                    return false;
                }
            }
            else
            {
                Patches.Contexts.ConsumeItemContext.Clear();
            }

            // let valheim handle
            return true;
        }













        public static float GetFedTimeLeft(Tameable t, ZDO zdo, DateTime zTime)
        {
            // IMPORTANT: s_tameLastFeeding is stored as DateTime.Ticks in vanilla Valheim.
            // This matches new DateTime(long) and is NOT a binary or ZNet time value.
            long lastFedTimeLong = zdo.GetLong(ZDOVars.s_tameLastFeeding, 0L);
            double secLeft;
            if (lastFedTimeLong == 0)
            {
                // never fed -> treat as "unfed since spawn"
                var baseAI = t.GetComponent<BaseAI>(); // need to use baseAI for tameable animals
                secLeft = -baseAI.GetTimeSinceSpawned().TotalSeconds;
            }
            else
            {
                var lastFedTime = new DateTime(lastFedTimeLong);
                secLeft = t.m_fedDuration - (zTime - lastFedTime).TotalSeconds;
            }
            return (float)secLeft;
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
