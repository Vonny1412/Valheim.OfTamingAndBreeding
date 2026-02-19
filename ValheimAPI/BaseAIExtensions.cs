using OfTamingAndBreeding.Data;
using OfTamingAndBreeding.Data.Models.SubData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OfTamingAndBreeding.ValheimAPI
{
    internal static class BaseAIExtensions
    {
        internal sealed class BaseAIExtraData : Lifecycle.ExtraData<BaseAI, BaseAIExtraData>
        {

        }

        public static class IsEnemyContext
        {
            [ThreadStatic] public static int Depth;
            [ThreadStatic] public static Character TargetInstance;
            [ThreadStatic] public static bool Active;

            public static void Cleanup()
            {
                Depth--;
                if (Depth <= 0)
                {
                    Depth = 0;
                    Active = false;
                    TargetInstance = null;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ZNetView GetZNetView(this BaseAI that)
            => LowLevel.BaseAI.__IAPI_m_nview_Invoker.Get(that);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Follow(this BaseAI that, GameObject target, float dt)
            => LowLevel.BaseAI.__IAPI_Follow_Invoker1.Invoke(that, target, dt);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tameable GetTameable(this BaseAI that)
            => LowLevel.BaseAI.__IAPI_m_tamable_Invoker.Get(that);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool MoveTo(this BaseAI that, float dt, Vector3 point, float dist, bool run)
            => LowLevel.BaseAI.__IAPI_MoveTo_Invoker1.Invoke(that, dt, point, dist, run);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LookAt(this BaseAI that, Vector3 point)
            => LowLevel.BaseAI.__IAPI_LookAt_Invoker1.Invoke(that, point);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLookingAt(this BaseAI that, Vector3 point, float minAngle, bool inverted = false)
            => LowLevel.BaseAI.__IAPI_IsLookingAt_Invoker1.Invoke(that, point, minAngle, inverted);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ZSyncAnimation GetZSyncAnimation(this BaseAI that)
            => LowLevel.BaseAI.__IAPI_m_animator_Invoker.Get(that);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Character GetCharacter(this BaseAI that)
            => LowLevel.BaseAI.__IAPI_m_character_Invoker.Get(that);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HavePath(this BaseAI that, Vector3 target)
            => LowLevel.BaseAI.__IAPI_HavePath_Invoker1.Invoke(that, target);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetAlertedUnsafe(this BaseAI that, bool alerted)
            => LowLevel.BaseAI.__IAPI_m_alerted_Invoker.Set(that, alerted);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetAlerted(this BaseAI that, bool alerted)
            => LowLevel.BaseAI.__IAPI_SetAlerted_Invoker1.Invoke(that, alerted);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UpdateTakeoffLanding(this BaseAI that, float dt)
            => LowLevel.BaseAI.__IAPI_UpdateTakeoffLanding_Invoker1.Invoke(that, dt);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetJumpTimer(this BaseAI that)
            => LowLevel.BaseAI.__IAPI_m_jumpTimer_Invoker.Get(that);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetJumpTimer(this BaseAI that, float value)
            => LowLevel.BaseAI.__IAPI_m_jumpTimer_Invoker.Set(that, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetRandomMoveUpdateTimer(this BaseAI that)
            => LowLevel.BaseAI.__IAPI_m_randomMoveUpdateTimer_Invoker.Get(that);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetRandomMoveUpdateTimer(this BaseAI that, float value)
            => LowLevel.BaseAI.__IAPI_m_randomMoveUpdateTimer_Invoker.Set(that, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UpdateRegeneration(this BaseAI that, float dt)
            => LowLevel.BaseAI.__IAPI_UpdateRegeneration_Invoker1.Invoke(that, dt);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetTimeSinceHurt(this BaseAI that)
            => LowLevel.BaseAI.__IAPI_m_timeSinceHurt_Invoker.Get(that);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetTimeSinceHurt(this BaseAI that, float value)
            => LowLevel.BaseAI.__IAPI_m_timeSinceHurt_Invoker.Set(that, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<BaseAI> GetInstances()
            => LowLevel.BaseAI.__IAPI_m_instances_Invoker.Get(null);

        private static readonly int m_itemMask = LayerMask.GetMask("item");
        private static readonly Collider[] colliders = new Collider[256]; // 256 should be enough for now

        public static bool CanConsume(IReadOnlyList<ItemDrop> consumeList, string itemName)
        {
            foreach (ItemDrop item in consumeList)
            {
                if (item.m_itemData.m_shared.m_name == itemName)
                {
                    return true;
                }
            }
            return false;
        }

        public static ItemDrop FindClosestConsumableItem(this BaseAI baseAI, float maxRange, IReadOnlyList<ItemDrop> consumeList)
        {
            var pos = baseAI.transform.position;

            int count = Physics.OverlapSphereNonAlloc(pos, maxRange, colliders, m_itemMask);
            if (count <= 0)
                return null;

            ItemDrop chosen = null;
            float num = 999999f;

            for (int i = 0; i < count; i++)
            {
                Collider col = colliders[i];
                if (!col)
                    continue;

                var rb = col.attachedRigidbody;
                if (!rb)
                    continue;

                ItemDrop item = rb.GetComponent<ItemDrop>();
                if (!item)
                    continue;

                // Same as vanilla: ZNetView must be valid + item must be consumable
                var nview = item.GetComponent<ZNetView>();
                if (!nview || !nview.IsValid())
                    continue;

                var data = item.m_itemData;
                if (data == null || !CanConsume(consumeList, data.m_shared.m_name))
                    continue;

                float num2 = Vector3.Distance(item.transform.position, pos);
                if (chosen == null || num2 < num)
                {
                    chosen = item;
                    num = num2;
                }
            }

            if (chosen != null)
            {
                if (baseAI.HavePath(chosen.transform.position))
                {
                    return chosen;
                }
            }

            return null;
        }

        public static ItemDrop FindNearbyConsumableItem(this BaseAI baseAI, float maxRange, IReadOnlyList<ItemDrop> consumeList)
        {
            var pos = baseAI.transform.position;

            int count = Physics.OverlapSphereNonAlloc(pos, maxRange, colliders, m_itemMask);
            if (count <= 0)
                return null;

            ItemDrop chosen = null;
            float totalWeight = 0f;

            for (int i = 0; i < count; i++)
            {
                Collider col = colliders[i];
                if (!col)
                    continue;

                var rb = col.attachedRigidbody;
                if (!rb)
                    continue;

                ItemDrop item = rb.GetComponent<ItemDrop>();
                if (!item)
                    continue;

                // Same as vanilla: ZNetView must be valid + item must be consumable
                var nview = item.GetComponent<ZNetView>();
                if (!nview || !nview.IsValid())
                    continue;

                var data = item.m_itemData;
                if (data == null || !CanConsume(consumeList, data.m_shared.m_name))
                    continue;

                //float dist = (item.transform.position - pos).sqrMagnitude;
                float dist = Vector3.Distance(item.transform.position, pos); // this costs more performance but z-distance could be important
                if (dist > maxRange)
                    continue;

                // Higher weight when closer (linear)
                float w = maxRange - dist;
                if (w <= 0f)
                    continue;

                // One-pass weighted selection (roulette/reservoir)
                totalWeight += w;
                if (UnityEngine.Random.value * totalWeight <= w)
                {
                    chosen = item;
                }
            }

            if (chosen != null)
            {
                if (baseAI.HavePath(chosen.transform.position))
                {
                    return chosen;
                }
            }

            return null;
        }

        public static bool IsEnemy_PatchPrefix(this BaseAI baseAI, Character a, Character b, ref bool result)
        {

            IsEnemyContext.Depth++;
            bool isOuterMost = (IsEnemyContext.Depth == 1);
            if (!isOuterMost)
            {
                return true;
            }

            if (!Helpers.ZNetHelper.TryGetZDO(a, out ZDO zdo1, out ZNetView nview1))
            {
                return true; // i dont care
            }
            if (!Helpers.ZNetHelper.TryGetZDO(b, out ZDO zdo2, out ZNetView nview2))
            {
                return true; // i dont care
            }

            Character.Faction faction1 = a.GetFaction();
            Character.Faction faction2 = b.GetFaction();
            bool isTamed1 = a.IsTamed();
            bool isTamed2 = b.IsTamed();

            // both are wild => let valheim decide
            if (isTamed1 == isTamed2 && !isTamed1)
            {
                return true;
            }

            var name1 = Utils.GetPrefabName(a.gameObject.name);
            var name2 = Utils.GetPrefabName(b.gameObject.name);

            // handle stick-to-faction
            var stickToFaction1 = Runtime.Character.GetSticksToFaction(name1);
            var stickToFaction2 = Runtime.Character.GetSticksToFaction(name2);
            if (isTamed1 && stickToFaction1 || isTamed2 && stickToFaction2)
            {
                if (faction1 == faction2)
                {
                    result = false;
                    return false;
                }
                else if (faction1 == Character.Faction.Undead && faction2 == Character.Faction.Demon)
                {
                    result = false;
                    return false;
                }
                else if (faction1 == Character.Faction.Demon && faction2 == Character.Faction.Undead)
                {
                    result = false;
                    return false;
                }
            }

            var tameable1 = a.GetComponent<Tameable>();
            var tameable2 = b.GetComponent<Tameable>();
            var isStarving1 = tameable1 && tameable1.IsStarving();
            var isStarving2 = tameable2 && tameable2.IsStarving();

            if (isTamed1 && isTamed2)
            {
                var canAttack = false;

                switch (Runtime.Character.GetCanAttackTames(name1))
                {
                    case IsEnemyCondition.Never:
                        canAttack = false;
                        break;
                    case IsEnemyCondition.Always:
                        canAttack = true;
                        break;
                    case IsEnemyCondition.WhenStarving:
                        canAttack = isStarving1;
                        break;
                }

                if (!canAttack)
                {
                    switch (Runtime.Character.GetCanBeAttackedByTames(name2))
                    {
                        case IsEnemyCondition.Never:
                            canAttack = false;
                            break;
                        case IsEnemyCondition.Always:
                            canAttack = true;
                            break;
                        case IsEnemyCondition.WhenStarving:
                            canAttack = isStarving2;
                            break;
                    }
                }

                if (canAttack)
                {
                    IsEnemyContext.Active = true;
                    IsEnemyContext.TargetInstance = b;
                }

            }
            else
            {
                if (isTamed1 && faction2 == Character.Faction.Players)
                {
                    var canAttack = false;
                    switch (Runtime.Character.GetCanAttackPlayer(name1))
                    {
                        case IsEnemyCondition.Never:
                            canAttack = false;
                            break;
                        case IsEnemyCondition.Always:
                            canAttack = true;
                            break;
                        case IsEnemyCondition.WhenStarving:
                            canAttack = isStarving1;
                            break;
                    }
                    if (canAttack)
                    {
                        result = true;
                        return false;
                    }
                }
            }

            return true; // let valheim decide
        }

        public static bool IdleMovement_PatchPrefix(this BaseAI baseAI, float dt)
        {
            if (baseAI is AnimalAI animalAI)
            {
                return animalAI.IdleMovement_PatchPrefix(dt);
            }
            return true;
        }

    }
}
