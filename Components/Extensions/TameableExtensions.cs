using System.Runtime.CompilerServices;

namespace OfTamingAndBreeding.Components.Extensions
{
    internal static class TameableExtensions
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnConsumedItem(this Tameable that, ItemDrop item)
            => ValheimAPI.Tameable.__IAPI_OnConsumedItem_Invoker1.Invoke(that, item);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DecreaseRemainingTime(this Tameable that, float time)
            => ValheimAPI.Tameable.__IAPI_DecreaseRemainingTime_Invoker1.Invoke(that, time);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetRemainingTime(this Tameable that)
            => ValheimAPI.Tameable.__IAPI_GetRemainingTime_Invoker1.Invoke(that);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Tame(this Tameable that)
            => ValheimAPI.Tameable.__IAPI_Tame_Invoker1.Invoke(that);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Player GetPlayer(this Tameable that, ZDOID characterID)
            => ValheimAPI.Tameable.__IAPI_GetPlayer_Invoker1.Invoke(that, characterID);

    }
}
