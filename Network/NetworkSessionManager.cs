using OfTamingAndBreeding.Components.Core;
using OfTamingAndBreeding.Processing.Core;
using OfTamingAndBreeding.Processing.Registry;
using System;
using System.IO;
using UnityEngine;

namespace OfTamingAndBreeding.Network
{
    internal static partial class NetworkSessionManager
    {

        private static BaseRPC HandshakeRPC;
        private static BaseRPC CacheRPC;

        private static Coroutine clientTimeoutRoutine;



        public static event Func<bool> OnSessionValidate;
        public static event Action OnSessionStarted;
        public static event Action OnSessionReady;
        public static event Action OnSessionError;
        public static event Action OnSessionClosed;









        private static bool OnSessionValidateCallback()
        {
            if (OnSessionValidate != null)
            {
                foreach (Func<bool> validator in OnSessionValidate.GetInvocationList())
                {
                    if (!validator())
                    {
                        return false;
                    }
                }
            }
            return true;
        }


        private static void OnSessionStartedCallback()
        {
            Runtime.ZNetSceneContext.Block();
            if (Network.NetworkSessionManager.IsServer() && Plugin.Configs.DumpPrefabsToCache.Value == true)
            {
                PrefabUtils.DumpPrefabs(Path.Combine(Plugin.CacheDir, "_prefabs"));
            }
            OnSessionStarted?.Invoke();
        }

        private static void OnSessionReadyCallback()
        {
            OTABComponentTypeRegistry.AddComponentsToPrefabs();

            if (DataProcessingManager.IsDataLoaded)
            {
                foreach (var p in DataProcessingManager.DataProcessors)
                {
                    Plugin.LogInfo($"Loaded {p.GetLoadedDataCount()} {p.ModelTypeName} entries");
                }
                Patches.DataReadyPatches.Install();
            }
            else
            {
                Plugin.LogInfo("No server sync detected (timeout). Running in vanilla mode.");
            }

            Runtime.ZNetSceneContext.Unblock();
            OnSessionReady?.Invoke();
        }

        private static void OnSessionErrorCallback()
        {
            static void Logout()
            {
                Runtime.ZNetSceneContext.Clear();
                Game.instance.Logout(save: true, changeToStartScene: true);
                // save:true is required to trigger Game.Shutdown() and properly disconnect the server.
            }
            if (UnifiedPopup.IsAvailable())
            {
                UnifiedPopup.Push(
                    new WarningPopup(
                        "Of Taming And Breeding",
                        "There was an error loading OTAB data. Please check your LogOutput.log for details.",
                        () => Logout(),
                        false
                    )
                );
            }
            else
            {
                Logout();
            }
            OnSessionError?.Invoke();
        }

        private static void OnSessionClosedCallback()
        {
            Patches.DataReadyPatches.Uninstall();
            OTABComponentTypeRegistry.RemoveComponentsFromPrefabs();
            OnSessionClosed?.Invoke();
        }









        private static bool isServer = false;
        public static bool IsServer()
        {
            return isServer;
        }

        private static bool isAdmin = false;
        internal static bool IsAdmin()
        {
            return isAdmin;
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
            var znet = ZNet.instance;
            //var isLocal = znet.IsLocalInstance();
            isServer = znet.IsServer();
            isAdmin = znet.LocalPlayerIsAdminOrHost();

            if (!OnSessionValidateCallback())
            {
                OnSessionErrorCallback();
                return;
            }

            OnSessionStartedCallback();
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

            isServer = false;
            isAdmin = false;

            Runtime.ZNetSceneContext.Clear();
            CancelClientTimeout();

            OnSessionClosedCallback();
            Plugin.LogInfo($"Session closed");
        }

    }

}
