using Jotunn;
using Jotunn.Entities;
using Jotunn.Managers;
using OfTamingAndBreeding.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OfTamingAndBreeding.Data.Processing.Base
{

    public class PrefabRegistry
    {
        //---------------------
        // prevent cross-server custom-prefab type corruption
        //---------------------

        private static readonly Dictionary<string, string> globalRegisteredPrefabTypes = new Dictionary<string, string>();

        public static bool RegisterPrefabType(string prefabName, string prefabTypeName, out string registeredPrefabTypeName)
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

        //---------------------
        // static/global
        //---------------------

        // each original prefab gets its own backup prefab
        protected static readonly Dictionary<string, GameObject> originalPrefabBackups = new Dictionary<string, GameObject>();

        // custom prefabs created by OTAB to be used ingame
        protected static readonly Dictionary<string, GameObject> customPrefabs = new Dictionary<string, GameObject>();

        // list of custom prefab backups
        // each custom prefab gets its own backup
        // eg:
        // GrowingAbomination1 -> "OTAB_BACKUP_(Abomination)_CUSTOM_0"
        // GrowingAbomination2 -> "OTAB_BACKUP_(Abomination)_CUSTOM_1"
        // GrowingAbomination3 -> "OTAB_BACKUP_(Abomination)_CUSTOM_2"
        protected static readonly Dictionary<string, List<GameObject>> customPrefabBackups = new Dictionary<string, List<GameObject>>();

        //---------------------
        // instance/session
        //---------------------

        // custom prefabs might get used multiple times, like an offspring that also is used for creature data
        // and eggs need to be pre-created so they can be used in creatures offpsirngs list
        // for that we are reserving them: prefabs are cloned if needed and can be used in onfollowing process-steps
        private readonly Dictionary<string, GameObject> reservedPrefabsByName;

        // pool of unused entries from customPrefabBackups
        // it this pool runs out of backups additional backups will be created and directly added to customPrefabBackups for next server/world joining
        private readonly Dictionary<string, List<GameObject>> unusedCustomPrefabBackups = new Dictionary<string, List<GameObject>>();

        // register of current used backups of custom prefabs created by otab
        private readonly Dictionary<string, GameObject> currentCustomPrefabBackup = new Dictionary<string, GameObject>();

        //
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

        public PrefabRegistry()
        {
            reservedPrefabsByName = new Dictionary<string, GameObject>();
            foreach(var kv in customPrefabBackups)
            {
                unusedCustomPrefabBackups[kv.Key] = kv.Value.ToList();
            }
        }

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

        public GameObject MakeCustomBackup(GameObject templatePrefab)
        {
            var prefabName = templatePrefab.name;

            if (!customPrefabBackups.TryGetValue(prefabName, out var backList))
            {
                customPrefabBackups[prefabName] = backList = new List<GameObject>();
            }

            var backupName = $"OTAB_BACKUP_({prefabName})_CUSTOM_{backList.Count}";
            Plugin.LogDebug($"{nameof(MakeCustomBackup)}() for {prefabName} ({backupName})");

            var backup = PrefabManager.Instance.GetPrefab(backupName);
            if (backup == null)
            {
                backup = PrefabManager.Instance.CreateClonedPrefab(backupName, templatePrefab);
            }
            backList.Add(backup);
            return backup;
        }

        public bool IsCustomPrefab(string prefabName)
        {
            return customPrefabs.ContainsKey(prefabName);
        }

        public GameObject GetCustomPrefab(string prefabName)
        {
            if (customPrefabs.TryGetValue(prefabName, out var prefab))
            {
                return prefab;
            }
            return null;
        }

        public GameObject MakeOriginalBackup(GameObject originalPrefab)
        {
            var prefabName = originalPrefab.name;
            if (originalPrefabBackups.TryGetValue(prefabName, out var backup1))
            {
                return backup1;
            }

            var backupName = $"OTAB_BACKUP_({prefabName})_ORIGINAL";
            Plugin.LogDebug($"MakeOriginalBackup() for {prefabName} ({backupName})");

            var backup = PrefabManager.Instance.CreateClonedPrefab(backupName, originalPrefab);
            originalPrefabBackups.Add(prefabName, backup);
            return backup;
        }

        public GameObject CreateClonedPrefab(string newName, string cloneFromName)
        {
            var clone = PrefabManager.Instance.CreateClonedPrefab(newName, cloneFromName);
            customPrefabs.Add(newName, clone);
            return clone;
        }
        /*
        public GameObject CreateClonedPrefab(string newName, GameObject cloneFrom)
        {
            var clone = PrefabManager.Instance.CreateClonedPrefab(newName, cloneFrom);
            customPrefabs.Add(newName, clone);
            return clone;
        }
        */
        public GameObject CreateClonedItemPrefab(string newName, string cloneFromName)
        {
            CustomItem eggCustom = new CustomItem(newName, cloneFromName);
            var clone = eggCustom.ItemPrefab;
            customPrefabs.Add(newName, clone);
            return clone;
        }

        public GameObject GetOriginalPrefab(string prefabName)
        {
            return PrefabManager.Instance.GetPrefab(prefabName);
        }







        public void SetCustomPrefabUsingBackup(string customPrefabName, GameObject templatePrefab)
        {
            currentCustomPrefabBackup.Add(customPrefabName, templatePrefab);
        }

        public GameObject GetUnusedCustomPrefabBackup(GameObject prefab)
        {
            var prefabName = prefab.name;
            if (unusedCustomPrefabBackups.TryGetValue(prefabName, out var queue) && queue.Count != 0)
            {
                var bak = queue[0];
                queue.RemoveAt(0);
                return bak;
            }
            return MakeCustomBackup(prefab);
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








        public void Restore(string prefabName, Action<GameObject, GameObject> cb)
        {
            var current = GetReservedPrefab(prefabName);
            if (current == null)
            {
                // for safety: prefab is completly unknown. should not happen but whatever
                return;
            }

            if (IsCustomPrefab(prefabName))
            {
                if (currentCustomPrefabBackup.TryGetValue(prefabName, out var backup))
                {
                    Plugin.LogDebug($"Restoring cloned prefab {prefabName} ({backup.name})");
                    cb(backup, current);
                }
                else
                {
                    // we got no backup data for that prefab
                    // only restore prefabs with existing backups
                }
            }
            else
            {
                if (PrefabRegistry.originalPrefabBackups.TryGetValue(prefabName, out var backup))
                {
                    Plugin.LogDebug($"Restoring original prefab {prefabName} ({backup.name})");
                    cb(backup, current);
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
