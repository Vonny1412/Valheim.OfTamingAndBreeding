using OfTamingAndBreeding.Processing.Registry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OfTamingAndBreeding.Processing.Core
{
    internal static class DataProcessingManager
    {
        private static bool dataLoaded = false;

        private static readonly IDataProcessor[] dataProcessors = new IDataProcessor[] {
            new TextureProcessor(),
            new TranslationProcessor(),
            new ItemProcessor(),
            new RecipeProcessor(),
            new CreatureProcessor(),
        };

        public static IEnumerable<IDataProcessor> DataProcessors => dataProcessors;
        public static bool IsDataLoaded => dataLoaded;

        // TODO: Distinguish expected processing failures from unexpected exceptions.
        //
        // Invalid user data (e.g. YAML validation errors) is an expected failure:
        // - ResetRegistry() restores/cleans the current processing attempt.
        // - The player is returned to the menu.
        // - After fixing the data, joining a world again is allowed.
        //
        // Unexpected exceptions during processing indicate an unknown/inconsistent state.
        // Even though ResetRegistry() performs a best-effort cleanup, we cannot guarantee
        // that every side effect caused before the exception has been tracked/restored.
        // Retrying processing in the same Valheim session could therefore cause prefab
        // corruption, stale Unity references, duplicate registrations, etc.
        //
        // Possible implementation:
        // - Add a static `fatalProcessingError` flag to DataProcessingManager.
        // - Set it when RunForAllProcessors() catches an unexpected exception.
        // - Also consider setting it if an exception occurs during ResetRegistry(),
        //   because cleanup itself was then incomplete.
        // - ResetRegistry() should still always attempt the full best-effort cleanup.
        // - ValidateDataAndRegisterPrefabs() should refuse another processing attempt
        //   while `fatalProcessingError` is set and log that Valheim must be restarted.
        // - Do NOT set the flag for normal `false` results caused by invalid user data.
        // - Do NOT clear the flag on world/menu transitions; only restarting Valheim
        //   should restore a trustworthy process state.
        //
        // Important:
        // Keep the distinction based on exceptions vs. normal validation failures.
        // Do not use a general processing-phase flag: an exception may happen halfway
        // through processing an individual prefab, so the reached phase alone cannot
        // describe which mutations have already occurred.

        private static bool RunForAllProcessors(Func<IDataProcessor, bool> action)
        {
            var valid = true;
            foreach (var processor in dataProcessors)
            {
                try
                {
                    valid &= action(processor);
                }
                catch (Exception ex)
                {
                    Plugin.LogError($"Unexpected exception while processing {processor.GetType().Name}:\n{ex}");
                    return false;
                }
            }
            return valid;
        }

        private static void RunForAllProcessorsReverse(Action<IDataProcessor> action)
        {
            for (var i = dataProcessors.Length - 1; i >= 0; i--)
            {
                var processor = dataProcessors[i];
                try
                {
                    action(processor);
                }
                catch (Exception ex)
                {
                    Plugin.LogError($"Unexpected exception while resetting {processor.GetType().Name}:\n{ex}");
                }
            }
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
                var valid = true;
                foreach (var dh in dataProcessors)
                {
                    foreach (var file in EnumerateCategoryFiles(worldRoot, dh.DirectoryName))
                    {
                        valid &= dh.LoadFromFile(file);
                    }
                }
                if (!valid)
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

            if (!RunForAllProcessors(p => p.PrepareProcess()))
            {
                ResetRegistry();
                return false;
            }

            if (!RunForAllProcessors(p => p.ReserveAllPrefabNames()))
            {
                ResetRegistry();
                return false;
            }

            if (!RunForAllProcessors(p => p.ValidateAllData()))
            {
                ResetRegistry();
                return false;
            }

            if (!RunForAllProcessors(p => p.RegisterAllPrefabs()))
            {
                ResetRegistry();
                return false;
            }

            if (!RunForAllProcessors(p => p.ProcessAllPrefabs()))
            {
                ResetRegistry();
                return false;
            }

            if (!RunForAllProcessors(p => p.FinalizeProcess()))
            {
                ResetRegistry();
                return false;
            }






            dataLoaded = true;
            return true;
        }

        public static void ResetRegistry()
        {
            dataLoaded = false;

            if (OTABPrefabRegistry.Instance != null)
            {
                RunForAllProcessorsReverse(p => p.RestoreAllPrefabs());
            }

            RunForAllProcessorsReverse(p => p.CleanupProcess());
            RunForAllProcessorsReverse(p => p.ResetData());

            OTABPrefabRegistry.DestroyInstance();
        }

    }
}
