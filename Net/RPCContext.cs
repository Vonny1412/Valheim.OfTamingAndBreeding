
using System.IO;
using System.Collections;

using OfTamingAndBreeding.Data;
using OfTamingAndBreeding.Helpers;
using UnityEngine;

namespace OfTamingAndBreeding.Net
{
    internal class RPCContext
    {

        internal static BaseRPC HandshakeRPC;
        internal static BaseRPC CacheRPC;

        static RPCContext()
        {
            DataLoader.OnDataReset(() => {
                CacheRPC.ResetClientState();
                HandshakeRPC.ResetClientState();
            });
        }

        public static void Init()
        {
            HandshakeRPC = new BaseRPC("OTAB_InitRPC");
            CacheRPC = new BaseRPC("OTAB_CacheRPC");
        }

        public static void InitServer()
        {
            var inFunc1 = $"{nameof(RPCContext)}.{nameof(RPCContext.InitServer)}";
            Plugin.LogDebug($"[{inFunc1}] Start");

            DataLoader.LoadDataFromServerFiles();
            Plugin.LogDebug($"[{inFunc1}] Data counts: Creatures={Data.Models.Creature.GetAll().Count} Offsprings={Data.Models.Offspring.GetAll().Count} Eggs={Data.Models.Egg.GetAll().Count}");

            var cacheFileName = Plugin.Configs.CacheFileName.Value;
            var cacheCryptKey = Plugin.Configs.CacheFileCryptKey.Value;
            cacheFileName = cacheFileName.Replace("{world}", ZNet.instance.GetWorldName());
            cacheFileName = cacheFileName.Replace("{seed}", ZNet.World.m_seedName);
            Plugin.LogDebug($"[{inFunc1}] Server cache settings: UseCache={Plugin.Configs.UseCache.Value} CacheFileName='{cacheFileName}' KeyLen={(cacheCryptKey?.Length ?? -1)}");

            string cacheContent;

            Plugin.LogDebug($"[{inFunc1}] Cache build: WriteCache #1");
            var hash1 = CacheManager.WriteCache(cacheFileName, cacheCryptKey);
            Plugin.LogDebug($"[{inFunc1}] Cache build: LoadCacheFromFile");
            CacheManager.LoadCacheFromFile(cacheFileName, cacheCryptKey);
            Plugin.LogDebug($"[{inFunc1}] Cache build: WriteCache #2");
            var hash2 = CacheManager.WriteCache(cacheFileName, cacheCryptKey);
            Plugin.LogDebug($"[{inFunc1}] Cache hash1={hash1} hash2={hash2} match={hash1 == hash2}");

            if (hash1 == hash2)
            {
                Plugin.Configs.CacheFileHash.Value = hash1;
                cacheContent = File.ReadAllText(CacheManager.GetCacheCryptedFile(cacheFileName));
            }
            else
            {
                Plugin.LogFatal($"[{inFunc1}] Failed building cache file -> abort");
                DataLoader.ResetData();
                return;
            }

            HandshakeRPC.OnServerReceive((ZPackage inPkg, ZPackage outPkg) => {
                var inFunc2 = $"{nameof(HandshakeRPC)}.{nameof(HandshakeRPC.OnServerReceive)}";

                Plugin.LogDebug($"[{inFunc2}] Server received handshake request");

                // policy

                outPkg.Write(Plugin.Configs.RequireFoodDroppedByPlayer.Value);
                outPkg.Write(Plugin.Configs.ShowEggGrowProgress.Value);
                outPkg.Write(Plugin.Configs.ShowOffspringGrowProgress.Value);
                outPkg.Write(Plugin.Configs.ShowTamingProgress.Value);
                outPkg.Write(Plugin.Configs.HoverProgressPrecision.Value);

                // cache

                outPkg.Write(Plugin.Configs.UseCache.Value);
                outPkg.Write(Plugin.Configs.CacheFileName.Value);
                outPkg.Write(Plugin.Configs.CacheFileHash.Value);

                var obf = KeyMask.Obfuscate(cacheCryptKey, $"{cacheFileName}|{Plugin.Version}");
                Plugin.LogDebug($"[{inFunc2}] Responding: UseCache={Plugin.Configs.UseCache.Value} File='{cacheFileName}' Hash='{Plugin.Configs.CacheFileHash.Value}' ObfLen={obf.Length}");
                outPkg.Write(obf);

                return true;
            });

            CacheRPC.OnServerReceive((ZPackage inPkg, ZPackage outPkg) => {
                var inFunc2 = $"{nameof(CacheRPC)}.{nameof(CacheRPC.OnServerReceive)}";

                Plugin.LogDebug($"[{inFunc2}] Server received request. Sending cacheContentLen={cacheContent?.Length ?? -1}");
                outPkg.Write(cacheContent);
                return true;
            });

            CacheRPC.SetServerReady();
            HandshakeRPC.SetServerReady();
            Plugin.LogMessage($"[{inFunc1}] Done -> ready for clients");
        }

