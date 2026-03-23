using Jotunn;
using OfTamingAndBreeding.Registry;
using OfTamingAndBreeding.StaticContext;
using System;
using UnityEngine;

namespace OfTamingAndBreeding.Net
{
    internal class NetworkSessionManager : Common.SingletonClass<NetworkSessionManager>
    {

        //--------------------------------------------------
        // Singleton

        protected override void OnCreate()
        {
            PrefabRegistryManager.Instance.OnRegistrationFinished += () => {
                CancelClientTimeout();
                OnSessionReady?.Invoke(Instance, true);
            };
            PrefabRegistryManager.Instance.OnReset += () => {
                // todo: remove me if unneccessary
            };
        }

        protected override void OnDestroy()
        {
            Plugin.LogFatal($"NetworkSessionManager.Instance has been destroyed");
            CloseSession();
        }

        //--------------------------------------------------

        public event Action<NetworkSessionManager> OnSessionStarted;
        public event Action<NetworkSessionManager, bool> OnSessionReady;
        public event Action<NetworkSessionManager, bool> OnSessionClosed;

        private bool isServer = false;
        private Coroutine clientTimeoutRoutine;

        public bool IsServer()
        {
            return isServer;
        }

        public void StartSession()
        {
            var zn = ZNet.instance;
            var isLocal = zn.IsLocalInstance();
            isServer = zn.IsServer();

            OnSessionStarted?.Invoke(Instance);

            if (isServer)
            {
                RPCContext.InitServerSession(isLocal);
            }
            else
            {
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
            Plugin.LogServerInfo($"Closing session");

            // called for client and server
            var wasServerDataLoaded = PrefabRegistryManager.Instance.IsDataLoaded();

            PrefabRegistryManager.Instance.ResetRegistry();
            RPCContext.DestroySession();

            ZNetSceneContext.Clear();
            CancelClientTimeout();

            Plugin.LogServerInfo($"Session closed");
            OnSessionClosed?.Invoke(Instance, wasServerDataLoaded);
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
                if (PrefabRegistryManager.Instance.IsDataLoaded())
                {
                    yield break;
                }
                yield return null;
            }
            clientTimeoutRoutine = null;
            OnSessionReady?.Invoke(Instance, false);
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
