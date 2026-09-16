using HarmonyLib;

namespace OfTamingAndBreeding.Patches
{
    internal partial class AlwaysActivePatches
    {

        [HarmonyPatch(typeof(ZNet), "Start")]
        [HarmonyPostfix]
        private static void ZNet_Start_Postfix()
        {
            Network.NetworkSessionManager.StartSession();
        }

        [HarmonyPatch(typeof(ZNet), "RPC_PeerInfo")]
        [HarmonyPostfix]
        private static void ZNet_RPC_PeerInfo_Postfix()
        {
            // using ZNet.RPC_PeerInfo as anchor because
            // we need to handshake AFTER client login on server
            Network.NetworkSessionManager.RequestHandshakeWithServer();
        }
        
    }
}
