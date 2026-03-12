using System;
using System.Collections.Generic;

namespace OfTamingAndBreeding.Components.Base
{
    internal static class OTABComponentRegistry
    {
        internal static readonly HashSet<Type> registeredTypes = new HashSet<Type>();

        public static void RemoveComponentsFromPrefabs()
        {
            foreach (var prefab in ZNetScene.instance.m_prefabs)
            {
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
            registeredTypes.Clear();
        }

    }
}
