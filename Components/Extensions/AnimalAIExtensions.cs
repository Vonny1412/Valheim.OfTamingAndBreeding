using System.Runtime.CompilerServices;

namespace OfTamingAndBreeding.Components.Extensions
{
    internal static class AnimalAIExtensions
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetAlerted(this AnimalAI that, bool alert)
            => ValheimAPI.AnimalAI.__IAPI_SetAlerted_Invoker1.Invoke(that, alert);

    }
}
