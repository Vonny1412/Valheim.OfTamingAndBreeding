using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace OfTamingAndBreeding.Components.Extensions
{
    internal static class BaseAIExtensions
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Follow(this BaseAI that, GameObject target, float dt)
            => ValheimAPI.BaseAI.__IAPI_Follow_Invoker1.Invoke(that, target, dt);

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

    }
}
