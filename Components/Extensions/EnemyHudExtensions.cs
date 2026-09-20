using System.Runtime.CompilerServices;

namespace OfTamingAndBreeding.Components.Extensions
{
    internal static class EnemyHudExtensions
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object GetHuds(this EnemyHud that)
            => ValheimAPI.EnemyHud.__IAPI_m_huds_Invoker.Get(that);

    }
}
