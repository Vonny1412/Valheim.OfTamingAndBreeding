using OfTamingAndBreeding.Data;
using OfTamingAndBreeding.Processing.Registry;
using OfTamingAndBreeding.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Core;

namespace OfTamingAndBreeding.Processing.Core
{
    internal abstract class DataProcessor<T> : IDataProcessor where T : DataBase<T>
    {
        public abstract string DirectoryName { get; }

        public string ModelTypeName => typeof(T).Name;

        public abstract string PrefabTypeName { get; }

        public abstract string GetDataKey(string filePath);

        public abstract bool LoadFromFile(string file);

        public bool LoadFromYamlFile(string filePath)
        {
            if (Path.GetExtension(filePath).ToLower() != ".yml")
            {
                return false;
            }
            var prefabName = Path.GetFileNameWithoutExtension(filePath);
            var yamlText = File.ReadAllText(filePath);
            var fileNameParsed = GetDataKey(filePath);
            if (fileNameParsed != null)
            {
                // file name => prefab name => data key
                prefabName = fileNameParsed;
            }
            return LoadYamlData(prefabName, yamlText);
        }

        public bool LoadYamlData(string prefabName, string yamlText)
        {
            try
            {
                Plugin.LogDebug($"Loading {typeof(T).Name} '{prefabName}' from YAML");
                var data = DataBase<T>.Deserialize(yamlText);
                DataBase<T>.Store(prefabName, data);
                return true;
            }
            catch (YamlException e)
            {
                Plugin.LogFatal(YamlUtils.FormatException(
                    e,
                    yamlText,
                    $"Failed loading YAML for {typeof(T).Name} '{prefabName}'"
                ));
            }

            return false;
        }

        public Dictionary<string, string> GetAllSerializedData()
        {
            var ret = new Dictionary<string, string>();
            foreach (var kv in DataBase<T>.GetAll())
            {
                var prefabName = kv.Key;
                var prefabData = kv.Value;
                var prefabYaml = prefabData.Serialize();
                ret.Add(prefabName, prefabYaml);
            }
            return ret;
        }

        public int GetLoadedDataCount() => DataBase<T>.GetAll().Count;

        //---------------------
        // orchestrator routine
        //---------------------

        public abstract bool PrepareProcess();

        public abstract bool ReservePrefabName(string prefabName);

        public abstract bool ValidateData(string prefabName, T data);

        public abstract bool RegisterPrefab(string prefabName, T data);

        public abstract bool ProcessPrefab(string prefabName, T data);

        public abstract bool FinalizeProcess();

        public abstract void RestorePrefab(string prefabName);

        public abstract void CleanupProcess();

        private bool CallForAllData(Func<string, T, bool> action, string actionName)
        {
            Plugin.LogDebug($"{actionName}: {typeof(T).Name}");

            var valid = true;

            foreach (var entry in DataBase<T>.GetAll())
            {
                var prefabName = entry.Key;
                var data = entry.Value;

                Plugin.LogDebug($"{actionName}: {typeof(T).Name} '{prefabName}'");

                try
                {
                    valid &= action(prefabName, data);
                }
                catch (Exception e)
                {
                    Plugin.LogFatal($"{ModelTypeName}.{actionName}() '{prefabName}' failed");
                    Plugin.LogFatal(e);
                    valid = false;
                }
            }

            return valid;
        }

        private void CallForAllData(Action<string> action, string actionName)
        {
            foreach (var prefabName in DataBase<T>.GetAll().Keys)
            {
                Plugin.LogDebug($"{actionName}: {typeof(T).Name} '{prefabName}'");
                try
                {
                    action(prefabName);
                }
                catch (Exception e)
                {
                    Plugin.LogFatal($"{ModelTypeName}.{actionName}() '{prefabName}' failed");
                    Plugin.LogFatal(e);
                }
            }
        }

        public bool ReserveAllPrefabNames()
        {
            Plugin.LogDebug($"{nameof(ReserveAllPrefabNames)}: {typeof(T).Name}");
            var valid = true;
            foreach (var prefabName in DataBase<T>.GetAll().Keys)
            {
                valid &= ReservePrefabName(prefabName);
            }
            return valid;
        }

        private bool ValidateDataInternal(string prefabName, T data)
        {
            if (PrefabTypeName != null && !OTABPrefabRegistry.TryRegisterPrefabType(prefabName, PrefabTypeName))
            {
                return false;
            }
            return ValidateData(prefabName, data);
        }

        public bool ValidateAllData()
        {
            return CallForAllData(ValidateDataInternal, nameof(ValidateData));
        }

        public bool RegisterAllPrefabs()
        {
            return CallForAllData(RegisterPrefab, nameof(RegisterPrefab));
        }

        public bool ProcessAllPrefabs()
        {
            return CallForAllData(ProcessPrefab, nameof(ProcessPrefab));
        }

        public void RestoreAllPrefabs()
        {
            Plugin.LogDebug($"{nameof(RestoreAllPrefabs)}: {typeof(T).Name}");

            CallForAllData((prefabName) => RestorePrefab(prefabName), nameof(RestorePrefab));
        }

        public void ResetData()
        {
            DataBase<T>.DropAll();
        }

    }

}
