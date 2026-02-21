using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OfTamingAndBreeding.ValheimAPI
{
    internal static class ZNetSceneExtensions
    {

        public static class ObjectsContext
        {
            public static bool blockObjectsCreation = false;
            public static readonly Queue<(List<ZDO> near, List<ZDO> distant)> pending = new Queue<(List<ZDO> near, List<ZDO> distant)>();
            public static void Clear()
            {
                blockObjectsCreation = false;
                pending.Clear();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateObjects(this ZNetScene zns, List<ZDO> currentNearObjects, List<ZDO> currentDistantObjects)
            => LowLevel.ZNetScene.__IAPI_CreateObjects_Invoker1.Invoke(zns, currentNearObjects, currentDistantObjects);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Dictionary<int, GameObject> GetNamedPrefabs(this ZNetScene zns)
            => LowLevel.ZNetScene.__IAPI_m_namedPrefabs_Invoker.Get(zns);

        public static bool CreateObjects_PatchPrefix(this ZNetScene zns, List<ZDO> currentNearObjects, List<ZDO> currentDistantObjects)
        {
            if (ObjectsContext.blockObjectsCreation)
            {
                ObjectsContext.pending.Enqueue((new List<ZDO>(currentNearObjects), new List<ZDO>(currentDistantObjects)));
                return false;
            }
            return true;
        }

        public static void UnblockObjectsCreation(this ZNetScene zns)
        {
            ObjectsContext.blockObjectsCreation = false;
            while (ObjectsContext.pending.Count > 0)
            {
                var (near, distant) = ObjectsContext.pending.Dequeue();
                zns.CreateObjects(near, distant);
            }
        }

    }
}
