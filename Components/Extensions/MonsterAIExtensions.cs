using System.Runtime.CompilerServices;

namespace OfTamingAndBreeding.Components.Extensions
{
    internal static class MonsterAIExtensions
    {

        /*
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool UpdateConsumeItem(this MonsterAI that, Humanoid humanoid, float dt)
            => ValheimAPI.MonsterAI.__IAPI_UpdateConsumeItem_Invoker1.Invoke(that, humanoid, dt);
        */

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemDrop GetConsumeTarget(this MonsterAI that)
            => ValheimAPI.MonsterAI.__IAPI_m_consumeTarget_Invoker.Get(that);



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetConsumeSearchTimer(this MonsterAI that)
            => ValheimAPI.MonsterAI.__IAPI_m_consumeSearchTimer_Invoker.Get(that);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetConsumeSearchTimer(this MonsterAI that, float value)
            => ValheimAPI.MonsterAI.__IAPI_m_consumeSearchTimer_Invoker.Set(that, value);






    }
}
