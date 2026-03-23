using OfTamingAndBreeding.Data;
using OfTamingAndBreeding.Registry;
using OfTamingAndBreeding.OTABUtils;
using System;

namespace OfTamingAndBreeding.Net
{
    internal partial class RPCContext
    {

        private class ServerSession
        {
            public string CacheFileName = null;
            public string CacheFileHash = null;
            public string CacheCryptKey = null;
            public string CacheContent = null;
        }

        public static void InitServerSession(bool isLocal)
        {
            var inFunc1 = $"{nameof(RPCContext)}.{nameof(RPCContext.InitServerSession)}";
            Plugin.LogServerDebug($"[{inFunc1}] Start");

            var dataLoaded = PrefabRegistryManager.Instance.LoadDataFromLocalFiles();
            if (!dataLoaded)
            {
                DestroySession();
                return;
            }

            var writeCacheFiles = Plugin.Configs.WriteServerCacheFiles.Value;

            serverSession = new ServerSession
            {
                CacheFileName = Plugin.Configs.CacheFileName.Value
                    .Replace("{world}", ZNet.instance.GetWorldName())
                    .Replace("{seed}", ZNet.World.m_seedName),

                CacheCryptKey = Plugin.Configs.CacheFileCryptKey.Value
            };
            if (serverSession.CacheCryptKey.Trim() == "")
            {
                serverSession.CacheCryptKey = null;
            }

            if (isLocal)
            {
                if (writeCacheFiles)
                {
                    CacheManager.Instance.BuildCache(serverSession.CacheFileName, serverSession.CacheCryptKey, true);
                }
            }
            else
            {

                Plugin.LogServerDebug($"[{inFunc1}] Server cache settings: WriteClientCacheFile={Plugin.Configs.WriteClientCacheFile.Value} CacheFileName='{serverSession.CacheFileName}' KeyLen={(serverSession.CacheCryptKey?.Length ?? -1)}");

                Plugin.LogServerDebug($"[{inFunc1}] Cache build: WriteCache #1");
                var hash1 = CacheManager.Instance.BuildCache(serverSession.CacheFileName, serverSession.CacheCryptKey, false, out serverSession.CacheContent);
                Plugin.LogServerDebug($"[{inFunc1}] Cache build: LoadCacheFromCrypted");
                if (!CacheManager.Instance.LoadCacheFromCrypted(serverSession.CacheContent, serverSession.CacheCryptKey))
                {
                    Plugin.LogFatal($"[{inFunc1}] Failed building cache: Cache #1 corrupted");
                    PrefabRegistryManager.Instance.ResetRegistry();
                    DestroySession();
                    return;
                }
                Plugin.LogServerDebug($"[{inFunc1}] Cache build: WriteCache #2");
                var hash2 = CacheManager.Instance.BuildCache(serverSession.CacheFileName, serverSession.CacheCryptKey, writeCacheFiles, out serverSession.CacheContent);
                Plugin.LogServerDebug($"[{inFunc1}] Cache hash1={hash1} hash2={hash2} match={hash1 == hash2}");

                if (hash1 == hash2)
                {
                    serverSession.CacheFileHash = hash1;
                }
                else
                {
                    Plugin.LogFatal($"[{inFunc1}] Failed building cache: Hashes mismatch");
                    PrefabRegistryManager.Instance.ResetRegistry();
                    DestroySession();
                }

            }

            Plugin.LogServerDebug($"[{inFunc1}] Done");
        }

        public static void RegisterServerCallbacks()
        {

            HandshakeRPC.OnServerReceive((ZPackage inPkg, ZPackage outPkg) => {
                var inFunc2 = $"{nameof(HandshakeRPC)}.{nameof(HandshakeRPC.OnServerReceive)}";

                Plugin.LogServerDebug($"[{inFunc2}] Server received handshake request");

                if (serverSession == null)
                {
                    // this method should not be called when cacheContent==null
                    // but maybe i gonna change that behavior in future release
                    Plugin.LogServerDebug($"[{inFunc2}] Server session not initialized");
                    outPkg.Write(0);
                }
                else
                {
                    outPkg.Write(1);

                    // cache

                    outPkg.Write(Plugin.Configs.WriteClientCacheFile.Value); // usecache
                    outPkg.Write(serverSession.CacheFileName);
                    outPkg.Write(serverSession.CacheFileHash);

                    string obf = null;
                    if (serverSession.CacheCryptKey != null)
                    {
                        obf = KeyMask.Obfuscate(serverSession.CacheCryptKey, $"{serverSession.CacheFileName}|{Plugin.Version}");
                    }
                    Plugin.LogServerInfo($"Handshaking: UseCache={Plugin.Configs.WriteClientCacheFile.Value} File='{serverSession.CacheFileName}' Hash='{serverSession.CacheFileHash}' ObfLen={obf?.Length ?? -1}");
                    outPkg.Write(obf ?? "");

                }
                return true;
            });

            CacheRPC.OnServerReceive((ZPackage inPkg, ZPackage outPkg) => {
                var inFunc2 = $"{nameof(CacheRPC)}.{nameof(CacheRPC.OnServerReceive)}";

                if (serverSession == null)
                {
                    // this method should not be called when cacheContent==null
                    // but maybe i gonna change that behavior in future release
                    Plugin.LogServerDebug($"[{inFunc2}] Server session not initialized");
                    outPkg.Write(0);
                }
                else
                {
                    outPkg.Write(1);

                    Plugin.LogServerInfo($"Sending Cache: cacheContentLen={serverSession.CacheContent?.Length ?? -1}");
                    outPkg.Write(serverSession.CacheContent);
                }

                return true;
            });

            CacheRPC.SetServerReady();
            HandshakeRPC.SetServerReady();
        }

    }
}
