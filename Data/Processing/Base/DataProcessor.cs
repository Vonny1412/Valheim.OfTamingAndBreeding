using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;

namespace OfTamingAndBreeding.Data.Processing.Base
{

    internal abstract class DataProcessor<T> : IDataProcessor where T : DataBase<T>
    {

        public abstract string DirectoryName { get; }

        public string ModelTypeName => typeof(T).Name;

        public abstract string GetDataKey(string fileName);

        public bool LoadFromYaml(string prefabName, string yamlText)
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
                Plugin.LogFatal(Helpers.YamlHelper.FormatException(
                    e,
                    yamlText,
                    $"Failed loading YAML for {typeof(T).Name} '{prefabName}'"
                ));
            }

            return false;
        }

        public bool LoadFromFile(string file)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            var yamlText = File.ReadAllText(file);
            var fileNameParsed = GetDataKey(file);
            if (fileNameParsed != null)
            {
                // file name => prefab name => data key
                fileName = fileNameParsed;
            }
            return LoadFromYaml(fileName, yamlText);
        }

        public Dictionary<string, string> GetAllYamlData()
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
        // context routine
        //---------------------

        public abstract void Prepare(DataProcessorContext ctx);

        public abstract bool ValidateData(DataProcessorContext ctx, string prefabName, T data);
        public void ValidateAllData(DataProcessorContext ctx)
        {
            Plugin.LogDebug($"{nameof(ValidateAllData)}: {typeof(T).Name}");
            var all = DataBase<T>.GetAll();
            var keys = all.Keys.ToList();
            foreach (var prefabName in keys)
            {
                if (!all.TryGetValue(prefabName, out var data))
                    continue;
                Plugin.LogDebug($"{nameof(ValidateData)}: {typeof(T).Name} '{prefabName}'");
                if (!ValidateData(ctx, prefabName, data))
                {
                    DataBase<T>.Drop(prefabName);
                }
            }
        }

        public abstract bool PreparePrefab(DataProcessorContext ctx, string prefabName, T data);
        public void PrepareAllPrefabs(DataProcessorContext ctx)
        {
            Plugin.LogDebug($"{nameof(PrepareAllPrefabs)}: {typeof(T).Name}");
            var all = DataBase<T>.GetAll();
            var keys = all.Keys.ToList();
            foreach (var prefabName in keys)
            {
                if (!all.TryGetValue(prefabName, out var data))
                    continue;
                Plugin.LogDebug($"{nameof(PreparePrefab)}: {typeof(T).Name} '{prefabName}'");
                if (!PreparePrefab(ctx, prefabName, data))
                {
                    DataBase<T>.Drop(prefabName);
                }
            }
        }

        public abstract bool ValidatePrefab(DataProcessorContext ctx, string prefabName, T data);
        public bool ValidateAllPrefabs(DataProcessorContext ctx)
        {
            Plugin.LogDebug($"{nameof(ValidateAllPrefabs)}: {typeof(T).Name}");
            var all = DataBase<T>.GetAll();
            var keys = all.Keys.ToList();
            var allOkay = true;
            foreach (var prefabName in keys)
            {
                if (!all.TryGetValue(prefabName, out var data))
                    continue;
                Plugin.LogDebug($"{nameof(ValidatePrefab)}: {typeof(T).Name} '{prefabName}'");
                allOkay &= ValidatePrefab(ctx, prefabName, data);
            }
            return allOkay;
        }

        public abstract void RegisterPrefab(DataProcessorContext ctx, string prefabName, T data);
        public void RegisterAllPrefabs(DataProcessorContext ctx)
        {
            Plugin.LogDebug($"{nameof(RegisterAllPrefabs)} {typeof(T).Name}");
            var all = DataBase<T>.GetAll();
            var keys = all.Keys.ToList();
            foreach (var prefabName in keys)
            {
                if (!all.TryGetValue(prefabName, out var data))
                    continue;
                Plugin.LogDebug($"{nameof(RegisterPrefab)} {typeof(T).Name} '{prefabName}'");
                RegisterPrefab(ctx, prefabName, data);
            }
        }

        public abstract void Finalize(DataProcessorContext ctx);

        public abstract void RestorePrefab(DataProcessorContext ctx, string prefabName, T data);
        public void RestoreAllPrefabs(DataProcessorContext ctx)
        {
            Plugin.LogDebug($"{nameof(RestoreAllPrefabs)} {typeof(T).Name}");
            var all = DataBase<T>.GetAll();
            var keys = all.Keys.ToList();
            foreach (var prefabName in keys)
            {
                if (!all.TryGetValue(prefabName, out var data))
                    continue;
                Plugin.LogDebug($"{nameof(RestorePrefab)} {typeof(T).Name} '{prefabName}'");
                RestorePrefab(ctx, prefabName, data);
            }
        }

        public abstract void Cleanup(DataProcessorContext ctx);

    }

}
