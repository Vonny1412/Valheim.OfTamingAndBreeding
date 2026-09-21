using System.Collections.Generic;

namespace OfTamingAndBreeding.Runtime
{
    internal static class TextureDataContext
    {
        public static readonly Dictionary<string, UnityEngine.Texture2D> textures;

        static TextureDataContext()
        {
            textures = new Dictionary<string, UnityEngine.Texture2D>();

            Network.NetworkSessionManager.OnSessionClosed += () => {
                foreach(var texture in textures.Values)
                {
                    if (texture)
                    {
                        UnityEngine.Object.Destroy(texture);
                    }
                }
                textures.Clear();
            };
        }

    }
}
