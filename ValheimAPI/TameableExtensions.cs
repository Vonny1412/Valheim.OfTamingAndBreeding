using Jotunn.Managers;
using OfTamingAndBreeding.Data;
using OfTamingAndBreeding.Data.Models;
using OfTamingAndBreeding.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.Networking.UnityWebRequest;

namespace OfTamingAndBreeding.ValheimAPI
{
    internal static class TameableExtensions
    {
        internal sealed class TameableExtraData : Lifecycle.ExtraData<Tameable, TameableExtraData>
        {
        }

        internal sealed class AnimalExtraData : Lifecycle.ExtraData<Tameable, AnimalExtraData>
        {
            public AnimalAI m_animalAI = null;
            public AnimalAIExtensions.AnimalAIExtraData m_animalAIData = null;
        }

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

            var animalAI = tameable.GetComponent<AnimalAI>();
            if (animalAI != null) // the tameable is an animal
            {
                // check if this animal is an animal that we are handling
                if (AnimalAIExtensions.AnimalAIExtraData.TryGet(animalAI, out var animalAIData))
                {
                    var animalData = AnimalExtraData.GetOrCreate(tameable);
                    // used for making the animal tameable
                    animalData.m_animalAI = animalAI;
                    animalData.m_animalAIData = animalAIData;
                    animalData.m_animalAIData.m_onConsumedItem = (Action<ItemDrop>)Delegate.Combine(animalAIData.m_onConsumedItem, new Action<ItemDrop>(tameable.OnConsumedItem));
                }
            }

            GameObject prefab = null;
            Tameable prefabTameable = null;
            var prefabName = Utils.GetPrefabName(tameable.gameObject.name);
            if (!Runtime.Tameable.TryGetBaseFedDuration(prefabName, out float _))
            {
                prefab = prefab ?? PrefabManager.Instance.GetPrefab(prefabName);
                prefabTameable = prefabTameable ?? prefab.GetComponent<Tameable>();
                Runtime.Tameable.SetBaseFedDuration(prefabName, prefabTameable.m_fedDuration);
            }
            if (!Runtime.Tameable.TryGetBaseTamingTime(prefabName, out float _))
            {
                prefab = prefab ?? PrefabManager.Instance.GetPrefab(prefabName);
                prefabTameable = prefabTameable ?? prefab.GetComponent<Tameable>();
                Runtime.Tameable.SetBaseTamingTime(prefabName, prefabTameable.m_tamingTime);
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
                tameable.UpdateTamingTime(1f); // back to base
                return;
            }
            var totalFactor = globalFactor;
            tameable.UpdateTamingTime(totalFactor);
        }

