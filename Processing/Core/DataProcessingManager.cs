using OfTamingAndBreeding.Registry;
using OfTamingAndBreeding.Registry.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OfTamingAndBreeding.Processing.Core
{
    internal static class DataProcessingManager
    {
        private static bool dataLoaded = false;

        public static bool IsDataLoaded()
        {
            return dataLoaded;
        }

        private static readonly IDataProcessor[] dataProcessors = new IDataProcessor[] {
            new IconProcessor(),
            new TranslationProcessor(),
            new OffspringProcessor(),
            new ItemProcessor(),
            new CreatureProcessor(),
            new RecipeProcessor(),
        };

        public static IEnumerable<IDataProcessor> IterDataProcessors()
        {
            foreach (var dh in dataProcessors)
                yield return dh;
        }

        public static bool LoadDataFromLocalFiles()
        {
            if (dataLoaded)
            {
                // todo
            }

            var zn = ZNet.instance;
            string worldName = zn.GetWorldName();
            Plugin.LogInfo($"Loading Data for world: '{worldName}'");

            // pick world root (worldName or fallback)
            string worldRoot = Path.Combine(Plugin.ServerDataDir, worldName);
            if (!Directory.Exists(worldRoot))
            {
                worldRoot = Path.Combine(Plugin.ServerDataDir, Plugin.Configs.DefaultWorldDirectory.Value);
                Plugin.LogInfo($"No data directory found for world '{worldName}', using fallback to '{Plugin.Configs.DefaultWorldDirectory.Value}'");
            }

            if (!Directory.Exists(worldRoot))
            {
                Plugin.LogInfo($"No data directory found.");
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
                    return false;
                }
            }

            return true;
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
                    foreach (var file in Directory.EnumerateFiles(dir, "*.*", SearchOption.TopDirectoryOnly)
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
        // process routine
        //---------------------------

        public static bool ValidateDataAndRegisterPrefabs()
        {
            if (dataLoaded)
            {
                return true;
            }

            OTABPrefabRegistry.CreateInstance();
            OTABPrefabRegistry.SaveOriginalPrefabNames();

            foreach (var p in dataProcessors)
            {
                p.CallPrepareProcess();
            }

            foreach (var p in dataProcessors)
            {
                p.CallValidateAllData();
            }

            foreach (var p in dataProcessors)
            {
                p.CallReserveAllPrefabs();
            }

            var allOkay = true;
            foreach (var p in dataProcessors)
            {
                allOkay &= p.CallValidateAllPrefabs();
            }

            if (allOkay == false)
            {
                foreach (var p in dataProcessors)
                {
                    p.CallFinalizeProcess();
                }
                ResetRegistry();
                return false;
            }

            // from this point everything is okay

            foreach (var p in dataProcessors)
            {
                p.CallRegisterAllPrefabs();
            }
            foreach (var p in dataProcessors)
            {
                p.CallEditAllPrefabs();
            }
            foreach (var p in dataProcessors)
            {
                p.CallFinalizeProcess();
            }

            dataLoaded = true;
            return true;
        }

        public static void ResetRegistry()
        {
            if (OTABPrefabRegistry.Instance == null)
            {
                return;
            }

            for (var i= dataProcessors.Length - 1; i >= 0; i--)
            {
                dataProcessors[i].CallRestoreAllPrefabs();
            }

            for (var i = dataProcessors.Length - 1; i >= 0; i--)
            {
                dataProcessors[i].CallCleanupProcess();
            }

            for (var i = dataProcessors.Length - 1; i >= 0; i--)
            {
                dataProcessors[i].ResetData();
            }

            dataLoaded = false;
            OTABPrefabRegistry.DestroyInstance();
        }

    }
}
