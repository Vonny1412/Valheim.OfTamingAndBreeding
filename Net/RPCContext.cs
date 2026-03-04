using System;

namespace OfTamingAndBreeding.Net
{
    internal partial class RPCContext
    {

        private static BaseRPC HandshakeRPC;
        private static BaseRPC CacheRPC;

        public static void RegisterRPCs()
        {
            HandshakeRPC = new BaseRPC("OTAB_InitRPC");
            CacheRPC = new BaseRPC("OTAB_CacheRPC");
            RegisterServerCallbacks();
            RegisterClientCallbacks();
        }

        private static ServerSession serverSession = null;
        private static ClientSession clientSession = null;

        public static void DestroySession()
        {
            CacheRPC.ResetState();
            HandshakeRPC.ResetState();
            serverSession = null;
            clientSession = null;
        }

    }
}
