using System.Collections.Generic;

namespace OfTamingAndBreeding.StaticContext
{
    internal static class IconDataContext
    {
        public static readonly Dictionary<string, UnityEngine.Texture2D> iconTextures;

        static IconDataContext()
        {
            iconTextures = new Dictionary<string, UnityEngine.Texture2D>();

            Net.NetworkSessionManager.Instance.OnSessionClosed += (netsess, dataLoaded) => {
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
