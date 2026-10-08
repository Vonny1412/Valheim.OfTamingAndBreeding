using System.Runtime.CompilerServices;

namespace OfTamingAndBreeding.Components.Extensions
{
    internal static class ContainerExtensions
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ZNetView GetZNetView(this Container that)
            => ValheimAPI.Container.__IAPI_m_nview_Invoker.Get(that);

    }
}
