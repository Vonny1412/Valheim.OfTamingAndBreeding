using Jotunn.Managers;
using OfTamingAndBreeding.Data;
using OfTamingAndBreeding.Helpers;
using OfTamingAndBreeding.ValheimAPI.LowLevel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OfTamingAndBreeding.ValheimAPI
{
    internal static class TameableExtensions
    {

        public static class ConsumeContext
        {
            [ThreadStatic] public static bool hasValue;
            [ThreadStatic] public static int lastItemDroppedByAnyPlayer;
            [ThreadStatic] public static int lastItemInstanceId;

            public static void Clear()
            {
                hasValue = false;
                lastItemDroppedByAnyPlayer = 0;
                lastItemInstanceId = 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ZNetView GetZNetView(this Tameable that)
            => LowLevel.Tameable.__IAPI_m_nview_Invoker.Get(that);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnConsumedItem(this Tameable that, ItemDrop item)
            => LowLevel.Tameable.__IAPI_OnConsumedItem_Invoker1.Invoke(that, item);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DecreaseRemainingTime(this Tameable that, float time)
            => LowLevel.Tameable.__IAPI_DecreaseRemainingTime_Invoker1.Invoke(that, time);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetRemainingTime(this Tameable that)
            => LowLevel.Tameable.__IAPI_GetRemainingTime_Invoker1.Invoke(that);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Tame(this Tameable that)
            => LowLevel.Tameable.__IAPI_Tame_Invoker1.Invoke(that);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Character GetCharacter(this Tameable that)
            => LowLevel.Tameable.__IAPI_m_character_Invoker.Get(that);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Player GetPlayer(this Tameable that, ZDOID characterID)
            => LowLevel.Tameable.__IAPI_GetPlayer_Invoker1.Invoke(that, characterID);

        //
        // 
        //




        

        public static void Awake_PatchPostfix(this Tameable tameable)
        {
            var m_nview = tameable.GetZNetView();
            if (m_nview.IsValid())
            {
                // sadly we need to wrap the target methods because valheim is doing this:
                // > m_action.DynamicInvoke(ZNetView.Deserialize(rpc, m_action.Method.GetParameters(), pkg));
                // the first param of the extension methods (this EggGrow eggGrow) is making problems while deserializing
                m_nview.Register<float>("RPC_UpdateFedDuration", (long sender, float totalFactor) => tameable.RPC_UpdateFedDuration(sender, totalFactor));
            }

            if (tameable.TryGetComponent<Custom.OTAB_CustomAnimalAI>(out var customAnimalAI))
            {
                customAnimalAI.m_onConsumedItem = (Action<ItemDrop>)Delegate.Combine(customAnimalAI.m_onConsumedItem, new Action<ItemDrop>(tameable.OnConsumedItem));
            }

            if (tameable.TryGetComponent<Custom.OTAB_TameableTrait>(out _) == false)
            {
                // we are using late-registration
                // instead of adding component in CreatureProcessor

                var prefabName = Utils.GetPrefabName(tameable.gameObject.name);
                var prefab =  PrefabManager.Instance.GetPrefab(prefabName);
                var prefabTameable = prefab.GetComponent<Tameable>();

                var c1 = tameable.gameObject.gameObject.AddComponent<Custom.OTAB_TameableTrait>();
                var c2 = prefab.gameObject.AddComponent<Custom.OTAB_TameableTrait>();

                c1.m_baseFedDuration = prefabTameable.m_fedDuration;
                c2.m_baseFedDuration = prefabTameable.m_fedDuration;

                c1.m_baseTamingTime = prefabTameable.m_tamingTime;
                c2.m_baseTamingTime = prefabTameable.m_tamingTime;
            }

            tameable.UpdateFedDuration();
            tameable.UpdateTamingTime();
        }

        public static void UpdateTamingTime(this Tameable tameable)
        {
            var m_nview = tameable.GetZNetView();
            if (!m_nview || !m_nview.IsValid()) return;
            var zdo = m_nview.GetZDO();
            if (zdo == null) return;

            var globalFactor = Plugin.Configs.GlobalTamingTimeFactor.Value;
            if (globalFactor < 0f)
            {
                // should not be possible but whatever
                //tameable.UpdateTamingTime(1f); // back to base
                return;
            }
            var totalFactor = globalFactor;
            tameable.UpdateTamingTime(totalFactor);
        }

        private static void UpdateTamingTime(this Tameable tameable, float totalFactor)
        {
            if (totalFactor >= 0) // yes, we do allow 0, too
            {
                var trait = tameable.GetComponent<Custom.OTAB_TameableTrait>();
                tameable.m_tamingTime = trait.m_baseTamingTime * totalFactor;
            }
        }

        public static void UpdateFedDuration(this Tameable tameable)
        {
            var m_nview = tameable.GetZNetView();
            if (!m_nview || !m_nview.IsValid()) return;
            var zdo = m_nview.GetZDO();
            if (zdo == null) return;

            var globalFactor = Plugin.Configs.GlobalFedDurationFactor.Value;
            if (globalFactor < 0f)
            {
                // should not be possible but whatever
                //tameable.UpdateFedDuration(1f); // back to base
                return;
            }
            var customFactor = zdo.GetFloat(Plugin.ZDOVars.z_fedDurationFactor, 1f);
            var totalFactor = globalFactor * customFactor;
            tameable.UpdateFedDuration(totalFactor);
        }

        private static void UpdateFedDuration(this Tameable tameable, float totalFactor)
        {
            if (totalFactor >= 0) // yes, we do allow 0, too
            {
                var baseValues = tameable.GetComponent<Custom.OTAB_TameableTrait>();
                tameable.m_fedDuration = baseValues.m_baseFedDuration * totalFactor;
            }
        }

        private static void RPC_UpdateFedDuration(this Tameable tameable, long sender, float totalFactor)
        {
            var m_nview = tameable.GetZNetView();
            if (!m_nview || !m_nview.IsValid()) return;

            if (!m_nview.IsOwner()) // because already updated
            {
                tameable.UpdateFedDuration(totalFactor);
            }
        }

        public static bool TamingUpdate_PatchPrefix(this Tameable tameable)
        {
            if (tameable.TryGetComponent<Custom.OTAB_Creature>(out var creature))
            {
                if (creature.m_tamingDisabled == true)
                {
                    // taming completly disabled
                    return false;
                }
            }

            if (tameable.TryGetComponent<Custom.OTAB_CustomAnimalAI>(out var customAnimalAI))
            {
                var m_nview = tameable.GetZNetView();
                if (m_nview.IsValid() && m_nview.IsOwner() && !tameable.IsTamed() && !tameable.IsHungry() && !customAnimalAI.GetAnimalAI().IsAlerted())
                {
                    tameable.DecreaseRemainingTime(3f);
                    if (tameable.GetRemainingTime() <= 0f)
                    {
                        tameable.Tame();
                    }
                    else
                    {
                        // todo: broadcast me
                        tameable.m_sootheEffect?.Create(tameable.transform.position, tameable.transform.rotation);
                    }
                }

                return false;
            }

            return true;
        }

        public static bool Tame_PatchPrefix(this Tameable tameable)
        {
            if (tameable.TryGetComponent<Custom.OTAB_CustomAnimalAI>(out var customAnimalAI))
            {
                Game.instance.IncrementPlayerStat(PlayerStatType.CreatureTamed);

                var m_nview = tameable.GetZNetView();
                var m_character = tameable.GetCharacter();
                if (m_nview.IsValid() && m_nview.IsOwner() && (bool)m_character && !tameable.IsTamed())
                {
                    customAnimalAI.MakeTame();
                    tameable.m_tamedEffect?.Create(tameable.transform.position, tameable.transform.rotation); // only for owner is okay
                    Player closestPlayer = Player.GetClosestPlayer(tameable.transform.position, 30f);
                    if ((bool)closestPlayer)
                    {
                        closestPlayer.Message(MessageHud.MessageType.Center, m_character.m_name + " $hud_tamedone");
                    }
                }

                return false;
            }

            return true;
        }

        public static void  Tame_PatchPostfix(this Tameable tameable)
        {
            var character = tameable.GetComponent<Character>();
            if (character)
            {
                character.SetCharacterStuffIfTamed();
            }
        }

        public static bool RPC_Command_PatchPrefix(this Tameable tameable, long sender, ZDOID characterID, bool message)
        {
            if (tameable.TryGetComponent<Custom.OTAB_CustomAnimalAI>(out var customAnimalAI))
            {
                Player player = tameable.GetPlayer(characterID);
                if (player == null)
                {
                    return false;
                }

                var m_nview = tameable.GetZNetView();

                if ((bool)customAnimalAI.m_follow)
                {
                    customAnimalAI.m_follow = null;
                    customAnimalAI.GetAnimalAI().SetPatrolPoint();
                    if (m_nview.IsOwner())
                    {
                        m_nview.GetZDO().Set(ZDOVars.s_follow, "");
                    }

                    if (message)
                    {
                        player.Message(MessageHud.MessageType.Center, tameable.GetHoverName() + " $hud_tamestay");
                    }
                }
                else
                {
                    customAnimalAI.GetAnimalAI().ResetPatrolPoint();
                    customAnimalAI.m_follow = player.gameObject;
                    if (m_nview.IsOwner())
                    {
                        m_nview.GetZDO().Set(ZDOVars.s_follow, player.GetPlayerName());
                    }

                    if (message)
                    {
                        player.Message(MessageHud.MessageType.Center, tameable.GetHoverName() + " $hud_tamefollow");
                    }

                    // well, the following is realy hard to port to AnimalAI

                    //int num = m_nview.GetZDO().GetInt(ZDOVars.s_maxInstances);
                    //if (num > 0)
                    //{
                    //UnsummonMaxInstances(num);
                    //}
                }
                //m_unsummonTime = 0f;

                return false;
            }

            return true;
        }

        public static void UpdateStarvingTimePoint(this Tameable tameable)
        {
            tameable.UpdateStarvingTimePoint(ZNet.instance.GetTime());
        }

        public static float GetStarvingGraceFactor(this Tameable tameable)
        {
            if (tameable.TryGetComponent<Custom.OTAB_Creature>(out var creature))
            {
                if (creature.m_starvingGraceFactor != -1)
                {
                    return creature.m_starvingGraceFactor;
                }
            }
            return Plugin.Configs.DefaultStarvingGraceFactor.Value;
        }



        private static long UpdateStarvingTimePoint(this Tameable tameable, DateTime now)
        {
            var m_nview = tameable.GetZNetView();
            var zdo = m_nview.GetZDO();
            if (m_nview.IsOwner())
            {
                var starvingGraceFactor = tameable.GetStarvingGraceFactor();
                var starvingAfter = now.AddSeconds(tameable.m_fedDuration + tameable.m_fedDuration * starvingGraceFactor);
                var ticks = starvingAfter.Ticks;
                ZNetHelper.SetLong(zdo, Plugin.ZDOVars.z_starvingAfter, ticks);
                return ticks;
            }
            return zdo.GetLong(Plugin.ZDOVars.z_starvingAfter, -1);
        }

        public static bool IsStarving(this Tameable tameable)
        {

            if (tameable.TryGetComponent<Custom.OTAB_Creature>(out var creatureTrait))
            {
                if (creatureTrait.m_fedTimerDisabled)
                {
                    // creatures that do not eat at all wont get starving!
                    return false;
                }
            }

            var m_nview = tameable.GetZNetView();
            if (!m_nview || !m_nview.IsValid()) return false;
            var zdo = m_nview.GetZDO();
            if (zdo == null) return false;

            var now = ZNet.instance.GetTime();

            var z_starvingAfter = zdo.GetLong(Plugin.ZDOVars.z_starvingAfter, -1);
            if (z_starvingAfter == -1)
            {
                // not set yet?
                if (m_nview.IsOwner())
                {
                    var s_tameLastFeeding = zdo.GetLong(ZDOVars.s_tameLastFeeding, -1);
                    if (s_tameLastFeeding != -1)
                    {
                        // how valheim stores time: m_nview.GetZDO().Set(ZDOVars.s_tameLastFeeding, ZNet.instance.GetTime().Ticks);
                        z_starvingAfter = tameable.UpdateStarvingTimePoint(new DateTime(s_tameLastFeeding));
                    }
                    else
                    {
                        var baseAI = tameable.GetComponent<BaseAI>();
                        z_starvingAfter = tameable.UpdateStarvingTimePoint(now - baseAI.GetTimeSinceSpawned());
                    }
                }
            }
            if (z_starvingAfter == -1)
            {
                return false;
            }

            var isStarving = z_starvingAfter < now.Ticks;
            return isStarving;
        }

        public static bool OnConsumedItem_PrefixPatch(this Tameable tameable, ItemDrop item)
        {
            var creature = tameable.GetComponent<Custom.OTAB_Creature>();

            if (creature && creature.m_fedTimerDisabled == true)
            {
                // creatures that do not eat at all wont get starving!
                // block ResetFeedingTimer()
                return false;
            }

            var m_nview = tameable.GetZNetView();
            if (!m_nview.IsOwner())
            {
                // just for safety
                return true;
            }

            var customFactor = m_nview.GetZDO().GetFloat(Plugin.ZDOVars.z_fedDurationFactor, 1f);

            if (creature && creature.HasCustomConsumeItems(out var consumeItems))
            {
                var itemName = Utils.GetPrefabName(item.gameObject.name);
                var newFactor = 1f;
                foreach (var entry in consumeItems)
                {
                    if (entry.Prefab == itemName)
                    {
                        newFactor *= entry.FedDurationFactor;
                        // what if the same prefab exists multiple times in list?
                        // just keep going, maybe one day we gonna expand the feature with more options or values
                        //break;
                    }
                }
                if (newFactor >= 0) // Intentionally allow 0
                {
                    customFactor = newFactor;
                }
            }
            
            var globalFactor = Plugin.Configs.GlobalFedDurationFactor.Value;
            var totalFactor = customFactor * globalFactor;
            ZNetHelper.SetFloat(m_nview.GetZDO(), Plugin.ZDOVars.z_fedDurationFactor, customFactor);
            m_nview.InvokeRPC(ZNetView.Everybody, "RPC_UpdateFedDuration", totalFactor);
            tameable.UpdateFedDuration(totalFactor);

            tameable.UpdateStarvingTimePoint();
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
                if (droppedByAnyPlayer < 0 && ConsumeContext.hasValue && item && ConsumeContext.lastItemInstanceId == item.GetInstanceID())
                {
                    droppedByAnyPlayer = ConsumeContext.lastItemDroppedByAnyPlayer;
                }
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

            // let valheim handle
            return true;
        }

        public static float GetFedTimeLeft(this Tameable t, ZDO zdo, DateTime zTime)
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

        public static IReadOnlyList<string> GetFeedingHoverText(this Tameable tameable)
        {
            var m_nview = tameable.GetZNetView();
            var zdo = m_nview.GetZDO();

            var L = Localization.instance;
            var zTime = ZNet.instance.GetTime();

            var returnLines = new List<string>(capacity: 2);

            tameable.AddFedTimerLineIfEnabled(returnLines, zdo, zTime, L);

            return returnLines;
        }

        private static void AddFedTimerLineIfEnabled(this Tameable tameable, List<string> returnLines, ZDO zdo, DateTime zTime, Localization L)
        {
            if (tameable.m_fedDuration <= 0)
                return;

            float secondsFedLeft = tameable.GetFedTimeLeft(zdo, zTime);
            if (secondsFedLeft >= 0)
            {
                // is fed
                if (Plugin.Configs.HoverShowFedTimer.Value)
                {
                    returnLines.Add(Helpers.StringHelper.FormatRelativeTime(
                        secondsFedLeft,
                        labelPositive:      L.Localize("$otab_hover_fed"),
                        labelPositiveAlt:   L.Localize("$otab_hover_fed_alt"),
                        labelNegative:      L.Localize("$otab_hover_hungry"),
                        labelNegativeAlt:   L.Localize("$otab_hover_hungry_alt"),
                        colorPositive:      Plugin.Configs.HoverColorGood.Value,
                        colorNegative:      Plugin.Configs.HoverColorBad.Value
                    ));
                }
            }
            else
            {
                if (Plugin.IsOTABDataLoaded())
                {
                    // starvation is an otab feature

                    var z_starvingAfter = zdo.GetLong(Plugin.ZDOVars.z_starvingAfter, -1);
                    if (z_starvingAfter != -1 && Plugin.Configs.HoverShowStarvingTimer.Value)
                    {
                        // is starving
                        // and starving point is set
                        var now = ZNet.instance.GetTime();
                        var secondsUntillStarving = (new DateTime(z_starvingAfter) - now).TotalSeconds;

                        returnLines.Add(Helpers.StringHelper.FormatRelativeTime(
                            secondsUntillStarving,
                            labelPositive: L.Localize("$otab_hover_starving"),
                            labelPositiveAlt: L.Localize("$otab_hover_starving_alt"),
                            labelNegative: L.Localize("$otab_hover_starving_alt"),
                            labelNegativeAlt: L.Localize("$otab_hover_starving_alt"),
                            colorPositive: Plugin.Configs.HoverColorBad.Value,
                            colorNegative: Plugin.Configs.HoverColorBad.Value
                        ));
                    }
                }
            }
        }

        // todo: also broadcast original soothe-effect when fedtimer is running

        public static float GetRemainingTimeDecreaseFactor(this Tameable tameable)
        {
            var character = tameable.GetComponent<Character>();
            if (character)
            {
                int stars = Mathf.Max(0, character.GetLevel() - 1);
                float slowdown = Plugin.Configs.TamingSlowdownPerStar.Value;
                float divisor = 1f + (stars * slowdown);
                if (divisor > 0f) // safety
                {
                    return 1 / divisor; // because we wanna return mul factor
                }
            }
            return 1;
        }

    }
}
