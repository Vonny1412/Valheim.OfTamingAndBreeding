using OfTamingAndBreeding.Data;
using OfTamingAndBreeding.Registry;
using OfTamingAndBreeding.OTABUtils;
using System.IO;

namespace OfTamingAndBreeding.Net
{
    internal partial class RPCContext
    {

        private class ClientSession
        {
            public bool UseCache = false;
            public string CacheFileName = null;
            public string CacheFileHash = null;
            public string CacheCryptKey = null;
        }

        public static void InitClientSession(bool isLocal)
        {
            var inFunc1 = $"{nameof(RPCContext)}.{nameof(RPCContext.InitClientSession)}";
            Plugin.LogDebug($"[{inFunc1}] Start");

            clientSession = new ClientSession();

            Plugin.LogDebug($"[{inFunc1}] Done");
        }

        public static void RegisterClientCallbacks()
        {

            HandshakeRPC.OnClientReceive((ZPackage inPkg) => {
                var inFunc2 = $"{nameof(HandshakeRPC)}.{nameof(HandshakeRPC.OnClientReceive)}";

                Plugin.LogDebug($"[{inFunc2}] Client received response");

                if (clientSession == null)
                {
                    Plugin.LogDebug($"[{inFunc2}] Client session not initialized");
                    return false;
                }

                var proceed = inPkg.ReadInt();
                if (proceed == 0)
                {
                    Plugin.LogFatal($"[{inFunc2}] Error on server side");
                    return false;
                }

                // cache

                clientSession.UseCache = inPkg.ReadBool();
                clientSession.CacheFileName = inPkg.ReadString();
                clientSession.CacheFileHash = inPkg.ReadString();

                var obf = inPkg.ReadString();
                if (obf.Trim() == "")
                {
                    obf = null;
                }

                clientSession.CacheCryptKey = obf == null ? null : KeyMask.Deobfuscate(obf, $"{clientSession.CacheFileName}|{Plugin.Version}");
                Plugin.LogDebug($"[{inFunc2}] useCache={clientSession.UseCache} file='{clientSession.CacheFileName}' hash='{clientSession.CacheFileHash}' obfLen={obf?.Length ?? -1} keyLen={clientSession.CacheCryptKey?.Length ?? -1} version='{Plugin.Version}'");

                var requestCacheFile = true;
                if (clientSession.UseCache)
                {

                    var cacheFilePath = CacheManager.GetCacheCryptedFile(clientSession.CacheFileName);
                    if (File.Exists(cacheFilePath))
                    {
                        var hash = CacheManager.ComputeSha256FileHash(cacheFilePath);
                        if (hash == clientSession.CacheFileHash)
                        {
                            if (CacheManager.Instance.LoadCacheFromCrypted(File.ReadAllText(cacheFilePath), clientSession.CacheCryptKey))
                            {
                                requestCacheFile = false;
                                PrefabRegistryManager.Instance.ValidateDataAndRegisterPrefabs();
                                Plugin.LogInfo($"Loaded data from existing cache");
                            }
                        }
                        else
                        {
                            Plugin.LogDebug($"[{inFunc2}] Hash mismatch: {hash} != {clientSession.CacheFileHash} -> request new cache");
                        }
                    }
                }
                else
                {
                    requestCacheFile = true;
                }

                if (requestCacheFile)
                {
                    Plugin.LogDebug($"[{inFunc2}] requestCacheFile={requestCacheFile} UseCache={clientSession.UseCache} cacheFileName={clientSession.CacheFileName}");
                    CacheRPC.RequestFromServer();
                }

                return true;
            });

            CacheRPC.OnClientReceive((ZPackage inPkg) => {
                var inFunc2 = $"{nameof(CacheRPC)}.{nameof(CacheRPC.OnClientReceive)}";

                Plugin.LogDebug($"[{inFunc2}] Client received response");

                if (clientSession == null)
                {
                    Plugin.LogDebug($"[{inFunc2}] Client session not initialized");
                    return false;
                }

                var proceed = inPkg.ReadInt();
                if (proceed == 0)
                {
                    Plugin.LogFatal($"[{inFunc2}] Error on server side");
                    return false;
                }

                var cacheContent = inPkg.ReadString();
                Plugin.LogDebug($"[{inFunc2}] contentLen={cacheContent?.Length ?? -1} UseCache={clientSession.UseCache} file='{clientSession.CacheFileName}'");

                if (clientSession.UseCache)
                {
                    var cacheFile = CacheManager.GetCacheCryptedFile(clientSession.CacheFileName);
                    Plugin.LogInfo($"Writing cache file to: '{cacheFile}'");
                    File.WriteAllText(cacheFile, cacheContent);
                }

                var success = CacheManager.Instance.LoadCacheFromCrypted(cacheContent, clientSession.CacheCryptKey);

                if (success)
                {
                    PrefabRegistryManager.Instance.ValidateDataAndRegisterPrefabs();
                    Plugin.LogInfo($"Loaded data from received cache");
                }
                else
                {
                    Plugin.LogFatal($"Failed loading data from received cache");
                }
                return success;
            });

        }

        public static void RequestHandshakeWithServer()
        {
            var inFunc = $"{nameof(RPCContext)}.{nameof(RPCContext.RequestHandshakeWithServer)}";

            Plugin.LogDebug($"[{inFunc}] Requesting handshake from server");
            HandshakeRPC.RequestFromServer();
        }

    }
}
