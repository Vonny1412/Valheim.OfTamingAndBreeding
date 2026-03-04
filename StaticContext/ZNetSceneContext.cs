using System;
using System.Collections.Generic;

namespace OfTamingAndBreeding.StaticContext
{
    internal static class ZNetSceneContext
    {
        public static bool blockObjectsCreation = false;
        public static readonly Queue<(List<ZDO> near, List<ZDO> distant)> pending = new Queue<(List<ZDO> near, List<ZDO> distant)>();

        public static void Clear()
        {
            blockObjectsCreation = false;
            pending.Clear();
        }
    }
}
