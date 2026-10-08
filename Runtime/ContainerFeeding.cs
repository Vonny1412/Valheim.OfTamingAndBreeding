using System.Collections.Generic;
using UnityEngine;

namespace OfTamingAndBreeding.Runtime
{
    internal static class ContainerFeeding
    {

        private static readonly HashSet<string> s_configPrefabNames = new HashSet<string>();
        private static readonly HashSet<string> s_customPrefabNames = new HashSet<string>();

        private static readonly HashSet<Container> s_allContainers = new HashSet<Container>();
        private static readonly HashSet<Container> s_containers = new HashSet<Container>();

        public static IEnumerable<Container> Containers => s_containers;
        private static readonly Dictionary<Container, Collider[]> s_containerColliders = new Dictionary<Container, Collider[]>();

        public static void RegisterCustomContainerPrefab(string prefabName)
        {
            if (!string.IsNullOrEmpty(prefabName))
            {
                s_customPrefabNames.Add(prefabName);
            }
        }

        public static void RegisterConfigContainerPrefabs(string value)
        {
            s_configPrefabNames.Clear();
            if (!string.IsNullOrWhiteSpace(value))
            {
                foreach (string entry in value.Split(','))
                {
                    string prefabName = entry.Trim();
                    if (!string.IsNullOrEmpty(prefabName))
                    {
                        s_configPrefabNames.Add(prefabName);
                    }
                }
            }
            RefreshContainers();
        }

        private static void RefreshContainers()
        {
            s_containers.Clear();
            s_containerColliders.Clear();
            foreach (Container container in s_allContainers)
            {
                if (!container || !IsRegisteredContainerPrefab(container.gameObject.name))
                {
                    continue;
                }
                s_containerColliders[container] = container.GetComponentsInChildren<Collider>();
                s_containers.Add(container);
            }
        }

        public static bool IsRegisteredContainerPrefab(string containerName)
        {
            string prefabName = Utils.GetPrefabName(containerName);
            return s_customPrefabNames.Contains(prefabName) || s_configPrefabNames.Contains(prefabName);
        }

        public static bool IsRegisteredContainerInstance(Container container)
        {
            return s_containers.Contains(container);
        }

        public static void RegisterContainerInstance(Container container)
        {
            if (!container)
            {
                return;
            }

            s_allContainers.Add(container);

            if (IsRegisteredContainerPrefab(container.gameObject.name))
            {
                s_containerColliders[container] = container.GetComponentsInChildren<Collider>();
                s_containers.Add(container);
            }
        }

        public static void UnregisterContainerInstance(Container container)
        {
            s_allContainers.Remove(container);
            s_containers.Remove(container);
            s_containerColliders.Remove(container);
        }

        public static bool TryGetContainerColliders(Container container, out Collider[] colliders)
        {
            return s_containerColliders.TryGetValue(container, out colliders);
        }

    }
}
