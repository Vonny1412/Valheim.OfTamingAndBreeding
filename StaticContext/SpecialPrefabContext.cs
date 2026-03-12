using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OfTamingAndBreeding.StaticContext
{
    internal static class SpecialPrefabContext
    {
        private const string SpecialPrefabPrefix = "@otab=";
        private const string SpecialPrefabName = "_OTAB_SPECIAL_PREFAB_";
        private static readonly Dictionary<int, GameObject> specialPrefabs = new Dictionary<int, GameObject>();

        public static bool IsSpecialPrefab(string anyName)
        {
            return anyName != null && anyName.StartsWith(SpecialPrefabName, StringComparison.Ordinal); // allows also "_OTAB_SPECIAL_PREFAB_[hash](Clone)"
        }

        public static bool IsSpecialPrefabCommand(string prefabName)
        {
            return prefabName.StartsWith(SpecialPrefabPrefix, StringComparison.OrdinalIgnoreCase);
        }

        // returns true if prefab has been created
        // returns false if prefab already exists
        public static bool CreateSpecialPrefabFromCommand(string command, out GameObject prefab)
        {
            var nameHash = command.GetStableHashCode();
            if (specialPrefabs.TryGetValue(nameHash, out prefab))
            {
                return false;
            }

            // ---

            var uniqueSpecialPrefabName = SpecialPrefabName + nameHash;
            prefab = PrefabManager.Instance.CreateEmptyPrefab(uniqueSpecialPrefabName, false);

            foreach (var payload in SplitCommand(command[SpecialPrefabPrefix.Length..], '|'))
            {
                if (payload.Length == 0)
                {
                    continue;
                }

                var subCommandParts = SplitCommand(payload, ':');
                var subCommand = subCommandParts[0].Trim();
                switch (subCommand.ToLowerInvariant())
                {

                    case "anyitem":
                        var itemDrop = prefab.AddComponent<ItemDrop>();
                        var customSharedName = subCommandParts.Length > 1 ? subCommandParts[1].Trim() : string.Empty;
                        itemDrop.m_itemData.m_shared = new ItemDrop.ItemData.SharedData
                        {
                            m_name = customSharedName,
                        };
                        var comparer = prefab.AddComponent<SpecialPrefabs.OTABSpecialConsumeAnyItem>();
                        break;

                    // more can be added here

                    default:
                        Plugin.LogWarning($"Unknown special prefab command '{subCommand}'");
                        break;
                }

            }

            // ---

            specialPrefabs.Add(nameHash, prefab);
            return true;
        }

        private static string[] SplitCommand(string input, char separator)
        {
            var result = new List<string>();
            var sb = new StringBuilder();

            bool inSingleQuote = false;
            bool inDoubleQuote = false;

            foreach (char c in input)
            {
                if (c == '\'' && !inDoubleQuote)
                {
                    inSingleQuote = !inSingleQuote;
                    continue;
                }

                if (c == '"' && !inSingleQuote)
                {
                    inDoubleQuote = !inDoubleQuote;
                    continue;
                }

                if (c == separator && !inSingleQuote && !inDoubleQuote)
                {
                    result.Add(sb.ToString());
                    sb.Clear();
                    continue;
                }

                sb.Append(c);
            }

            result.Add(sb.ToString());
            return result.ToArray();
        }
    }
}
