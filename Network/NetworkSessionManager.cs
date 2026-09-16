using Jotunn;
using OfTamingAndBreeding.Processing.Core;
using OfTamingAndBreeding.Runtime;
using System;
using UnityEngine;

namespace OfTamingAndBreeding.Network
{
    internal static partial class NetworkSessionManager
    {

        private static BaseRPC HandshakeRPC;
        private static BaseRPC CacheRPC;

        public static event Action OnSessionStarted;
        public static event Action OnSessionReady;
        public static event Action OnSessionError;
        public static event Action OnSessionClosed;

        private static bool isServer = false;
        private static Coroutine clientTimeoutRoutine;

        public static bool IsServer()
        {
            return isServer;
        }

        public static void RegisterRPCs()
        {
            HandshakeRPC = new BaseRPC("OTAB_InitRPC");
            CacheRPC = new BaseRPC("OTAB_CacheRPC");
            RegisterServerCallbacks();
            RegisterClientCallbacks();
        }

        public static void StartSession()
        {
            var zn = ZNet.instance;
            var isLocal = zn.IsLocalInstance();
            isServer = zn.IsServer();

            OnSessionStarted?.Invoke();

            if (isServer)
            {
                InitServerSession();
            }
            else
            {
                InitClientSession();
            }
        }

        public static void CloseSession()
        {
            Plugin.LogInfo($"Closing session");

            DataProcessingManager.ResetRegistry();
            CacheRPC.ResetState();
            HandshakeRPC.ResetState();
            serverSession = null;
            clientSession = null;

            ZNetSceneContext.Clear();
            CancelClientTimeout();

            Plugin.LogInfo($"Session closed");
            OnSessionClosed?.Invoke();
        }

    }

}