        private static void UpdateTamingTime(this Tameable tameable, float totalFactor)
        {
            if (totalFactor >= 0) // yes, we do allow 0, too
            {
                var prefabName = Utils.GetPrefabName(tameable.gameObject.name);
                if (Runtime.Tameable.TryGetBaseTamingTime(prefabName, out float baseTamingTime))
                {
                    tameable.m_tamingTime = baseTamingTime * totalFactor;
                }
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
                tameable.UpdateFedDuration(1f); // back to base
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
                var prefabName = Utils.GetPrefabName(tameable.gameObject.name);
                if (Runtime.Tameable.TryGetBaseFedDuration(prefabName, out float baseDuration))
                {
                    tameable.m_fedDuration = baseDuration * totalFactor;
                }
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
            var prefabName = Utils.GetPrefabName(tameable.gameObject.name);
            if (Runtime.Tameable.GetTamingDisabled(prefabName))
            {
                // taming completly disabled
                return false;
            }

            if (AnimalExtraData.TryGet(tameable, out var animal))
            {
                var m_nview = tameable.GetZNetView();
                if (m_nview.IsValid() && m_nview.IsOwner() && !tameable.IsTamed() && !tameable.IsHungry() && !animal.m_animalAI.IsAlerted())
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
            if (AnimalExtraData.TryGet(tameable, out var animal))
            {
                Game.instance.IncrementPlayerStat(PlayerStatType.CreatureTamed);

                var m_nview = tameable.GetZNetView();
                var m_character = tameable.GetCharacter();
                if (m_nview.IsValid() && m_nview.IsOwner() && (bool)m_character && !tameable.IsTamed())
                {
                    animal.m_animalAI.MakeTame();
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
            if (AnimalExtraData.TryGet(tameable, out var animal))
            {
                Player player = tameable.GetPlayer(characterID);
                if (player == null)
                {
                    return false;
                }

                var m_nview = tameable.GetZNetView();

                if ((bool)animal.m_animalAIData.m_follow)
                {
                    animal.m_animalAIData.m_follow = null;
                    animal.m_animalAI.SetPatrolPoint();
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
                    animal.m_animalAI.ResetPatrolPoint();
                    animal.m_animalAIData.m_follow = player.gameObject;
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

        private static void UpdateStarvingTimePoint(this Tameable tameable, DateTime now)
        {
            var m_nview = tameable.GetZNetView();
            if (m_nview.IsOwner())
            {
                var prefabName = Utils.GetPrefabName(tameable.gameObject.name);
                var starvingDelayMul = Runtime.Tameable.GetStarvingGraceFactor(prefabName);
                var starvingAfter = now.AddSeconds(tameable.m_fedDuration + tameable.m_fedDuration * starvingDelayMul);
                ZNetHelper.SetLong(m_nview.GetZDO(), Plugin.ZDOVars.z_starvingAfter, starvingAfter.Ticks);
            }
        }

        public static bool IsStarving(this Tameable tameable)
        {
            // important: first handle init value, then check for GetIsFedTimerDisabled

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
                        tameable.UpdateStarvingTimePoint(new DateTime(s_tameLastFeeding));
                    }
                    else
                    {
                        var baseAI = tameable.GetComponent<BaseAI>();
                        tameable.UpdateStarvingTimePoint(now - baseAI.GetTimeSinceSpawned());
                    }
                    z_starvingAfter = zdo.GetLong(Plugin.ZDOVars.z_starvingAfter, -1);
                }
            }
            if (z_starvingAfter == -1)
            {
                return false;
            }

            var prefabName = Utils.GetPrefabName(tameable.gameObject.name);
            if (Runtime.Tameable.GetIsFedTimerDisabled(prefabName))
            {
                // creatures that do not eat at all wont get starving!
                return false;
            }

            var isStarving = z_starvingAfter < now.Ticks;
            return isStarving;
        }

        public static bool OnConsumedItem_PrefixPatch(this Tameable tameable, ItemDrop item)
        {
            var prefabName = Utils.GetPrefabName(tameable.gameObject.name);
            if (Runtime.Tameable.GetIsFedTimerDisabled(prefabName))
            {
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
            if (Runtime.MonsterAI.TryGetCustomConsumeItems(prefabName, out Data.Models.Creature.MonsterAIConsumItemData[] consumeItems))
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
                // is hungry
                var z_starvingAfter = zdo.GetLong(Plugin.ZDOVars.z_starvingAfter, -1);
                if (z_starvingAfter != -1 && Plugin.Configs.HoverShowStarvingTimer.Value)
                {
                    // starving point is set
                    var now = ZNet.instance.GetTime();
                    var secondsUntillStarving = (new DateTime(z_starvingAfter) - now).TotalSeconds;

                    returnLines.Add(Helpers.StringHelper.FormatRelativeTime(
                        secondsUntillStarving,
                        labelPositive:      L.Localize("$otab_hover_starving"),
                        labelPositiveAlt:   L.Localize("$otab_hover_starving_alt"),
                        labelNegative:      L.Localize("$otab_hover_starving_alt"),
                        labelNegativeAlt:   L.Localize("$otab_hover_starving_alt"),
                        colorPositive:      Plugin.Configs.HoverColorBad.Value,
                        colorNegative:      Plugin.Configs.HoverColorBad.Value
                    ));
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
