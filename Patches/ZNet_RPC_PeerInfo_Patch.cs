using HarmonyLib;
using OfTamingAndBreeding.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Patches
{
    [HarmonyPatch(typeof(ZNet), "RPC_PeerInfo")]
    static class ZNet_RPC_PeerInfo_Patch
    {
        static void Postfix()
        {
            if (ZNet.instance != null && !ZNet.instance.IsServer())
            {
                RPCContext.RequestHandshakeWithServer();
            }
        }
    }
}
