using OfTamingAndBreeding.Data;
using OfTamingAndBreeding.Utils;
using OfTamingAndBreeding.Registry;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OfTamingAndBreeding.Net
{
    internal static class RPCContext
    {

        private static BaseRPC HandshakeRPC;
        private static BaseRPC CacheRPC;

        public static void RegisterRPCs()
        {
            HandshakeRPC = new BaseRPC("OTAB_InitRPC");
            CacheRPC = new BaseRPC("OTAB_CacheRPC");
            RegisterServerCallbacks();
            RegisterClientCallbacks();
        }

        private static ServerSession serverSession = null;
        private static ClientSession clientSession = null;

        public static void DestroySession()
        {
            CacheRPC.ResetState();
            HandshakeRPC.ResetState();
            serverSession = null;
            clientSession = null;
        }

        #region Server

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
            Plugin.LogDebug($"[{inFunc1}] Start");

            RegistryOrchestrator.LoadDataFromLocalFiles();
            if (RegistryOrchestrator.IsDataLoaded() == false)
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
                    CacheManager.BuildCache(serverSession.CacheFileName, serverSession.CacheCryptKey, true);
                }
            }
            else
            {

                Plugin.LogDebug($"[{inFunc1}] Server cache settings: WriteClientCacheFile={Plugin.Configs.WriteClientCacheFile.Value} CacheFileName='{serverSession.CacheFileName}' KeyLen={(serverSession.CacheCryptKey?.Length ?? -1)}");

                Plugin.LogDebug($"[{inFunc1}] Cache build: WriteCache #1");
                var hash1 = CacheManager.BuildCache(serverSession.CacheFileName, serverSession.CacheCryptKey, false, out serverSession.CacheContent);
                Plugin.LogDebug($"[{inFunc1}] Cache build: LoadCacheFromCrypted");
                if (!CacheManager.LoadCacheFromCrypted(serverSession.CacheContent, serverSession.CacheCryptKey))
                {
                    Plugin.LogFatal($"[{inFunc1}] Failed building cache: Cache #1 corrupted");
                    RegistryOrchestrator.ResetData();
                    DestroySession();
                    return;
                }
                Plugin.LogDebug($"[{inFunc1}] Cache build: WriteCache #2");
                var hash2 = CacheManager.BuildCache(serverSession.CacheFileName, serverSession.CacheCryptKey, writeCacheFiles, out serverSession.CacheContent);
                Plugin.LogDebug($"[{inFunc1}] Cache hash1={hash1} hash2={hash2} match={hash1 == hash2}");

                if (hash1 == hash2)
                {
                    serverSession.CacheFileHash = hash1;
                }
                else
                {
                    Plugin.LogFatal($"[{inFunc1}] Failed building cache: Hashes mismatch");
                    RegistryOrchestrator.ResetData();
                    DestroySession();
                }

            }

            Plugin.LogDebug($"[{inFunc1}] Done");
        }

        public static void RegisterServerCallbacks()
        {

            HandshakeRPC.OnServerReceive((ZPackage inPkg, ZPackage outPkg) => {
                var inFunc2 = $"{nameof(HandshakeRPC)}.{nameof(HandshakeRPC.OnServerReceive)}";

                Plugin.LogDebug($"[{inFunc2}] Server received handshake request");

                if (serverSession == null)
                {
                    // this method should not be called when cacheContent==null
                    // but maybe i gonna change that behavior in future release
                    Plugin.LogDebug($"[{inFunc2}] Server session not initialized");
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
                    Plugin.LogInfo($"Handshaking: UseCache={Plugin.Configs.WriteClientCacheFile.Value} File='{serverSession.CacheFileName}' Hash='{serverSession.CacheFileHash}' ObfLen={obf?.Length ?? -1}");
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
                    Plugin.LogDebug($"[{inFunc2}] Server session not initialized");
                    outPkg.Write(0);
                }
                else
                {
                    outPkg.Write(1);

                    Plugin.LogInfo($"Sending Cache: cacheContentLen={serverSession.CacheContent?.Length ?? -1}");
                    outPkg.Write(serverSession.CacheContent);
                }

                return true;
            });

            CacheRPC.SetServerReady();
            HandshakeRPC.SetServerReady();
        }

        #endregion

        #region Client

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
                            if (CacheManager.LoadCacheFromCrypted(File.ReadAllText(cacheFilePath), clientSession.CacheCryptKey))
                            {
                                requestCacheFile = false;
                                RegistryOrchestrator.ValidateDataAndRegisterPrefabs();
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

                var success = CacheManager.LoadCacheFromCrypted(cacheContent, clientSession.CacheCryptKey);

                if (success)
                {
                    RegistryOrchestrator.ValidateDataAndRegisterPrefabs();
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

        #endregion

    }
}
