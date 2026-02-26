using HarmonyLib;
using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Patches
{
    internal partial class AlwaysActivePatches
    {

        [HarmonyPatch(typeof(ZNet), "Start")]
        [HarmonyPostfix]
        private static void ZNet_Start_Postfix()
        {
            Net.NetworkSessionManager.InitSession();
        }

        [HarmonyPatch(typeof(ZNet), "RPC_PeerInfo")]
        [HarmonyPostfix]
        private static void ZNet_RPC_PeerInfo_Postfix()
        {
            if (ZNet.instance == null)
            {
                // wtf?
                Plugin.LogWarning("ZNet.instance is still null on ZNet.RPC_PeerInfo");
                return;
            }
            if (!ZNet.instance.IsServer())
            {
                Net.NetworkSessionManager.RequestHandshakeWithServer();
            }
        }

        [HarmonyPatch(typeof(ZNet), "OnDestroy")]
        [HarmonyPrefix]
        private static void ZNet_OnDestroy_Prefix()
        {
            Net.NetworkSessionManager.CloseSession();
        }

    }
}