        public static void InitClient()
        {
            var inFunc1 = $"{nameof(RPCContext)}.{nameof(RPCContext.InitClient)}";
            Plugin.LogDebug($"[{inFunc1}] Start");

            var saveCache = false;
            string cacheFileName = null;
            string cacheFileHash = null;
            string cacheFileCryptKey = null;

            CacheRPC.OnClientReceive((ZPackage inPkg) => {
                var inFunc2 = $"{nameof(CacheRPC)}.{nameof(CacheRPC.OnClientReceive)}";

                Plugin.LogDebug($"[{inFunc2}] Client received response");

                var cacheContent = inPkg.ReadString();
                Plugin.LogDebug($"[{inFunc2}] contentLen={cacheContent?.Length ?? -1} saveCache={saveCache} file='{cacheFileName}'");

                if (saveCache)
                {
                    if (cacheFileName == null)
                    {
                        return false; // this dhould not happen Oo
                    }
                    var cacheFile = CacheManager.GetCacheCryptedFile(cacheFileName);
                    Plugin.LogDebug($"[{inFunc2}] Writing cache file '{cacheFile}'");
                    File.WriteAllText(cacheFile, cacheContent);
                }

                var success = CacheManager.LoadCacheFromCrypted(cacheContent, cacheFileCryptKey);
                Plugin.LogMessage($"[{inFunc2}] Loaded data from received cache");

                if (success)
                {
                    DataLoader.ValidateAndPreparePrefabs();
                }
                return success;
            });

            HandshakeRPC.OnClientReceive((ZPackage inPkg) => {
                var inFunc2 = $"{nameof(HandshakeRPC)}.{nameof(HandshakeRPC.OnClientReceive)}";

                Plugin.LogDebug($"[{inFunc2}] Client received response");

                // policy

                Plugin.Configs.RequireFoodDroppedByPlayer.Value = inPkg.ReadBool();
                Plugin.Configs.ShowEggGrowProgress.Value = inPkg.ReadBool();
                Plugin.Configs.ShowOffspringGrowProgress.Value = inPkg.ReadBool();
                Plugin.Configs.ShowTamingProgress.Value = inPkg.ReadBool();
                Plugin.Configs.HoverProgressPrecision.Value = inPkg.ReadSingle();

                // cache

                bool useCache = inPkg.ReadBool();
                cacheFileName = inPkg.ReadString();
                cacheFileHash = inPkg.ReadString();

                var obf = inPkg.ReadString();
                cacheFileCryptKey = KeyMask.Deobfuscate(obf, $"{cacheFileName}|{Plugin.Version}");
                Plugin.LogDebug($"[{inFunc2}] useCache={useCache} file='{cacheFileName}' hash='{cacheFileHash}' obfLen={obf?.Length ?? -1} keyLen={cacheFileCryptKey?.Length ?? -1} version='{Plugin.Version}'");

                var requestCacheFile = true;
                if (useCache)
                {
                    saveCache = true;

                    var cacheFilePath = Path.Combine(Plugin.CacheDir, cacheFileName);
                    if (File.Exists(cacheFilePath))
                    {
                        var hash = CacheManager.ComputeSha256FileHash(cacheFilePath);
                        if (hash == cacheFileHash)
                        {
                            if (CacheManager.LoadCacheFromCrypted(File.ReadAllText(cacheFilePath), cacheFileCryptKey))
                            {
                                Plugin.LogMessage($"[{inFunc2}] Loaded data from existing cache");
                                requestCacheFile = false;
                                DataLoader.ValidateAndPreparePrefabs();
                            }
                        }
                    }
                }
                else
                {
                    requestCacheFile = true;
                    saveCache = false;
                }

                if (requestCacheFile)
                {
                    Plugin.LogDebug($"[{inFunc2}] requestCacheFile={requestCacheFile} readCache={useCache} cacheFileName={cacheFileName}");
                    CacheRPC.RequestFromServer();
                }
                else
                {
                    Plugin.LogDebug($"[{inFunc2}] Cache accepted locally, no request needed");
                }

                return true;
            });

            Plugin.LogDebug($"[{inFunc1}] Done");
        }

        private static bool handshakeRequested;

        public static void RequestHandshakeWithServer()
        {
            var inFunc = $"{nameof(RPCContext)}.{nameof(RPCContext.RequestHandshakeWithServer)}";
            if (handshakeRequested) return;
            handshakeRequested = true;
            Plugin.LogDebug($"[{inFunc}] Requesting handshake from server");
            HandshakeRPC.RequestFromServer();
        }

    }
}
