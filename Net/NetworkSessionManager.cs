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
                RunOnReadyCallback(true);
            });
            PrefabRegistryManager.Instance.OnReset(() => {
                serverDataLoaded = false;
            });
        }

        protected override void OnDestroy()
        {
            CloseSession();
        }

        //--------------------------------------------------

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

        public void OnReady(Action<bool> cb)
        {
            onSessionReady.Add(cb);
        }

        public void OnClosed(Action<bool> cb)
        {
            onSessionClosed.Add(cb);
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
        }

        public void RequestHandshakeWithServer()
        {
            // only called for clients
            RPCContext.RequestHandshakeWithServer();
            StartClientTimeout(5f);
        }

        public void CloseSession()
        {
            // called for client and server
            var wasServerDataLoaded = serverDataLoaded;

            PrefabRegistryManager.Instance.ResetRegistry();
            RPCContext.DestroySession();

            ZNetSceneContext.Clear();
            CancelClientTimeout();

            RunOnClosedCallback(wasServerDataLoaded);
        }

        public void StartClientTimeout(float seconds)
        {
            if (clientTimeoutRoutine == null)
            {
                clientTimeoutRoutine = Plugin.Instance.StartCoroutine(RunClientTimeout(seconds));
            }
        }

        public void CancelClientTimeout()
        {
            if (clientTimeoutRoutine != null)
            {
                Plugin.Instance.StopCoroutine(clientTimeoutRoutine);
                clientTimeoutRoutine = null;
            }
        }

        private void RunOnReadyCallback(bool isServerDataLoaded)
        {
            foreach (var cb in onSessionReady)
            {
                cb(isServerDataLoaded);
            }
        }

        private void RunOnClosedCallback(bool wasServerDataLoaded)
        {
            foreach (var cb in onSessionClosed)
            {
                cb(wasServerDataLoaded);
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
            RunOnReadyCallback(false);
        }

    }
}
