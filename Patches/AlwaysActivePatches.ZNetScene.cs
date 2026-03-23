using HarmonyLib;
using OfTamingAndBreeding.StaticContext;
using System.Collections.Generic;

namespace OfTamingAndBreeding.Patches
{
    internal partial class AlwaysActivePatches
    {

        [HarmonyPatch(typeof(ZNetScene), "CreateObjects")]
        [HarmonyPrefix]
        private static bool ZNetScene_CreateObjects_Prefix(/* ZNetScene __instance, */ List<ZDO> currentNearObjects, List<ZDO> currentDistantObjects)
        {
            // IMPORTANT: We must not let ZNetScene instantiate network objects before OTAB server data is applied.
            // Otherwise components (Awake/Start) would run with wrong vanilla values.
            // We therefore defer CreateObjects until DataOrchestrator marks dataLoaded == true.

            if (ZNetSceneContext.IsBlocking())
            {
                ZNetSceneContext.Enqueue(new List<ZDO>(currentNearObjects), new List<ZDO>(currentDistantObjects));
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(ZNetScene), "Shutdown")]
        [HarmonyPrefix]
        private static void ZNetScene_Shutdown_Prefix()
        {
            Net.NetworkSessionManager.Instance.CloseSession();
        }

    }
}
