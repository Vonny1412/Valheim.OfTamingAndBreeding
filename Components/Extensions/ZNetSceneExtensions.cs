using OfTamingAndBreeding.StaticContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OfTamingAndBreeding.Components.Extensions
{
    internal static class ZNetSceneExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateObjects(this ZNetScene zns, List<ZDO> currentNearObjects, List<ZDO> currentDistantObjects)
            => ValheimAPI.ZNetScene.__IAPI_CreateObjects_Invoker1.Invoke(zns, currentNearObjects, currentDistantObjects);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Dictionary<int, GameObject> GetNamedPrefabs(this ZNetScene zns)
            => ValheimAPI.ZNetScene.__IAPI_m_namedPrefabs_Invoker.Get(zns);

        public static bool CreateObjects_PatchPrefix(this ZNetScene zns, List<ZDO> currentNearObjects, List<ZDO> currentDistantObjects)
        {
            if (ZNetSceneContext.blockObjectsCreation)
            {
                ZNetSceneContext.pending.Enqueue((new List<ZDO>(currentNearObjects), new List<ZDO>(currentDistantObjects)));
                return false;
            }
            return true;
        }

        public static void UnblockObjectsCreation(this ZNetScene zns)
        {
            ZNetSceneContext.blockObjectsCreation = false;
            while (ZNetSceneContext.pending.Count > 0)
            {
                var (near, distant) = ZNetSceneContext.pending.Dequeue();
                zns.CreateObjects(near, distant);
            }
        }

    }
}
