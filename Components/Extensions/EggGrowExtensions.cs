using System.Runtime.CompilerServices;

namespace OfTamingAndBreeding.Components.Extensions
{
    internal static class EggGrowExtensions
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UpdateEffects(this EggGrow that, float grow)
            => ValheimAPI.EggGrow.__IAPI_UpdateEffects_Invoker1.Invoke(that, grow);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CanGrow(this EggGrow that)
            => ValheimAPI.EggGrow.__IAPI_CanGrow_Invoker1.Invoke(that);
        
    }
}
