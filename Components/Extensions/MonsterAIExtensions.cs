using System.Runtime.CompilerServices;

namespace OfTamingAndBreeding.Components.Extensions
{
    internal static class MonsterAIExtensions
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool UpdateConsumeItem(this MonsterAI that, Humanoid humanoid, float dt)
            => ValheimAPI.MonsterAI.__IAPI_UpdateConsumeItem_Invoker1.Invoke(that, humanoid, dt);

    }
}
