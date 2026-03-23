using OfTamingAndBreeding.Registry.Processing;
using OfTamingAndBreeding.Registry.Processing.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OfTamingAndBreeding.Registry
{
    internal class PrefabRegistryManager : Common.SingletonClass<PrefabRegistryManager>
    {

        //--------------------------------------------------
        // Singleton

        protected override void OnCreate()
        {
        }

        protected override void OnDestroy()
        {
        }

        //--------------------------------------------------

        public event Action OnRegistrationFinished;
        public event Action OnReset;

        private bool dataLoaded = false;

        private readonly IDataProcessor[] dataProcessors = new IDataProcessor[] {
            new IconProcessor(),
            new TranslationProcessor(),
            new OffspringProcessor(),
            new EggProcessor(),
            new CreatureProcessor(),
            new RecipeProcessor(),
        };

        public bool IsDataLoaded()
        {
            return dataLoaded;
        }

        public IEnumerable<IDataProcessor> IterDataProcessors()
        {
            foreach (var dh in dataProcessors)
                yield return dh;
        }

        public bool LoadDataFromLocalFiles()
        {
            if (dataLoaded)
            {
                // todo
            }

            var zn = ZNet.instance;
            string worldName = zn.GetWorldName();
            Plugin.LogServerInfo($"Loading Data for world: '{worldName}'");

            // pick world root (worldName or fallback)
            string worldRoot = Path.Combine(Plugin.ServerDataDir, worldName);
            if (!Directory.Exists(worldRoot))
            {
                worldRoot = Path.Combine(Plugin.ServerDataDir, Plugin.Configs.DefaultWorldDirectory.Value);
                Plugin.LogServerInfo($"No data directory found for world '{worldName}', using fallback to '{Plugin.Configs.DefaultWorldDirectory.Value}'");
            }

            if (!Directory.Exists(worldRoot))
            {
                Plugin.LogServerInfo($"No data directory found. Continuing without data...");
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

            // just do it even with empty data
            return ValidateDataAndRegisterPrefabs();
        }

        private IEnumerable<string> EnumerateCategoryFiles(string worldRoot, string categoryFolderName)
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

        public bool ValidateDataAndRegisterPrefabs()
        {
            if (dataLoaded)
            {
                // todo
            }

            PrefabRegistry.CreateInstance();
            PrefabRegistry.SaveOriginalPrefabNames();

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
            OnRegistrationFinished?.Invoke();
            return true;
        }

        public void ResetRegistry()
        {

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

            PrefabRegistry.DestroyInstance();

            dataLoaded = false;
            OnReset?.Invoke();
        }

    }
}
