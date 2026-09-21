using OfTamingAndBreeding.Data;
using OfTamingAndBreeding.Registry;
using OfTamingAndBreeding.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Core;

namespace OfTamingAndBreeding.Processing.Core
{
    internal abstract class DataProcessor<T> : IDataProcessor where T : DataBase<T>
    {


        //
        // IDataProcessor
        //

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

        public void ResetData()
        {
            DataBase<T>.DropAll();
        }

        //---------------------
        // orchestrator routine
        //---------------------

        private readonly HashSet<string> reservedPrefabNames = new HashSet<string>(); // for current session

        public abstract void PrepareProcess();

        public abstract bool ValidateData(string prefabName, T data);

        public abstract bool ReservePrefab(string prefabName, T data);

        public abstract bool ValidatePrefab(string prefabName, T data);

        public abstract void RegisterPrefab(string prefabName, T data);

        public abstract bool ProcessPrefab(string prefabName, T data);

        public abstract void FinalizeProcess();

        public abstract void RestorePrefab(string prefabName);

        public abstract void CleanupProcess();

        //
        // methods called by orchestrator
        //

        public void CallPrepareProcess()
        {
            PrepareProcess();
        }

        public bool CallValidateAllData()
        {
            Plugin.LogDebug($"{nameof(CallValidateAllData)}: {typeof(T).Name}");
            var all = DataBase<T>.GetAll();
            var keys = all.Keys.ToList();
            var valid = true;
            foreach (var prefabName in keys)
            {
                if (!all.TryGetValue(prefabName, out var data))
                    continue;
                Plugin.LogDebug($"{nameof(ValidateData)}: {typeof(T).Name} '{prefabName}'");
                try
                {
                    if (!ValidateData(prefabName, data))
                    {
                        //DataBase<T>.Drop(prefabName);
                        valid = false;
                    }
                }
                catch (Exception e)
                {
                    Plugin.LogFatal($"{ModelTypeName}.{nameof(ValidateData)}() '{prefabName}' failed");
                    Plugin.LogFatal(e);
                    valid = false;
                }
            }
            return valid;
        }

        public bool CallReserveAllPrefabs()
        {
            Plugin.LogDebug($"{nameof(CallReserveAllPrefabs)}: {typeof(T).Name}");
            var all = DataBase<T>.GetAll();
            var keys = all.Keys.ToList();
            var valid = true;
            foreach (var prefabName in keys)
            {
                if (!all.TryGetValue(prefabName, out var data))
                    continue;
                Plugin.LogDebug($"{nameof(ReservePrefab)}: {typeof(T).Name} '{prefabName}'");
                try
                {
                    if (PrefabTypeName != null)
                    {
                        if (OTABPrefabRegistry.TryRegisterPrefabType(prefabName, PrefabTypeName, out string registeredTypeName) == false)
                        {
                            Plugin.LogFatal($"Tried to register {typeof(T).Name} '{prefabName}' as type '{PrefabTypeName}' but has already been registered as type '{registeredTypeName}' before by an other OTAB instance. Rename your custom prefab to avoid prefab corruption");
                            valid = false;
                            continue;
                        }
                    }

                    if (reservedPrefabNames.Contains(prefabName))
                    {
                        Plugin.LogError($"{nameof(ReservePrefab)}: {typeof(T).Name} '{prefabName}' already reserved!");
                        valid = false;
                        continue;
                    }

                    if (ReservePrefab(prefabName, data))
                    {
                        reservedPrefabNames.Add(prefabName);
                    }
                    else
                    {
                        valid = false;
                    }
                    
                }
                catch (Exception e)
                {
                    Plugin.LogFatal($"{ModelTypeName}.{nameof(ReservePrefab)}() '{prefabName}' failed");
                    Plugin.LogFatal(e);
                    valid = false;
                }
            }
            return valid;
        }

        public bool CallValidateAllPrefabs()
        {
            Plugin.LogDebug($"{nameof(CallValidateAllPrefabs)}: {typeof(T).Name}");
            var all = DataBase<T>.GetAll();
            var keys = all.Keys.ToList();
            var valid = true;
            foreach (var prefabName in keys)
            {
                if (!all.TryGetValue(prefabName, out var data))
                    continue;
                Plugin.LogDebug($"{nameof(ValidatePrefab)}: {typeof(T).Name} '{prefabName}'");
                try
                {
                    if (!ValidatePrefab(prefabName, data))
                    {
                        valid = false;
                    }
                }
                catch (Exception e)
                {
                    Plugin.LogFatal($"{ModelTypeName}.{nameof(ValidatePrefab)}() '{prefabName}' failed");
                    Plugin.LogFatal(e);
                    valid = false;
                }
            }
            return valid;
        }

        public void CallRegisterAllPrefabs()
        {
            Plugin.LogDebug($"{nameof(CallRegisterAllPrefabs)} {typeof(T).Name}");
            var all = DataBase<T>.GetAll();
            var keys = all.Keys.ToList();
            foreach (var prefabName in keys)
            {
                if (!all.TryGetValue(prefabName, out var data))
                    continue;
                Plugin.LogDebug($"{nameof(RegisterPrefab)} {typeof(T).Name} '{prefabName}'");
                try
                {
                    RegisterPrefab(prefabName, data);
                }
                catch (Exception e)
                {
                    Plugin.LogFatal($"{ModelTypeName}.{nameof(RegisterPrefab)}() '{prefabName}' failed");
                    Plugin.LogFatal(e);
                }
            }
        }

        public bool CallProcessAllPrefabs()
        {
            Plugin.LogDebug($"{nameof(CallProcessAllPrefabs)} {typeof(T).Name}");
            var all = DataBase<T>.GetAll();
            var keys = all.Keys.ToList();
            var valid = true;
            foreach (var prefabName in keys)
            {
                if (!all.TryGetValue(prefabName, out var data))
                    continue;
                Plugin.LogDebug($"{nameof(ProcessPrefab)} {typeof(T).Name} '{prefabName}'");
                try
                {
                    if (!ProcessPrefab(prefabName, data))
                    {
                        valid = false;
                    }
                }
                catch (Exception e)
                {
                    Plugin.LogFatal($"{ModelTypeName}.{nameof(ProcessPrefab)}() '{prefabName}' failed");
                    Plugin.LogFatal(e);
                    valid = false;
                }
            }
            return valid;
        }

        public void CallFinalizeProcess()
        {
            FinalizeProcess();
        }

        public void CallRestoreAllPrefabs()
        {
            Plugin.LogDebug($"{nameof(CallRestoreAllPrefabs)} {typeof(T).Name}");
            var all = DataBase<T>.GetAll();
            var keys = all.Keys.ToList();
            foreach (var prefabName in keys)
            {
                if (!all.TryGetValue(prefabName, out var data))
                    continue;
                Plugin.LogDebug($"{nameof(RestorePrefab)} {typeof(T).Name} '{prefabName}'");
                try
                {
                    RestorePrefab(prefabName);
                }
                catch (Exception e)
                {
                    Plugin.LogFatal($"{ModelTypeName}.{nameof(RestorePrefab)}() '{prefabName}' failed");
                    Plugin.LogFatal(e);
                }
            }
        }

        public void CallCleanupProcess()
        {
            CleanupProcess();
            reservedPrefabNames.Clear();
        }

    }

}
