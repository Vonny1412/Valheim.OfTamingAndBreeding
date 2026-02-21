
using OfTamingAndBreeding.Data.Processing;
using OfTamingAndBreeding.Data.Processing.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace OfTamingAndBreeding.Data
{
    internal static class DataOrchestrator
    {

        private static bool dataLoaded = false;
        private static readonly List<Action> dataLoadedCallbacks = new List<Action>();
        private static readonly List<Action> dataResetCallbacks = new List<Action>();

        public static bool IsDataLoaded()
        {
            return dataLoaded;
        }

        public static void OnDataLoaded(Action cb)
        {
            dataLoadedCallbacks.Add(cb);
        }

        public static void OnDataReset(Action cb)
        {
            dataResetCallbacks.Add(cb);
        }

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
            {
                worldRoot = Path.Combine(Plugin.ServerDataDir, Plugin.Configs.DefaultWorldDirectory.Value);
                Plugin.LogWarning($"No data directory found for world '{worldName}', using fallback to '{Plugin.Configs.DefaultWorldDirectory.Value}'");
            }

            if (!Directory.Exists(worldRoot))
            {
                Plugin.LogWarning($"No data directory found. Continuing without data...");
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
                if (!allokay)
                {
                    // fatal error in data
                    return;
                }
            }

            // just do it even with empty data
            ValidateDataAndRegisterPrefabs();
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
            ctx = new DataProcessorContext();

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
                p.ReserveAllPrefabs(ctx);
            }

            var allOkay = true;
            foreach (var p in dataProcessors)
            {
                allOkay &= p.ValidateAllPrefabs(ctx);
            }

            if (allOkay)
            {
                foreach (var p in dataProcessors)
                {
                    p.RegisterAllPrefabs(ctx);
                }
            }

            foreach (var p in dataProcessors)
            {
                p.Finalize(ctx);
            }

            if (allOkay)
            {
                dataLoaded = true;
                foreach (var cb in dataLoadedCallbacks)
                {
                    cb();
                }
            }
            else
            {
                ResetData();
            }
        }

        public static void ResetData()
        {
            if (ctx != null)
            {
                // ctx==null would mean no data loaded/processed yet
                for (var i= dataProcessors.Length - 1; i >= 0; i--)
                {
                    dataProcessors[i].RestoreAllPrefabs(ctx);
                }
                for (var i = dataProcessors.Length - 1; i >= 0; i--)
                {
                    dataProcessors[i].Cleanup(ctx);
                }
                ctx = null;
            }

            Runtime.Reset();
            for (var i = dataProcessors.Length - 1; i >= 0; i--)
            {
                dataProcessors[i].ResetData();
            }

            if (dataLoaded)
            {
                dataLoaded = false;
                foreach (var cb in dataResetCallbacks)
                {
                    cb();
                }
            }
        }

    }
}
