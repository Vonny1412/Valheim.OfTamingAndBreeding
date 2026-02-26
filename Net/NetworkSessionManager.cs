using Jotunn;
using OfTamingAndBreeding.Components.Extensions;
using OfTamingAndBreeding.Registry;
using OfTamingAndBreeding.StaticContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OfTamingAndBreeding.Net
{
    internal static class NetworkSessionManager
    {

        private static Coroutine clientTimeoutRoutine;

        public static void InitSession()
        {
            var zn = ZNet.instance;
            var isLocal = zn.IsLocalInstance();
            if (zn.IsServer())
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

        public static void RequestHandshakeWithServer()
        {
            // only called for clients
            RPCContext.RequestHandshakeWithServer();
            StartClientTimeout(5f);
        }

        public static void CloseSession()
        {
            // called for client and server

            RegistryOrchestrator.ResetData();
            RPCContext.DestroySession();

            ZNetSceneContext.Clear();
            CancelClientTimeout();
        }

        public static void StartClientTimeout(float seconds)
        {
            if (clientTimeoutRoutine == null)
            {
                clientTimeoutRoutine = Plugin.Instance.StartCoroutine(RunClientTimeout(seconds));
            }
        }

        public static void CancelClientTimeout()
        {
            if (clientTimeoutRoutine != null)
            {
                Plugin.Instance.StopCoroutine(clientTimeoutRoutine);
                clientTimeoutRoutine = null;
            }
        }

        private static System.Collections.IEnumerator RunClientTimeout(float seconds)
        {
            float start = Time.time;
            while (Time.time - start < seconds)
            {
                if (Plugin.otabDataLoaded)
                {
                    yield break;
                }
                yield return null;
            }
            Plugin.LogInfo("No server sync detected (timeout). Running in vanilla mode.");
            ZNetScene.instance?.UnblockObjectsCreation();
            Plugin.otabDataLoaded = false;
        }

    }
}
