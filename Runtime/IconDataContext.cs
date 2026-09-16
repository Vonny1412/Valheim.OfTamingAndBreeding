using System.Collections.Generic;

namespace OfTamingAndBreeding.Runtime
{
    internal static class IconDataContext
    {
        public static readonly Dictionary<string, UnityEngine.Texture2D> iconTextures;

        static IconDataContext()
        {
            iconTextures = new Dictionary<string, UnityEngine.Texture2D>();

            Network.NetworkSessionManager.OnSessionClosed += () => {
                foreach(var texture in iconTextures.Values)
                {
                    if (texture)
                    {
                        UnityEngine.Object.Destroy(texture);
                    }
                }
                iconTextures.Clear();
            };
        }

    }
}
