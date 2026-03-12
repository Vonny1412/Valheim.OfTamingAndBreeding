using OfTamingAndBreeding.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using YamlDotNet.Core;

namespace OfTamingAndBreeding.Registry.Processing.Base
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
                Plugin.LogServerDebug($"Loading {typeof(T).Name} '{prefabName}' from YAML");
                var data = DataBase<T>.Deserialize(yamlText);
                DataBase<T>.Store(prefabName, data);
                return true;
            }
            catch (YamlException e)
            {
                Plugin.LogFatal(OTABUtils.YamlUtils.FormatException(
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
        // processor utils
        //---------------------

        protected static string ParseGlobalKey(string rawKey)
        {
            string[] keyParts = rawKey.Split(':');
            switch (keyParts.Length)
            {

                case 1:
                    {
                        var key = keyParts[0].Trim();
                        return key;
                    }

                case 2:
                    {
                        var mod = keyParts[0].Trim();
                        var key = keyParts[1].Trim();
                        if (ThirdParty.ThirdPartyManager.TryGetPluginMetadata(mod, out _))
                        {
                            return key;
                        }
                        break;
                    }
                default:
                    // todo: warning
                    break;

            }
            return null;
        }

        protected static List<string[]> ParseGlobalKeys(string[] rawKeys)
        {
            var orList = new List<string[]>();
            foreach (var unsplitted in rawKeys)
            {
                var splitted = unsplitted.Split(',')
                    .Select(ParseGlobalKey)
                    .Where((key) => !string.IsNullOrEmpty(key))
                    .ToArray();
                if (splitted.Length > 0)
                {
                    orList.Add(splitted);
                }
                else
                {
                    // todo: warning
                }
            }
            return orList;
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

        public abstract void EditPrefab(string prefabName, T data);

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

        public void CallValidateAllData()
        {
            Plugin.LogDebug($"{nameof(CallValidateAllData)}: {typeof(T).Name}");
            var all = DataBase<T>.GetAll();
            var keys = all.Keys.ToList();
            foreach (var prefabName in keys)
            {
                if (!all.TryGetValue(prefabName, out var data))
                    continue;
                Plugin.LogServerDebug($"{nameof(ValidateData)}: {typeof(T).Name} '{prefabName}'");
                try
                {
                    if (!ValidateData(prefabName, data))
                    {
                        DataBase<T>.Drop(prefabName);
                    }
                }
                catch(Exception)
                {
                    Plugin.LogFatal($"{ModelTypeName}.{nameof(ValidateData)}() '{prefabName}' failed");
                    throw;
                }
            }
        }

        public void CallReserveAllPrefabs()
        {
            Plugin.LogDebug($"{nameof(CallReserveAllPrefabs)}: {typeof(T).Name}");
            var all = DataBase<T>.GetAll();
            var keys = all.Keys.ToList();
            foreach (var prefabName in keys)
            {
                if (!all.TryGetValue(prefabName, out var data))
                    continue;
                Plugin.LogServerDebug($"{nameof(ReservePrefab)}: {typeof(T).Name} '{prefabName}'");
                try
                {
                    if (PrefabTypeName != null)
                    {
                        if (PrefabRegistry.TryRegisterPrefabType(prefabName, PrefabTypeName, out string registeredTypeName) == false)
                        {
                            Plugin.LogFatal($"Tried to register {typeof(T).Name} '{prefabName}' as type '{PrefabTypeName}' but has already been registered as type '{registeredTypeName}' before by an other OTAB instance. Rename your custom prefab to avoid prefab corruption");
                            DataBase<T>.Drop(prefabName);
                            continue;
                        }
                    }

                    if (reservedPrefabNames.Contains(prefabName))
                    {
                        Plugin.LogError($"{nameof(ReservePrefab)}: {typeof(T).Name} '{prefabName}' already reserved!");
                        DataBase<T>.Drop(prefabName);
                        continue;
                    }

                    if (ReservePrefab(prefabName, data))
                    {
                        reservedPrefabNames.Add(prefabName);
                    }
                    else
                    {
                        // error should be logged in ReservePrefab()
                        DataBase<T>.Drop(prefabName);
                    }
                    
                }
                catch (Exception)
                {
                    Plugin.LogFatal($"{ModelTypeName}.{nameof(ReservePrefab)}() '{prefabName}' failed");
                    throw;
                }
            }
        }

        public bool CallValidateAllPrefabs()
        {
            Plugin.LogDebug($"{nameof(CallValidateAllPrefabs)}: {typeof(T).Name}");
            var all = DataBase<T>.GetAll();
            var keys = all.Keys.ToList();
            var allOkay = true;
            foreach (var prefabName in keys)
            {
                if (!all.TryGetValue(prefabName, out var data))
                    continue;
                Plugin.LogServerDebug($"{nameof(ValidatePrefab)}: {typeof(T).Name} '{prefabName}'");
                try
                {
                    allOkay &= ValidatePrefab(prefabName, data);
                }
                catch (Exception)
                {
                    Plugin.LogFatal($"{ModelTypeName}.{nameof(ValidatePrefab)}() '{prefabName}' failed");
                    throw;
                }
            }
            return allOkay;
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
                Plugin.LogServerDebug($"{nameof(RegisterPrefab)} {typeof(T).Name} '{prefabName}'");
                try
                {
                    RegisterPrefab(prefabName, data);
                }
                catch (Exception)
                {
                    Plugin.LogFatal($"{ModelTypeName}.{nameof(RegisterPrefab)}() '{prefabName}' failed");
                    throw;
                }
            }
        }

        public void CallEditAllPrefabs()
        {
            Plugin.LogDebug($"{nameof(CallEditAllPrefabs)} {typeof(T).Name}");
            var all = DataBase<T>.GetAll();
            var keys = all.Keys.ToList();
            foreach (var prefabName in keys)
            {
                if (!all.TryGetValue(prefabName, out var data))
                    continue;
                Plugin.LogServerDebug($"{nameof(EditPrefab)} {typeof(T).Name} '{prefabName}'");
                try
                {
                    EditPrefab(prefabName, data);
                }
                catch (Exception)
                {
                    Plugin.LogFatal($"{ModelTypeName}.{nameof(EditPrefab)}() '{prefabName}' failed");
                    throw;
                }
            }
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
                Plugin.LogServerDebug($"{nameof(RestorePrefab)} {typeof(T).Name} '{prefabName}'");
                try
                {
                    RestorePrefab(prefabName);
                }
                catch (Exception)
                {
                    Plugin.LogFatal($"{ModelTypeName}.{nameof(RestorePrefab)}() '{prefabName}' failed");
                    throw;
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
