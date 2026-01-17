
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using OfTamingAndBreeding.Data.Handling;

namespace OfTamingAndBreeding.Data
{
    internal sealed class DataLoader
    {

        private static readonly List<Action> OnResetCallbacks = new List<Action>();
        public static void OnDataReset(Action cb)
            => OnResetCallbacks.Add(cb);

        private static readonly IModelHandler[] dataHandlers = new IModelHandler[] {
                new OffspringModelHandler(),
                new EggModelHandler(),
                new CreatureModelHandler(),
            };

        
        public static void IterDataHandlers(Action<IModelHandler> cb)
        {
            foreach(var dh in dataHandlers)
            {
                cb(dh);
            }
        }
        public static IEnumerable<IModelHandler> IterDataHandlers()
        {
            foreach (var dh in dataHandlers)
                yield return dh;
        }

        // NOTE:
        // Server data is authoritative.
        // Client YAMLs are ignored once connected to a server.
        // This ensures deterministic breeding/taming behavior in multiplayer.

        public static void LoadDataFromServerFiles()
        {
            var zn = ZNet.instance;
            string worldName = zn.GetWorldName();
            Plugin.LogMessage($"Loading Data for world: '{worldName}'");

            // pick world root (worldName or fallback)
            string worldRoot = Path.Combine(Plugin.ServerDataDir, worldName);
            if (!Directory.Exists(worldRoot))
                worldRoot = Path.Combine(Plugin.ServerDataDir, Plugin.DefaultWorldDirectory);

            if (!Directory.Exists(worldRoot))
            {
                Plugin.LogMessage($"No data directory found for world '{worldName}' (or default).");
            }
            else
            {
                var allokay = true;
                foreach (var dh in dataHandlers)
                {
                    foreach (var file in EnumerateCategoryFiles(worldRoot, dh.DirectoryName))
                    {
                        allokay &= dh.LoadFromFile(file);
                    }
                }
                if (allokay)
                {
                    ValidateAndPreparePrefabs();
                }
            }
        }

        private static IEnumerable<string> EnumerateCategoryFiles(string worldRoot, string categoryFolderName)
        {
            // Find all folders named e.g. "Creatures" anywhere under worldRoot (including direct one)
            var folders = Directory.EnumerateDirectories(worldRoot, "*", SearchOption.AllDirectories)
                .Where(d => string.Equals(Path.GetFileName(d), categoryFolderName, StringComparison.OrdinalIgnoreCase))
                .OrderBy(d => d, StringComparer.OrdinalIgnoreCase);

            // Deterministic file order
            foreach (var folder in folders)
            {
                foreach (var file in Directory.EnumerateFiles(folder, "*.yml", SearchOption.TopDirectoryOnly)
                             .OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
                {
                    yield return file;
                }
            }
        }

        public static void ValidateAndPreparePrefabs()
        {
            var ctx = new ModelHandlerContext(ZNetScene.instance);
            foreach (var dh in dataHandlers)
            {
                dh.ValidateAllData(ctx);
            }
            foreach (var dh in dataHandlers)
            {
                dh.PrepareAllPrefabs(ctx);
            }
            var allOkay = true;
            foreach (var dh in dataHandlers)
            {
                allOkay &= dh.ValidateAllPrefabs(ctx);
            }
            if (!allOkay)
            {
                Plugin.LogFatal($"Missing prefab dependencies found!");
                ResetData();
                return;
            }
            foreach (var dh in dataHandlers)
            {
                dh.RegisterAllPrefabs(ctx);
            }
            /*
            if (ZNet.instance.IsServer())
            {
                foreach (var dh in dataHandlers)
                {
                    Plugin.LogMessage($"Loaded {dh.GetLoadedDataCount()} {dh.ModelTypeName} entries");
                }
            }
            */
        }

        public static Dictionary<string, Dictionary<string, string>> GetServerData()
        {
            var allData = new Dictionary<string, Dictionary<string, string>>();
            foreach (var dh in dataHandlers)
            {
                allData.Add(dh.DirectoryName, dh.GetYamlDictFromAll());
            }
            return allData;
        }

        public static void ResetData()
        {
            foreach (var dh in dataHandlers)
            {
                dh.ResetData();
            }
            foreach (var cb in OnResetCallbacks)
            {
                cb();
            }
        }

    }
}
