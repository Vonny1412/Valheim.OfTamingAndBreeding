using HarmonyLib;
using OfTamingAndBreeding.StaticContext;
using System.Collections.Generic;

namespace OfTamingAndBreeding.Patches
{
    internal partial class AlwaysActivePatches
    {

        [HarmonyPatch(typeof(ZNetScene), "CreateObjects")]
        [HarmonyPrefix]
        private static bool ZNetScene_CreateObjects_Prefix(ZNetScene __instance, List<ZDO> currentNearObjects, List<ZDO> currentDistantObjects)
        {
            // do not block even when otab is not ready
            // because we need to cache objects while client is waiting for otab data from server

            // IMPORTANT: We must not let ZNetScene instantiate network objects before OTAB server data is applied.
            // Otherwise components (Awake/Start) would run with wrong vanilla values.
            // We therefore defer CreateObjects until DataOrchestrator marks dataLoaded == true.

            if (ZNetSceneContext.blockObjectsCreation)
            {
                ZNetSceneContext.pending.Enqueue((new List<ZDO>(currentNearObjects), new List<ZDO>(currentDistantObjects)));
                return false;
            }
            return true;
        }

    }
}
