
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using OfTamingAndBreeding.Data.Handling;

namespace OfTamingAndBreeding.Data
{
    internal static class DataLoader
    {

        private static readonly List<Action> OnResetCallbacks = new List<Action>();
        public static void OnDataReset(Action cb)
            => OnResetCallbacks.Add(cb);

        private static readonly IModelHandler[] dataHandlers = new IModelHandler[] {
                new OffspringModelHandler(),
                new EggModelHandler(),
                new CreatureModelHandler(),
                new RecipeModelHandler(),
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

        public static void LoadDataFromLocalFiles()
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
            var stack = new Stack<string>();
            stack.Push(worldRoot);

            while (stack.Count > 0)
            {
                var dir = stack.Pop();

                // Skip subtree if this directory starts with "_" (but allow worldRoot itself)
                if (!string.Equals(dir, worldRoot, StringComparison.OrdinalIgnoreCase))
                {
                    if (Path.GetFileName(dir)?.StartsWith("_", StringComparison.Ordinal) ?? true)
                        continue;
                }

                // If this directory IS the category folder, yield its .yml files (top-level only)
                if (string.Equals(Path.GetFileName(dir), categoryFolderName, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var file in Directory.EnumerateFiles(dir, "*.yml", SearchOption.TopDirectoryOnly)
                                 .OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
                    {
                        yield return file;
                    }
                    continue;
                }

                // Descend: push subdirectories in reverse-sorted order so pop() processes them sorted
                var subDirs = Directory.EnumerateDirectories(dir, "*", SearchOption.TopDirectoryOnly)
                    .OrderByDescending(d => d, StringComparer.OrdinalIgnoreCase);

                foreach (var sub in subDirs)
                    stack.Push(sub);
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
            if (ZNet.instance.IsServer())
            {
                foreach (var dh in dataHandlers)
                {
                    Plugin.LogMessage($"Loaded {dh.GetLoadedDataCount()} {dh.ModelTypeName} entries");
                }
            }
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
