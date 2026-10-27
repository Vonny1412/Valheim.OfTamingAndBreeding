using OfTamingAndBreeding.Data.Cache;
using OfTamingAndBreeding.Utilities;
using OfTamingAndBreeding.Processing.Core;
using System.IO;
using UnityEngine;

namespace OfTamingAndBreeding.Network
{

    internal static partial class NetworkSessionManager
    {

        private class ClientSession
        {
            public bool UseCache = false;
            public string CacheFileName = null;
            public string CacheFileHash = null;
        }

        private static ClientSession clientSession = null;

        public static void InitClientSession()
        {
            Plugin.LogDebug($"Initializing Client Session");
            clientSession = new ClientSession();
        }

        public static void RegisterClientCallbacks()
        {

            HandshakeRPC.OnClientReceive((ZPackage inPkg) => {
                Plugin.LogDebug($"Handshake (response) RPC received from Server");

                if (clientSession == null)
                {
                    Plugin.LogFatal($"Client Session not initialized");
                    return false;
                }

                var proceed = inPkg.ReadInt();
                if (proceed == 0)
                {
                    Plugin.LogFatal($"Error on server side");
                    return false;
                }

                // cache

                clientSession.UseCache = inPkg.ReadBool();
                clientSession.CacheFileName = CacheManager.GetSafeCacheFileName(inPkg.ReadString());
                clientSession.CacheFileHash = inPkg.ReadString();

                Plugin.LogInfo($"Cache Info: useCache={clientSession.UseCache} file='{clientSession.CacheFileName}' hash='{clientSession.CacheFileHash}' version='{Plugin.Version}'");

                var requestCacheFile = true;
                if (clientSession.UseCache)
                {

                    var cacheFilePath = CacheManager.GetCacheYamlFile(clientSession.CacheFileName);
                    if (File.Exists(cacheFilePath))
                    {
                        var hash = CacheManager.ComputeSha256FileHash(cacheFilePath);
                        if (hash == clientSession.CacheFileHash)
                        {
                            if (CacheManager.LoadDataFromCache(File.ReadAllText(cacheFilePath)))
                            {
                                requestCacheFile = false;
                                var dataLoaded = DataProcessingManager.ProcessData();
                                if (dataLoaded)
                                {
                                    Plugin.LogInfo($"Loaded data from existing cache");
                                }
                                else
                                {
                                    OnSessionErrorCallback();
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            Plugin.LogInfo($"Cache Hash mismatch: {hash} != {clientSession.CacheFileHash} -> request new cache");
                            requestCacheFile = true;
                        }
                    }
                }
                else
                {
                    requestCacheFile = true;
                }

                if (requestCacheFile)
                {
                    Plugin.LogInfo($"Requesting cache RPC from server");
                    CacheRPC.RequestFromServer();
                }
                else
                {
                    OnSessionReadyCallback();
                }

                return true;
            });

            CacheRPC.OnClientReceive((ZPackage inPkg) => {
                Plugin.LogInfo($"Cache (response) RPC received from Server");

                if (clientSession == null)
                {
                    Plugin.LogFatal($"Client Session not initialized");
                    return false;
                }

                var proceed = inPkg.ReadInt();
                if (proceed == 0)
                {
                    Plugin.LogFatal($"Error on Server side");
                    return false;
                }

                var cacheContent = inPkg.ReadString();
                Plugin.LogInfo($"Cache size: {cacheContent?.Length ?? -1}");

                var receivedHash = CacheManager.ComputeSha256StringHash(cacheContent);
                if (receivedHash != clientSession.CacheFileHash)
                {
                    Plugin.LogFatal("Received cache hash does not match hash from server handshake");
                    return false;
                }

                if (clientSession.UseCache)
                {
                    var cacheFile = CacheManager.GetCacheYamlFile(clientSession.CacheFileName);
                    Plugin.LogInfo($"Writing cache file to: '{cacheFile}'");
                    File.WriteAllText(cacheFile, cacheContent);
                }

                var success = CacheManager.LoadDataFromCache(cacheContent) && DataProcessingManager.ProcessData();
                if (success)
                {
                    Plugin.LogInfo("Loaded data from received cache");
                    OnSessionReadyCallback();
                }
                else
                {
                    Plugin.LogFatal("Failed loading or registering data from received cache");
                    OnSessionErrorCallback();
                }

                return success;
            });

        }

        public static void RequestHandshakeWithServer()
        {
            // only called for clients
            if (isServer)
            {
                return;
            }
            Plugin.LogInfo($"Requesting handshake RPC from server");
            HandshakeRPC.RequestFromServer();
            StartClientTimeout(Plugin.Constants.ClientHandshakeTimeout);
        }

        public static void StartClientTimeout(float seconds)
        {
            if (clientTimeoutRoutine == null)
            {
                clientTimeoutRoutine = Plugin.Instance.StartCoroutine(RunClientTimeout(seconds));
            }
        }

        private static System.Collections.IEnumerator RunClientTimeout(float seconds)
        {
            float start = Time.time;
            while (Time.time - start < seconds)
            {
                if (DataProcessingManager.IsDataProcessed)
                {
                    clientTimeoutRoutine = null;
                    yield break;
                }
                yield return null;
            }
            clientTimeoutRoutine = null;
            OnSessionReadyCallback(); // no response from server -> server seems to be running without otab -> vanilla mode
        }

        private static void CancelClientTimeout()
        {
            if (clientTimeoutRoutine != null)
            {
                Plugin.Instance.StopCoroutine(clientTimeoutRoutine);
                clientTimeoutRoutine = null;
            }
        }

    }

}
