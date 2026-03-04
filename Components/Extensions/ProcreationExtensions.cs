using System.Runtime.CompilerServices;
using UnityEngine;

namespace OfTamingAndBreeding.Components.Extensions
{
    internal static class ProcreationExtensions
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GameObject GetMyPrefab(this Procreation that)
            => ValheimAPI.Procreation.__IAPI_m_myPrefab_Invoker.Get(that);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetMyPrefab(this Procreation that, GameObject prefab)
            => ValheimAPI.Procreation.__IAPI_m_myPrefab_Invoker.Set(that, prefab);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPregnant(this Procreation that)
            => ValheimAPI.Procreation.__IAPI_IsPregnant_Invoker1.Invoke(that);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDue(this Procreation that)
            => ValheimAPI.Procreation.__IAPI_IsDue_Invoker1.Invoke(that);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GameObject GetOffspringPrefab(this Procreation that)
            => ValheimAPI.Procreation.__IAPI_m_offspringPrefab_Invoker.Get(that);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetOffspringPrefab(this Procreation that, GameObject prefab)
            => ValheimAPI.Procreation.__IAPI_m_offspringPrefab_Invoker.Set(that, prefab);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MakePregnant(this Procreation that)
            => ValheimAPI.Procreation.__IAPI_MakePregnant_Invoker1.Invoke(that);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ResetPregnancy(this Procreation that)
            => ValheimAPI.Procreation.__IAPI_ResetPregnancy_Invoker1.Invoke(that);

    }
}
