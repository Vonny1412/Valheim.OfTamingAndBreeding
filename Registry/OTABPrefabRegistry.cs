using Jotunn;
using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OfTamingAndBreeding.Registry
{
    internal class OTABPrefabRegistry // We need a unique name here that doesn't conflict with other classes, such as those from Jotunn.
    {

        //--------------------------------------------------
        // prevent cross-server custom-prefab corruption

        private static readonly Dictionary<string, string> globalRegisteredPrefabTypes = new Dictionary<string, string>();

        public static bool TryRegisterPrefabType(string prefabName, string prefabTypeName, out string registeredPrefabTypeName)
        {
            registeredPrefabTypeName = null;
            if (globalRegisteredPrefabTypes.TryGetValue(prefabName, out registeredPrefabTypeName))
            {
                // todo: put error log here
                return prefabTypeName == registeredPrefabTypeName;
            }
            globalRegisteredPrefabTypes.Add(prefabName, prefabTypeName);
            return true;
        }

        private static readonly Dictionary<string, string> globalRegisteredPrefabCloneSources = new Dictionary<string, string>();

        public static bool TryRegisterPrefabCloneSource(string prefabName, string cloneFromName, out string registeredCloneFromName)
        {
            registeredCloneFromName = null;
            if (globalRegisteredPrefabCloneSources.TryGetValue(prefabName, out registeredCloneFromName))
            {
                // todo: put error log here
                return cloneFromName == registeredCloneFromName; // valid
            }
            globalRegisteredPrefabCloneSources.Add(prefabName, cloneFromName);
            return true;
        }

        //--------------------------------------------------
        // lifetime backups

        private static bool originalPrefabsSaved = false;
        private static readonly HashSet<string> originalPrefabNames = new HashSet<string>();

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
        private static readonly Dictionary<string, List<GameObject>> customPrefabBackups = new Dictionary<string, List<GameObject>>();

        public static void SaveOriginalPrefabNames()
        {
            if (originalPrefabsSaved)
            {
                return;
            }
            originalPrefabsSaved = true;
            foreach (var prefab in ZNetScene.instance.m_prefabs)
            {
                originalPrefabNames.Add(prefab.name);
            }
        }

        public static bool IsOriginalPrefab(string prefabName)
        {
            return originalPrefabNames.Contains(prefabName);
        }

        public static bool IsCustomPrefab(string prefabName)
        {
            return IsOriginalPrefab(prefabName) == false;
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

        // custom prefabs might get used multiple times, like an offspring that also is used for creature data
        // and eggs need to be pre-created so they can be used in creatures offpsirngs list
        // for that we are reserving them: prefabs are cloned if needed and can be used in onfollowing process-steps
        private readonly Dictionary<string, GameObject> reservedPrefabsByName = new Dictionary<string, GameObject>();

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
            if (!TryRegisterPrefabCloneSource(prefabName, cloneFromName, out var registeredCloneFromName))
            {
                Plugin.LogFatal($"Custom prefab '{prefabName}' was previously cloned from '{registeredCloneFromName}', but is now requested to clone from '{cloneFromName}'. Custom prefab names must be unique across OTAB instances.");
                return null;
            }

            var custom = PrefabManager.Instance.CreateClonedPrefab(prefabName, cloneFromName);
            customPrefabs.Add(prefabName, custom);

            var backup = MakeCustomBackup(cloneFromName);
            SetCustomPrefabUsingBackup(prefabName, backup);

            return custom;
        }

        public GameObject ReactivateCustomPrefab(string prefabName, string cloneFromName)
        {
            if (!TryRegisterPrefabCloneSource(prefabName, cloneFromName, out var registeredCloneFromName))
            {
                Plugin.LogFatal($"Custom prefab '{prefabName}' was previously cloned from '{registeredCloneFromName}', but is now requested to clone from '{cloneFromName}'. Custom prefab names must be unique across OTAB instances.");
                return null;
            }

            var custom = customPrefabs[prefabName];
            var backup = GetUnusedCustomPrefabBackup(cloneFromName);
            //Plugin.LogServerWarning($"--- {backup.name}");
            //RestorePrefabFromBackup(custom, backup); // todo: this can be deleted if everything is working fine
            SetCustomPrefabUsingBackup(prefabName, backup);
            return custom;
        }

        private void RestorePrefabFromBackup(GameObject current, GameObject backup)
        {
            PrefabUtils.RestoreComponent<AnimalAI>(current, backup);
            PrefabUtils.RestoreComponent<MonsterAI>(current, backup);
            
            PrefabUtils.RestoreComponent<Character>(current, backup);
            PrefabUtils.RestoreComponent<CharacterDrop>(current, backup);
            PrefabUtils.RestoreComponent<EggGrow>(current, backup);
            PrefabUtils.RestoreComponent<Floating>(current, backup);

            PrefabUtils.RestoreComponent<Growup>(current, backup);
            PrefabUtils.RestoreComponent<ItemDrop>(current, backup);
            PrefabUtils.RestoreComponent<Pet>(current, backup);
            PrefabUtils.RestoreComponent<Procreation>(current, backup);
            PrefabUtils.RestoreComponent<Ragdoll>(current, backup);
            PrefabUtils.RestoreComponent<Sadle>(current, backup);
            PrefabUtils.RestoreComponent<Tameable>(current, backup);
        }

        private GameObject MakeCustomBackup(string prefabName)
        {
            if (!customPrefabBackups.TryGetValue(prefabName, out var backList))
            {
                customPrefabBackups[prefabName] = backList = new List<GameObject>();
            }

            var backupName = $"OTAB_BACKUP_{prefabName}_CUSTOM_{backList.Count}";
            Plugin.LogDebug($"{nameof(MakeCustomBackup)}() for {prefabName} ({backupName})");

            var backup = PrefabManager.Instance.GetPrefab(backupName);
            if (backup == null)
            {
                backup = PrefabManager.Instance.CreateClonedPrefab(backupName, prefabName);
            }
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

        public GameObject GetOriginalPrefab(string prefabName)
        {
            return PrefabManager.Instance.GetPrefab(prefabName);
        }

        public void MakeOriginalBackup(string prefabName)
        {
            if (!IsOriginalPrefab(prefabName))
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

        public GameObject GetReservedPrefab(string prefabName)
        {
            if (reservedPrefabsByName.TryGetValue(prefabName, out var prefab))
            {
                return prefab;
            }
            return null;
        }

        public void ReservePrefab(string prefabName, GameObject prefab)
        {
            reservedPrefabsByName.Add(prefabName, prefab);
        }

        public bool PrefabExists(string prefabName)
        {
            if (reservedPrefabsByName.TryGetValue(prefabName, out _))
            {
                return true;
            }
            if ((bool)GetOriginalPrefab(prefabName))
            {
                return true;
            }
            return false;
        }

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
                UnityEngine.Object.DestroyImmediate(c);
        }

        public void RestorePrefab(string prefabName, Action<GameObject, GameObject> restoreProcessorState)
        {
            var current = GetReservedPrefab(prefabName);
            if (current == null)
            {
                return;
            }

            GameObject backup = null;
            if (IsOriginalPrefab(prefabName))
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

            Plugin.LogDebug($"Restoring {(IsOriginalPrefab(prefabName) ? "original" : "cloned")} prefab {prefabName} ({backup.name})");
            RestorePrefabFromBackup(current, backup);
            restoreProcessorState?.Invoke(current, backup);
        }




    }
}
