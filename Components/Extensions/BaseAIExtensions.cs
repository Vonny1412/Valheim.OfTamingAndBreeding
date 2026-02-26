using HarmonyLib;
using OfTamingAndBreeding.Components;
using OfTamingAndBreeding.Data;
using OfTamingAndBreeding.Data.Models.SubData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OfTamingAndBreeding.Components.Extensions
{
    internal static class BaseAIExtensions
    {

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
            => ValheimAPI.BaseAI.__IAPI_m_nview_Invoker.Get(that);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Follow(this BaseAI that, GameObject target, float dt)
            => ValheimAPI.BaseAI.__IAPI_Follow_Invoker1.Invoke(that, target, dt);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tameable GetTameable(this BaseAI that)
            => ValheimAPI.BaseAI.__IAPI_m_tamable_Invoker.Get(that);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool MoveTo(this BaseAI that, float dt, Vector3 point, float dist, bool run)
            => ValheimAPI.BaseAI.__IAPI_MoveTo_Invoker1.Invoke(that, dt, point, dist, run);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LookAt(this BaseAI that, Vector3 point)
            => ValheimAPI.BaseAI.__IAPI_LookAt_Invoker1.Invoke(that, point);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLookingAt(this BaseAI that, Vector3 point, float minAngle, bool inverted = false)
            => ValheimAPI.BaseAI.__IAPI_IsLookingAt_Invoker1.Invoke(that, point, minAngle, inverted);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ZSyncAnimation GetZSyncAnimation(this BaseAI that)
            => ValheimAPI.BaseAI.__IAPI_m_animator_Invoker.Get(that);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Character GetCharacter(this BaseAI that)
            => ValheimAPI.BaseAI.__IAPI_m_character_Invoker.Get(that);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HavePath(this BaseAI that, Vector3 target)
            => ValheimAPI.BaseAI.__IAPI_HavePath_Invoker1.Invoke(that, target);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetAlertedUnsafe(this BaseAI that, bool alerted)
            => ValheimAPI.BaseAI.__IAPI_m_alerted_Invoker.Set(that, alerted);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetAlerted(this BaseAI that, bool alerted)
            => ValheimAPI.BaseAI.__IAPI_SetAlerted_Invoker1.Invoke(that, alerted);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UpdateTakeoffLanding(this BaseAI that, float dt)
            => ValheimAPI.BaseAI.__IAPI_UpdateTakeoffLanding_Invoker1.Invoke(that, dt);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetJumpTimer(this BaseAI that)
            => ValheimAPI.BaseAI.__IAPI_m_jumpTimer_Invoker.Get(that);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetJumpTimer(this BaseAI that, float value)
            => ValheimAPI.BaseAI.__IAPI_m_jumpTimer_Invoker.Set(that, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetRandomMoveUpdateTimer(this BaseAI that)
            => ValheimAPI.BaseAI.__IAPI_m_randomMoveUpdateTimer_Invoker.Get(that);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetRandomMoveUpdateTimer(this BaseAI that, float value)
            => ValheimAPI.BaseAI.__IAPI_m_randomMoveUpdateTimer_Invoker.Set(that, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UpdateRegeneration(this BaseAI that, float dt)
            => ValheimAPI.BaseAI.__IAPI_UpdateRegeneration_Invoker1.Invoke(that, dt);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetTimeSinceHurt(this BaseAI that)
            => ValheimAPI.BaseAI.__IAPI_m_timeSinceHurt_Invoker.Get(that);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetTimeSinceHurt(this BaseAI that, float value)
            => ValheimAPI.BaseAI.__IAPI_m_timeSinceHurt_Invoker.Set(that, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<BaseAI> GetInstances()
            => ValheimAPI.BaseAI.__IAPI_m_instances_Invoker.Get(null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetPatrolPoint(this BaseAI that, out Vector3 point)
        {
            object[] args = new object[] { default(Vector3) };
            var result = ValheimAPI.BaseAI.__IAPI_GetPatrolPoint_Invoker1.Invoke(that, args);
            point = (Vector3)args[0];
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RandomMovement(this BaseAI that, float dt, Vector3 centerPoint, bool snapToGround = false)
            => ValheimAPI.BaseAI.__IAPI_RandomMovement_Invoker1.Invoke(that, dt, centerPoint, snapToGround);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 GetSpawnPoint(this BaseAI that)
            => ValheimAPI.BaseAI.__IAPI_m_spawnPoint_Invoker.Get(that);


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

            Character.Faction faction1 = a.GetFaction();
            Character.Faction faction2 = b.GetFaction();
            bool isTamed1 = a.IsTamed();
            bool isTamed2 = b.IsTamed();

            // both are wild => let valheim decide
            if (isTamed1 == isTamed2 && !isTamed1)
            {
                return true;
            }

            var custom1 = a.GetComponent<OTAB_Creature>();
            var custom2 = b.GetComponent<OTAB_Creature>();

            // handle stick-to-faction
            var stickToFaction1 = custom1 != null && custom1.m_stickToFaction;
            var stickToFaction2 = custom2 != null && custom2.m_stickToFaction;
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

            var attackTamed1 = custom1?.m_tamedCanAttackTamed ?? IsEnemyCondition.Never;
            var attackByTamed2 = custom2?.m_tamedCanBeAttackedByTamed ?? IsEnemyCondition.Never;
            var attackPlayer1 = custom1?.m_tamedCanAttackPlayer ?? IsEnemyCondition.Never;

            if (isTamed1 && isTamed2)
            {
                var canAttack = false;

                switch (attackTamed1)
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
                    switch (attackByTamed2)
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

                    switch (attackPlayer1)
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
                // used for animals that can consume food or follow the player
                // that stuff comes always before idle movement
                if (animalAI.IdleMovement_PatchPrefix(dt) == false)
                {
                    return false;
                }
            }

            /*
    protected void IdleMovement(float dt)
    {
        Vector3 centerPoint = ((m_character.IsTamed() || HuntPlayer()) ? base.transform.position : m_spawnPoint);
        if (GetPatrolPoint(out var point))
        {
            centerPoint = point;
        }

        RandomMovement(dt, centerPoint, snapToGround: true);
    }
            */

            var character = baseAI.GetComponent<Character>();
            if (!character || !character.IsTamed() || baseAI.HuntPlayer()) return true;

            if (baseAI.TryGetComponent<OTAB_Creature>(out var creature))
            {
                if (creature.m_tamedStayNearSpawn == true)
                {
                    if (baseAI.GetPatrolPoint(out _))
                    {
                        // is commandable and currently not following
                        // => bound to partol point
                        return true;
                    }

                    baseAI.RandomMovement(dt, baseAI.GetSpawnPoint(), snapToGround: true);
                    return false;
                }
            }

            return true;
        }





    }
}
