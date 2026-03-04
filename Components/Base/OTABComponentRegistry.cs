using Jotunn;
using System;
using System.Collections.Generic;

namespace OfTamingAndBreeding.Components.Base
{
    internal static class OTABComponentRegistry
    {
        public static readonly HashSet<Type> registeredTypes = new HashSet<Type>();

        public static void RemoveComponentsFromPrefabs()
        {
            foreach (var prefab in ZNetScene.instance.m_prefabs)
            {
                if (!prefab.IsValid())
                {
                    continue;
                }
                // dirty AND effective
                foreach (Type t in registeredTypes)
                {
                    var addedComponent = prefab.GetComponent(t);
                    if (addedComponent)
                    {
                        Plugin.LogServerDebug($"Removing OTABComponent '{t.Name}' from prefab '{prefab.name}'");
                        UnityEngine.Object.DestroyImmediate(addedComponent);
                    }
                }
            }
        }

    }
}
