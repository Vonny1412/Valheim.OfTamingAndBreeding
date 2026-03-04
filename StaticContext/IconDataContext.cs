using System.Collections.Generic;
using UnityEngine;

namespace OfTamingAndBreeding.StaticContext
{
    internal static class IconDataContext
    {
        public static readonly Dictionary<string, Texture2D> iconTextures;

        static IconDataContext()
        {
            iconTextures = new Dictionary<string, Texture2D>();

            Net.NetworkSessionManager.Instance.OnClosed((dataLoaded) => {
                foreach(var texture in iconTextures.Values)
                {
                    if (texture)
                    {
                        UnityEngine.Object.Destroy(texture);
                    }
                }
                iconTextures.Clear();
            });
        }

    }
}
