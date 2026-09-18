using HarmonyLib;
using OfTamingAndBreeding.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace OfTamingAndBreeding.Patches
{
    internal partial class AlwaysActivePatches
    {
        // IMPORTANT: We must not let ZNetScene instantiate network objects before OTAB server data is applied.
        // Otherwise components (Awake/Start) would run with wrong vanilla values.
        // We therefore defer CreateObjects until DataOrchestrator marks dataLoaded == true.

        [HarmonyPatch(typeof(ZNetScene), "CreateObjects")]
        [HarmonyPrefix]
        private static bool ZNetScene_CreateObjects_Prefix(/* ZNetScene __instance, */ List<ZDO> currentNearObjects, List<ZDO> currentDistantObjects)
        {
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
            Network.NetworkSessionManager.CloseSession();
        }






    }
}
