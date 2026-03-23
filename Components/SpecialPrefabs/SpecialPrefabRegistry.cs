using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace OfTamingAndBreeding.Components.SpecialPrefabs
{
    public static class SpecialPrefabRegistry
    {
        private const string SpecialPrefabPrefix = "@otab=";
        private const string SpecialPrefabName = "_OTAB_SPECIAL_PREFAB_";
        private static readonly Dictionary<int, GameObject> specialPrefabs;

        static SpecialPrefabRegistry()
        {
            specialPrefabs = new Dictionary<int, GameObject>();

            Net.NetworkSessionManager.Instance.OnSessionClosed += (netsess, dataLoaded) => {
                // do not clear ! keep it as cache
                //specialPrefabs.Clear();
            };





            // todo: "Decoy Bait"
            // todo: "Admin Bait"



        }

        public static bool IsSpecialPrefabCommand(string command)
        {
            if (command == null)
            {
                return false;
            }
            if (command.Length == 0)
            {
                return false;
            }
            return command.StartsWith(SpecialPrefabPrefix, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsSpecialPrefab(GameObject prefab)
        {
            if (!prefab)
            {
                return false;
            }
            var prefabName = prefab.name;
            if (prefabName == null)
            {
                return false;
            }
            if (prefabName.Length == 0)
            {
                return false;
            }
            return prefabName.CustomStartsWith(SpecialPrefabName); // allows also "_OTAB_SPECIAL_PREFAB_[hash](Clone)"
        }

        // returns true if prefab has been created
        // returns false if prefab already exists
        public static bool CreateSpecialPrefabFromCommand(string command, out GameObject prefab)
        {
            var commandHash = command.GetStableHashCode();
            if (specialPrefabs.TryGetValue(commandHash, out prefab))
            {
                return false;
            }

            // ---

            var uniqueSpecialPrefabName = SpecialPrefabName + commandHash;
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
                        var sharedName = subCommandParts.Length > 1 ? subCommandParts[1].Trim() : string.Empty;
                        var component = OTABSpecialConsumeAnyItem.AddComponentToSpecialPrefab(prefab, sharedName);
                        OTABSpecialConsumeAnyItem.Register(component); // need to registter ourself because Awake never gets called
                        break;

                    // more can be added

                    default:
                        Plugin.LogWarning($"Unknown special prefab command '{subCommand}'");
                        break;
                }

            }

            // ---

            specialPrefabs.Add(commandHash, prefab);
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
