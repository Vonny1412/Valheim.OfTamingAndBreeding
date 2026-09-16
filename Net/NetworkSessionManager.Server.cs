using OfTamingAndBreeding.Data;
using OfTamingAndBreeding.OTABUtils;
using OfTamingAndBreeding.Registry;
using System;

namespace OfTamingAndBreeding.Net
{

    internal static partial class NetworkSessionManager
    {

        private class ServerSession
        {
            public string CacheFileName = null;
            public string CacheFileHash = null;
            public string CacheCryptKey = null;
            public string CacheContent = null;
        }

        private static ServerSession serverSession = null;

        public static void InitServerSession()
        {
            Plugin.LogInfo($"Initializing Server Session");

            var dataLoaded = DataProcessingManager.LoadDataFromLocalFiles();
            if (!dataLoaded)
            {
                Plugin.LogFatal($"Error in Data files found - Aborting");
                OnSessionError?.Invoke();
                return;
            }

            var writeCacheFiles = Plugin.Configs.WriteServerCacheFiles.Value;

            var resolvedCacheFileName = Plugin.Configs.CacheFileName.Value
                .Replace("{world}", ZNet.instance.GetWorldName())
                .Replace("{seed}", ZNet.World.m_seedName);

            serverSession = new ServerSession
            {
                CacheFileName = CacheManager.GetSafeCacheFileName(resolvedCacheFileName),
                CacheCryptKey = SecurityUtils.GenerateCryptKey()
            };

            Plugin.LogInfo($"WriteClientCacheFile={Plugin.Configs.WriteClientCacheFile.Value} CacheFileName='{serverSession.CacheFileName}' KeyLen={(serverSession.CacheCryptKey?.Length ?? -1)}");

            Plugin.LogInfo($"Building cache #1");
            var hash1 = CacheManager.BuildCache(serverSession.CacheFileName, serverSession.CacheCryptKey, false, out serverSession.CacheContent);
            Plugin.LogInfo($"Test-loading cache #1");
            if (!CacheManager.LoadCacheFromCrypted(serverSession.CacheContent, serverSession.CacheCryptKey))
            {
                Plugin.LogFatal($"Cache #1 corrupted");
                DataProcessingManager.ResetRegistry();
                OnSessionError?.Invoke();
                return;
            }
            Plugin.LogInfo($"Building cache #2");
            var hash2 = CacheManager.BuildCache(serverSession.CacheFileName, serverSession.CacheCryptKey, writeCacheFiles, out serverSession.CacheContent);
            Plugin.LogInfo($"Cache hash1={hash1} hash2={hash2} match={hash1 == hash2}");

            if (hash1 == hash2)
            {
                serverSession.CacheFileHash = hash1;
            }
            else
            {
                Plugin.LogFatal($"Cache Hashes mismatch");
                DataProcessingManager.ResetRegistry();
                OnSessionError?.Invoke();
                return;
            }

            if (DataProcessingManager.ValidateDataAndRegisterPrefabs())
            {
                Plugin.LogInfo($"Cache is ready");
                OnSessionReady?.Invoke();
            }
            else
            {
                Plugin.LogFatal($"Error in cache data");
                OnSessionError?.Invoke();
            }
        }

        public static void RegisterServerCallbacks()
        {

            HandshakeRPC.OnServerReceive((ZPackage inPkg, ZPackage outPkg) => {
                Plugin.LogInfo($"Handshake (request) RPC received from client");

                if (serverSession == null)
                {
                    // this method should not be called when cacheContent==null
                    // but maybe i gonna change that behavior in future release
                    Plugin.LogFatal($"Server session not initialized");
                    outPkg.Write(0);
                }
                else
                {
                    outPkg.Write(1);

                    // cache

                    outPkg.Write(Plugin.Configs.WriteClientCacheFile.Value); // usecache
                    outPkg.Write(serverSession.CacheFileName);
                    outPkg.Write(serverSession.CacheFileHash);

                    string obf = SecurityUtils.Obfuscate(serverSession.CacheCryptKey, $"{serverSession.CacheFileName}|{Plugin.Version}");
                    Plugin.LogInfo($"Handshaking: UseCache={Plugin.Configs.WriteClientCacheFile.Value} File='{serverSession.CacheFileName}' Hash='{serverSession.CacheFileHash}' ObfLen={obf?.Length ?? -1}");
                    outPkg.Write(obf);

                }
                return true;
            });

            CacheRPC.OnServerReceive((ZPackage inPkg, ZPackage outPkg) => {
                Plugin.LogInfo($"Cache (request) RPC received from client");

                if (serverSession == null)
                {
                    // this method should not be called when cacheContent==null
                    // but maybe i gonna change that behavior in future release
                    Plugin.LogFatal($"Server session not initialized");
                    outPkg.Write(0);
                }
                else
                {
                    Plugin.LogInfo($"Sending Cache: cacheContentLen={serverSession.CacheContent?.Length ?? -1}");
                    outPkg.Write(1);
                    outPkg.Write(serverSession.CacheContent);
                }

                return true;
            });

            CacheRPC.SetServerReady();
            HandshakeRPC.SetServerReady();
        }

    }

}
