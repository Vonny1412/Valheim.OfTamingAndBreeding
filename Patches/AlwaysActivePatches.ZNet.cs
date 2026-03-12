using HarmonyLib;

namespace OfTamingAndBreeding.Patches
{
    internal partial class AlwaysActivePatches
    {

        [HarmonyPatch(typeof(ZNet), "Start")]
        [HarmonyPostfix]
        private static void ZNet_Start_Postfix()
        {
            Net.NetworkSessionManager.Instance.StartSession();
        }

        [HarmonyPatch(typeof(ZNet), "RPC_PeerInfo")]
        [HarmonyPostfix]
        private static void ZNet_RPC_PeerInfo_Postfix()
        {
            if (ZNet.instance == null)
            {
                // wtf?
                Plugin.LogError("ZNet.instance is still null on ZNet.RPC_PeerInfo");
                return;
            }
            // using ZNet.RPC_PeerInfo as anchor because
            // we need to handshake AFTER client login on server
            if (!ZNet.instance.IsServer())
            {
                Net.NetworkSessionManager.Instance.RequestHandshakeWithServer();
            }
        }
        
    }
}
