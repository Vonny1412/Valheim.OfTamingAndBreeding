using Jotunn;
using Jotunn.Managers;
using OfTamingAndBreeding.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OfTamingAndBreeding.Registry
{
    internal class PrefabRegistry : Common.SingletonClass<PrefabRegistry>
    {

        //--------------------------------------------------
        // deep example:
        //
        // Server "ChickenServer" uses custom egg prefab "OTAB_TestEgg", cloned from ChickenEgg
        // Server "DragonServer" uses custom egg prefab "OTAB_TestEgg", cloned from DragonEgg
        //
        // problem: both are using same prefab name "OTAB_TestEgg"
        // what happens:
        //
        // - user joins "ChickenServer"
        // - custom prefab "OTAB_TestEgg" created, cloned from ChickenEgg
        // - new backup: "OTAB_BACKUP_(ChickenEgg)_CUSTOM_0"
        // - currentCustomPrefabBackup["OTAB_TestEgg"] = "OTAB_BACKUP_(ChickenEgg)_CUSTOM_0"
        // - backup added to static customPrefabBackups
        // - custom prefab "OTAB_TestEgg" gets setup (components add/remove/edit)
        //
        // - user leaves server
        // - "OTAB_TestEgg" gets restored by "OTAB_BACKUP_(ChickenEgg)_CUSTOM_0"
        //
        // - user enters "DragonServer"
        // - prefab "OTAB_TestEgg" already exists
        // - reactivate "OTAB_TestEgg" by "DragonEgg":
        //   - "OTAB_BACKUP_(DragonEgg)_CUSTOM_0" does not exist in static customPrefabBackups
        //   - new backup: "OTAB_BACKUP_(DragonEgg)_CUSTOM_0"
        //   - backup added to static customPrefabBackups
        //   - "OTAB_TestEgg" gets restored by "OTAB_BACKUP_(DragonEgg)_CUSTOM_0"
        //   - currentCustomPrefabBackup["OTAB_TestEgg"] = "OTAB_BACKUP_(DragonEgg)_CUSTOM_0"
        // - custom prefab "OTAB_TestEgg" gets setup (components add/remove/edit)
        //
        // - user leaves server
        // - "OTAB_TestEgg" gets restored by "OTAB_BACKUP_(DragonEgg)_CUSTOM_0"
        //
        // - and goes back to "ChickenServer"
        // - prefab "OTAB_TestEgg" already exists
        // - reactivate "OTAB_TestEgg" by "ChickenEgg":
        //   - "OTAB_BACKUP_(ChickenEgg)_CUSTOM_0" exists in static customPrefabBackups
        //   - "OTAB_TestEgg" gets restored by "OTAB_BACKUP_(ChickenEgg)_CUSTOM_0"
        //   - currentCustomPrefabBackup["OTAB_TestEgg"] = "OTAB_BACKUP_(ChickenEgg)_CUSTOM_0"
        // - custom prefab "OTAB_TestEgg" gets setup (components add/remove/edit)
        //

        //--------------------------------------------------
        // prevent cross-server custom-prefab type corruption

        private static readonly Dictionary<string, string> globalRegisteredPrefabTypes = new Dictionary<string, string>();

        public static bool TryRegisterPrefabType(string prefabName, string prefabTypeName, out string registeredPrefabTypeName)
        {
            registeredPrefabTypeName = null;
            if (globalRegisteredPrefabTypes.TryGetValue(prefabName, out registeredPrefabTypeName))
            {
                var registeredTypeMatches = prefabTypeName == registeredPrefabTypeName;
                return registeredTypeMatches; // types match. no need to register. prefab name/type is valid
            }
            // not registered yet
            globalRegisteredPrefabTypes.Add(prefabName, prefabTypeName);
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
                var isImportant = false;
                isImportant |= (bool)prefab.GetComponent<AnimalAI>();
                isImportant |= (bool)prefab.GetComponent<BaseAI>();
                isImportant |= (bool)prefab.GetComponent<Character>();
                isImportant |= (bool)prefab.GetComponent<EggGrow>();
                isImportant |= (bool)prefab.GetComponent<Growup>();
                isImportant |= (bool)prefab.GetComponent<ItemDrop>();
                isImportant |= (bool)prefab.GetComponent<MonsterAI>();
                isImportant |= (bool)prefab.GetComponent<Procreation>();
                isImportant |= (bool)prefab.GetComponent<Tameable>();
                if (isImportant)
                {
                    originalPrefabNames.Add(prefab.name);
                }
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

        protected override void OnCreate()
        {
            foreach (var kv in customPrefabBackups)
            {
                unusedCustomPrefabBackups[kv.Key] = kv.Value.ToList();
            }
        }

        protected override void OnDestroy()
        {
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
            var custom = PrefabManager.Instance.CreateClonedPrefab(prefabName, cloneFromName);
            customPrefabs.Add(prefabName, custom);

            var backup = MakeCustomBackup(cloneFromName);
            SetCustomPrefabUsingBackup(prefabName, backup);

            return custom;
        }

        public GameObject ReactivateCustomPrefab(string prefabName, string cloneFromName)
        {
            var custom = customPrefabs[prefabName];
            var backup = GetUnusedCustomPrefabBackup(cloneFromName);
            RestorePrefabFromBackup(custom, backup);
            SetCustomPrefabUsingBackup(prefabName, backup);
            return custom;
        }

        private void RestorePrefabFromBackup(GameObject current, GameObject backup)
        {
            PrefabUtils.RestoreComponent<AnimalAI>(backup, current);
            PrefabUtils.RestoreComponent<BaseAI>(backup, current);
            PrefabUtils.RestoreComponent<Character>(backup, current);
            PrefabUtils.RestoreComponent<CharacterDrop>(backup, current);
            PrefabUtils.RestoreComponent<EggGrow>(backup, current);
            PrefabUtils.RestoreComponent<Floating>(backup, current);
            PrefabUtils.RestoreComponent<Growup>(backup, current);
            PrefabUtils.RestoreComponent<ItemDrop>(backup, current);
            PrefabUtils.RestoreComponent<MonsterAI>(backup, current);
            PrefabUtils.RestoreComponent<Pet>(backup, current);
            PrefabUtils.RestoreComponent<Procreation>(backup, current);
            PrefabUtils.RestoreComponent<Ragdoll>(backup, current);
            PrefabUtils.RestoreComponent<Sadle>(backup, current);
            PrefabUtils.RestoreComponent<Tameable>(backup, current);
        }

        private GameObject MakeCustomBackup(string prefabName)
        {
            if (!customPrefabBackups.TryGetValue(prefabName, out var backList))
            {
                customPrefabBackups[prefabName] = backList = new List<GameObject>();
            }

            var backupName = $"OTAB_BACKUP_{prefabName}_CUSTOM_{backList.Count}";
            Plugin.LogServerDebug($"{nameof(MakeCustomBackup)}() for {prefabName} ({backupName})");

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
            Plugin.LogServerDebug($"MakeOriginalBackup() for {prefabName} ({backupName})");

            var backup = PrefabManager.Instance.CreateClonedPrefab(backupName, prefabName);
            originalPrefabBackups.Add(prefabName, backup);
        }

        //--------------------------------------------------
        // reserve/register prefabs

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

        public bool PrefabExists(string prefabName, bool requireRegistered = false)
        {
            if (!requireRegistered && reservedPrefabsByName.TryGetValue(prefabName, out _))
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
            return go.GetOrAddComponent<T>();
        }

        public void DestroyComponentIfExists<T>(string prefabName, GameObject obj) where T : UnityEngine.Object
        {
            T c = obj.GetComponent<T>();
            if (c != null) UnityEngine.Object.DestroyImmediate(c);
        }

        public bool GetCurrentCustomPrefabBackup(string prefabName, GameObject backup)
        {
            return currentCustomPrefabBackup.TryGetValue(prefabName, out backup);
        }

        public void RestorePrefab(string prefabName, Action<GameObject, GameObject> cb)
        {
            var current = GetReservedPrefab(prefabName);
            if (current == null)
            {
                return;
            }

            if (IsOriginalPrefab(prefabName))
            {
                if (originalPrefabBackups.TryGetValue(prefabName, out var backup) && backup.IsValid())
                {
                    Plugin.LogServerDebug($"Restoring original prefab {prefabName} ({backup.name})");
                    cb?.Invoke(current, backup);
                }
                else
                {
                    // we got no backup data for that prefab
                    // only restore prefabs with existing backups
                }
            }
            else
            {
                if (currentCustomPrefabBackup.TryGetValue(prefabName, out var backup) && backup.IsValid())
                {
                    Plugin.LogServerDebug($"Restoring cloned prefab {prefabName} ({backup.name})");
                    cb?.Invoke(current, backup);
                }
                else
                {
                    // we got no backup data for that prefab
                    // only restore prefabs with existing backups
                }
            }

        }

    }
}
