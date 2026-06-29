using OfTamingAndBreeding.Components.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace OfTamingAndBreeding.StaticContext
{
    internal static class ZNetSceneContext
    {
        private static bool blockObjectsCreation = false;
        private static readonly Dictionary<uint, ZDO> nearObjects = new Dictionary<uint, ZDO>();
        private static readonly Dictionary<uint, ZDO> distantObjects = new Dictionary<uint, ZDO>();

        public static void Clear()
        {
            blockObjectsCreation = false;
            //pending.Clear();
            nearObjects.Clear();
            distantObjects.Clear();
        }

        public static void Block()
        {
            blockObjectsCreation = true;
            Plugin.LogDebug("Blocking ZNetScene Objects");
        }

        public static bool IsBlocking()
        {
            return blockObjectsCreation;
        }

        public static void Enqueue(List<ZDO> near, List<ZDO> distant)
        {
            foreach (var n in near)
            {
                nearObjects[n.m_uid.ID] = n;
            }
            foreach (var d in distant)
            {
                distantObjects[d.m_uid.ID] = d;
            }
        }

        public static void Unblock()
        {
            if (!blockObjectsCreation)
            {
                return;
            }
            Plugin.LogDebug("Unblocking ZNetScene Objects");
            blockObjectsCreation = false;
            ZNetScene.instance.CreateObjects(nearObjects.Values.ToList(), distantObjects.Values.ToList());
            nearObjects.Clear();
            distantObjects.Clear();
        }

    }
}
