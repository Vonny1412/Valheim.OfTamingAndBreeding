
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OfTamingAndBreeding.Data.Processing;
using OfTamingAndBreeding.Data.Processing.Base;

namespace OfTamingAndBreeding.Data
{
    internal static class DataOrchestrator
    {

        private static readonly IDataProcessor[] dataProcessors = new IDataProcessor[] {
            new TranslationProcessor(),
            new OffspringProcessor(),
            new EggProcessor(),
            new CreatureProcessor(),
            new RecipeProcessor(),
        };

        public static IEnumerable<IDataProcessor> IterDataProcessors()
        {
            foreach (var dh in dataProcessors)
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
            Plugin.LogInfo($"Loading Data for world: '{worldName}'");

            // pick world root (worldName or fallback)
            string worldRoot = Path.Combine(Plugin.ServerDataDir, worldName);
            if (!Directory.Exists(worldRoot))
                worldRoot = Path.Combine(Plugin.ServerDataDir, Plugin.DefaultWorldDirectory);

            if (!Directory.Exists(worldRoot))
            {
                Plugin.LogWarning($"No data directory found for world '{worldName}' (or default).");
            }
            else
            {
                var allokay = true;
                foreach (var dh in dataProcessors)
                {
                    foreach (var file in EnumerateCategoryFiles(worldRoot, dh.DirectoryName))
                    {
                        allokay &= dh.LoadFromFile(file);
                    }
                }
                if (allokay)
                {
                    ValidateDataAndRegisterPrefabs();
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
                    if (Path.GetFileName(dir).StartsWith("_", StringComparison.Ordinal))
                        continue;
                }

                // If this directory IS the category folder, yield its .yml files (top-level only)
                if (string.Equals(Path.GetFileName(dir), categoryFolderName, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var file in Directory.EnumerateFiles(dir, "*.yml", SearchOption.TopDirectoryOnly)
                                 .OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
                    {
                        if (Path.GetFileName(file).StartsWith("_", StringComparison.Ordinal))
                            continue;

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

        //---------------------------
        // context stuff
        //---------------------------

        private static DataProcessorContext ctx = null;

        public static void ValidateDataAndRegisterPrefabs()
        {
            ctx = new DataProcessorContext(ZNetScene.instance);

            foreach (var p in dataProcessors)
            {
                p.Prepare(ctx);
            }

            foreach (var p in dataProcessors)
            {
                p.ValidateAllData(ctx);
            }

            foreach (var p in dataProcessors)
            {
                p.PrepareAllPrefabs(ctx);
            }

            var allOkay = true;
            foreach (var p in dataProcessors)
            {
                allOkay &= p.ValidateAllPrefabs(ctx);
            }
            if (!allOkay)
            {
                Plugin.LogFatal($"Missing prefab dependencies found!");
                ResetData();
                return;
            }

            foreach (var p in dataProcessors)
            {
                p.RegisterAllPrefabs(ctx);
            }

            foreach (var p in dataProcessors)
            {
                p.Cleanup(ctx);
            }

            if (ZNet.instance.IsServer())
            {
                foreach (var p in dataProcessors)
                {
                    Plugin.LogInfo($"Loaded {p.GetLoadedDataCount()} {p.ModelTypeName} entries");
                }
            }
            
        }

        public static void ResetData()
        {
            if (ctx != null)
            {
                // ctx==null would mean no data loaded/processed yet
                foreach (var p in dataProcessors)
                {
                    p.RestoreAllPrefabs(ctx);
                }
            }
            Runtime.Reset();
            foreach (var p in dataProcessors)
            {
                // resets stored data in DataBase<T>
                p.ResetData();
            }
            ctx = null;
        }

    }
}
