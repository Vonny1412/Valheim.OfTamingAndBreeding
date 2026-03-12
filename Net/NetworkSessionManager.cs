using Jotunn;
using OfTamingAndBreeding.Registry;
using OfTamingAndBreeding.StaticContext;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OfTamingAndBreeding.Net
{
    internal class NetworkSessionManager : Common.SingletonClass<NetworkSessionManager>
    {

        //--------------------------------------------------
        // Singleton

        protected override void OnCreate()
        {
            PrefabRegistryManager.Instance.OnFinished(() => {
                serverDataLoaded = true;
                CancelClientTimeout();
                RunOnReadyCallbacks(true);
            });
            PrefabRegistryManager.Instance.OnReset(() => {
                serverDataLoaded = false;
            });
        }

        protected override void OnDestroy()
        {
            Plugin.LogFatal($"NetworkSessionManager.Instance has been destroyed");
            CloseSession();
        }

        //--------------------------------------------------

        private readonly List<Action> onSessionStarted = new List<Action>();
        private readonly List<Action<bool>> onSessionReady = new List<Action<bool>>();
        private readonly List<Action<bool>> onSessionClosed = new List<Action<bool>>();

        private bool serverDataLoaded = false;
        private bool isServer = false;
        private Coroutine clientTimeoutRoutine;

        public bool IsServerDataLoaded()
        {
            return serverDataLoaded;
        }

        public bool IsServer()
        {
            return isServer;
        }

        public void OnStarted(Action cb)
        {
            onSessionStarted.Add(cb);
        }

        public void OnReady(Action<bool> cb)
        {
            onSessionReady.Add(cb);
        }

        public void OnClosed(Action<bool> cb)
        {
            onSessionClosed.Add(cb);
        }

        private void RunOnStartedCallbacks()
        {
            foreach (var cb in onSessionStarted)
            {
                cb();
            }
        }

        private void RunOnReadyCallbacks(bool isServerDataLoaded)
        {
            foreach (var cb in onSessionReady)
            {
                cb(isServerDataLoaded);
            }
        }

        private void RunOnClosedCallbacks(bool wasServerDataLoaded)
        {
            foreach (var cb in onSessionClosed)
            {
                cb(wasServerDataLoaded);
            }
        }

        public void StartSession()
        {
            var zn = ZNet.instance;
            var isLocal = zn.IsLocalInstance();
            isServer = zn.IsServer();
            if (isServer)
            {
                ZNetSceneContext.blockObjectsCreation = false;
                RPCContext.InitServerSession(isLocal);
            }
            else
            {
                ZNetSceneContext.blockObjectsCreation = true;
                RPCContext.InitClientSession(isLocal);
            }
            RunOnStartedCallbacks();
        }

        public void RequestHandshakeWithServer()
        {
            // only called for clients
            RPCContext.RequestHandshakeWithServer();
            StartClientTimeout(5f);
        }

        public void CloseSession()
        {
            Plugin.LogServerInfo($"Closing session");

            // called for client and server
            var wasServerDataLoaded = serverDataLoaded;

            PrefabRegistryManager.Instance.ResetRegistry();
            RPCContext.DestroySession();

            ZNetSceneContext.Clear();
            CancelClientTimeout();

            Plugin.LogServerInfo($"Session closed");
            RunOnClosedCallbacks(wasServerDataLoaded);
        }

        public void StartClientTimeout(float seconds)
        {
            if (clientTimeoutRoutine == null)
            {
                clientTimeoutRoutine = Plugin.Instance.StartCoroutine(RunClientTimeout(seconds));
            }
        }

        private System.Collections.IEnumerator RunClientTimeout(float seconds)
        {
            float start = Time.time;
            while (Time.time - start < seconds)
            {
                if (serverDataLoaded)
                {
                    yield break;
                }
                yield return null;
            }
            clientTimeoutRoutine = null;
            RunOnReadyCallbacks(false);
        }

        public void CancelClientTimeout()
        {
            if (clientTimeoutRoutine != null)
            {
                Plugin.Instance.StopCoroutine(clientTimeoutRoutine);
                clientTimeoutRoutine = null;
            }
        }

    }
}
