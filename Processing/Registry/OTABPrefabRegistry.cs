using Jotunn;
using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OfTamingAndBreeding.Processing.Registry
{
    internal class OTABPrefabRegistry // We need a unique name here that doesn't conflict with other classes, such as those from Jotunn.
    {

        //--------------------------------------------------
        // prevent cross-server custom-prefab corruption

        private static readonly Dictionary<string, string> globalRegisteredPrefabTypes = new Dictionary<string, string>();

        public static bool TryRegisterPrefabType(string prefabName, string prefabTypeName)
        {
            if (globalRegisteredPrefabTypes.TryGetValue(prefabName, out var registeredPrefabTypeName))
            {
                if (prefabTypeName != registeredPrefabTypeName)
                {
                    Plugin.LogFatal(
                        $"Tried to register '{prefabName}' as type '{prefabTypeName}' " +
                        $"but has already been registered as type '{registeredPrefabTypeName}' before by another OTAB instance. " +
                        $"Rename your custom prefab to avoid prefab corruption");
                    return false;
                }
                return true;
            }
            globalRegisteredPrefabTypes.Add(prefabName, prefabTypeName);
            return true;
        }

        private static readonly Dictionary<string, string> globalRegisteredPrefabCloneSources = new Dictionary<string, string>();

        public static bool TryRegisterPrefabCloneSource(string prefabName, string cloneFromName)
        {
            if (globalRegisteredPrefabCloneSources.TryGetValue(prefabName, out var registeredCloneFromName))
            {
                if (cloneFromName != registeredCloneFromName)
                {
                    Plugin.LogFatal($"Custom prefab '{prefabName}' was previously cloned from '{registeredCloneFromName}', but is now requested to clone from '{cloneFromName}'. Custom prefab names must be unique across OTAB instances.");
                    return false;
                }
                return true;
            }
            globalRegisteredPrefabCloneSources.Add(prefabName, cloneFromName);
            return true;
        }

        //--------------------------------------------------
        // lifetime backups

        // each original prefab gets its own backup prefab
        private static readonly Dictionary<string, GameObject> originalPrefabBackups = new Dictionary<string, GameObject>();

        // custom prefabs created by OTAB to be used ingame
        private static readonly Dictionary<string, GameObject> customPrefabs = new Dictionary<string, GameObject>();

        // list of custom prefab backups
        // each custom prefab gets its own backup
        // eg:
        // GrowingAbomination1 -> "OTAB_BACKUP_Abomination_CUSTOM_0"
        // GrowingAbomination2 -> "OTAB_BACKUP_Abomination_CUSTOM_1"
        // GrowingAbomination3 -> "OTAB_BACKUP_Abomination_CUSTOM_2"
        // => customPrefabBackups["Abomination"] = [
        //      OTAB_BACKUP_Abomination_CUSTOM_0,
        //      OTAB_BACKUP_Abomination_CUSTOM_1,
        //      OTAB_BACKUP_Abomination_CUSTOM_2,
        //      ...
        // ]
        private static readonly Dictionary<string, List<GameObject>> customPrefabBackups = new Dictionary<string, List<GameObject>>();

        public static bool IsCustomPrefab(string prefabName)
        {
            // if its not returned by jotunns PrefabManager it means it WILL be a custom prefab
            return customPrefabs.ContainsKey(prefabName) || !PrefabManager.Instance.GetPrefab(prefabName);
        }

        //--------------------------------------------------
        // Singleton

        private static OTABPrefabRegistry _instance;
        public static OTABPrefabRegistry Instance => _instance;

        public static void CreateInstance()
        {
            _instance = new OTABPrefabRegistry();
        }

        public static void DestroyInstance()
        {
            _instance = null;
        }

        public OTABPrefabRegistry()
        {
            foreach (var kv in customPrefabBackups)
            {
                unusedCustomPrefabBackups[kv.Key] = kv.Value.ToList();
            }
        }

        //--------------------------------------------------
        // instance for current session

        private readonly List<string> reservedPrefabNames = new List<string>();

        // pool of unused entries from customPrefabBackups
        // it this pool runs out of backups additional backups will be created and directly added to customPrefabBackups for next server/world joining
        private readonly Dictionary<string, List<GameObject>> unusedCustomPrefabBackups = new Dictionary<string, List<GameObject>>();

        // register of current used backups of custom prefabs created by otab
        private readonly Dictionary<string, GameObject> currentCustomPrefabBackup = new Dictionary<string, GameObject>();

        //--------------------------------------------------
        // custom prefabs

        public GameObject GetCustomPrefab(string prefabName)
        {
            if (customPrefabs.TryGetValue(prefabName, out var prefab))
            {
                return prefab;
            }
            return null;
        }

        public GameObject CreateCustomPrefab(string prefabName, string cloneFromName)
        {
            if (IsCustomPrefab(cloneFromName))
            {
                Plugin.LogFatal($"Custom prefab '{prefabName}' cannot be cloned from '{cloneFromName}' because '{cloneFromName}' is not a valid original prefab.");
                return null;
            }

            if (!TryRegisterPrefabCloneSource(prefabName, cloneFromName))
            {
                return null;
            }

            var custom = PrefabManager.Instance.CreateClonedPrefab(prefabName, cloneFromName);
            customPrefabs.Add(prefabName, custom);

            var backup = GetUnusedCustomPrefabBackup(cloneFromName);
            SetCustomPrefabUsingBackup(prefabName, backup);

            return custom;
        }

        public GameObject ReactivateCustomPrefab(string prefabName, string cloneFromName)
        {
            if (!TryRegisterPrefabCloneSource(prefabName, cloneFromName))
            {
                return null;
            }

            var custom = customPrefabs[prefabName];
            var backup = GetUnusedCustomPrefabBackup(cloneFromName);
            //Plugin.LogServerWarning($"--- {backup.name}");
            //RestorePrefabFromBackup(custom, backup); // todo: this can be deleted if everything is working fine
            SetCustomPrefabUsingBackup(prefabName, backup);
            return custom;
        }

        private GameObject MakeCustomBackup(string prefabName)
        {
            if (!customPrefabBackups.TryGetValue(prefabName, out var backList))
            {
                customPrefabBackups[prefabName] = backList = new List<GameObject>();
            }

            var backupName = $"OTAB_BACKUP_{prefabName}_CUSTOM_{backList.Count}";
            Plugin.LogDebug($"{nameof(MakeCustomBackup)}() for {prefabName} ({backupName})");

            var backup = PrefabManager.Instance.GetPrefab(backupName) ?? PrefabManager.Instance.CreateClonedPrefab(backupName, prefabName);
            backList.Add(backup);
            return backup;
        }

        private void SetCustomPrefabUsingBackup(string customPrefabName, GameObject backup)
        {
            currentCustomPrefabBackup.Add(customPrefabName, backup);
        }

        private GameObject GetUnusedCustomPrefabBackup(string prefabName)
        {
            if (unusedCustomPrefabBackups.TryGetValue(prefabName, out var queue) && queue.Count != 0)
            {
                var bak = queue[0];
                queue.RemoveAt(0);
                return bak;
            }
            return MakeCustomBackup(prefabName);
        }

        //--------------------------------------------------
        // original prefabs


        public GameObject GetRegisteredPrefab(string prefabName)
        {
            return PrefabManager.Instance.GetPrefab(prefabName);
        }





        public void MakeOriginalBackup(string prefabName)
        {
            if (IsCustomPrefab(prefabName))
            {
                return;
            }
            if (originalPrefabBackups.ContainsKey(prefabName))
            {
                return;
            }

            var backupName = $"OTAB_BACKUP_{prefabName}_ORIGINAL";
            Plugin.LogDebug($"MakeOriginalBackup() for {prefabName} ({backupName})");

            var backup = PrefabManager.Instance.CreateClonedPrefab(backupName, prefabName);
            originalPrefabBackups.Add(prefabName, backup);
        }

        //--------------------------------------------------
        // reserve/register/restore prefabs

        public bool ReservePrefabName(string prefabName)
        {
            if (reservedPrefabNames.Contains(prefabName))
            {
                return false;
            }
            reservedPrefabNames.Add(prefabName);
            return true;
        }

        public bool PrefabWillExist(string prefabName)
        {
            //return reservedPrefabNames.Contains(prefabName) || originalPrefabNames.Contains(prefabName);
            return reservedPrefabNames.Contains(prefabName) || (bool)PrefabManager.Instance.GetPrefab(prefabName);
        }


        //--------------------------------------------------
        // editing





        public T GetOrAddComponent<T>(string prefabName, GameObject go) where T : Component
        {
            // prefabName is currently unused, but may be needed for tracking/restoring component changes in the future.
            return go.GetOrAddComponent<T>();
        }

        public void DestroyComponentIfExists<T>(string prefabName, GameObject obj) where T : UnityEngine.Object
        {
            // prefabName is currently unused, but may be needed for tracking/restoring component changes in the future.
            T c = obj.GetComponent<T>();
            if (c != null)
            {
                UnityEngine.Object.DestroyImmediate(c);
            }
        }

        public void RestorePrefab(string prefabName, Action<GameObject, GameObject> restoreProcessorState)
        {
            var current = GetRegisteredPrefab(prefabName);
            if (current == null)
            {
                return;
            }

            GameObject backup;
            var isOriginal = IsCustomPrefab(prefabName) == false;
            if (isOriginal)
            {
                originalPrefabBackups.TryGetValue(prefabName, out backup);
            }
            else
            {
                currentCustomPrefabBackup.TryGetValue(prefabName, out backup);
            }
            if (!backup)
            {
                return;
            }

            Plugin.LogDebug($"Restoring {(!IsCustomPrefab(prefabName) ? "original" : "cloned")} prefab {prefabName} ({backup.name})");
            restoreProcessorState?.Invoke(current, backup);

            if (!isOriginal)
            {
                PrefabManager.Instance.RemovePrefab(prefabName);
            }
        }





    }
}
