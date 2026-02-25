using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OfTamingAndBreeding.Helpers
{
    internal static class RestoreHelper
    {





        private static string GetPath(Transform t, Transform root)
        {
            var parts = new Stack<string>();
            while (t != null && t != root)
            {
                parts.Push(t.name);
                t = t.parent;
            }
            return string.Join("/", parts);
        }

        private static void CopyPublicFields(Component src, Component dst)
        {
            var t = src.GetType();
            foreach (var f in t.GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                if (f.IsInitOnly) continue;
                try
                {
                    f.SetValue(dst, f.GetValue(src));
                }
                catch
                {
                    // dirty: keep going
                }
            }
        }


        //------------------------------
        // single components
        //------------------------------

        public static void RestoreComponent<T>(GameObject backup, GameObject current) where T : Component
        {
            var src = backup.GetComponent<T>();
            var dst = current.GetComponent<T>();

            if (src == null)
            {
                if (dst != null) UnityEngine.Object.DestroyImmediate(dst);
                return;
            }

            if (dst == null) dst = current.AddComponent<T>();
            CopyPublicFields(src, dst);
        }



        //------------------------------
        // children list
        //------------------------------

        private static Dictionary<string, T> MapByPath<T>(GameObject go) where T : Component
        {
            var root = go.transform;
            var map = new Dictionary<string, T>(StringComparer.Ordinal);

            foreach (var c in go.GetComponentsInChildren<T>(true))
            {
                if (!c) continue;
                var p = GetPath(c.transform, root);
                map[p] = c;
            }
            return map;
        }

        public static void RestoreChildRenderers(GameObject backup, GameObject current)
        {
            var bMap = MapByPath<Renderer>(backup);
            var cMap = MapByPath<Renderer>(current);

            foreach (var kv in cMap)
            {
                var path = kv.Key;
                var curR = kv.Value;

                if (!bMap.TryGetValue(path, out var bakR) || !bakR) continue;

                // destroy instanced materials we created (to avoid leaks)
                var curMats = curR.sharedMaterials ?? Array.Empty<Material>();
                var bakMats = bakR.sharedMaterials ?? Array.Empty<Material>();

                // Anything that isn't exactly the backup material is likely instanced by you
                for (int i = 0; i < curMats.Length; i++)
                {
                    var m = curMats[i];
                    if (!m) continue;

                    bool isSameAsBackup = (i < bakMats.Length && bakMats[i] == m);
                    if (!isSameAsBackup)
                        UnityEngine.Object.DestroyImmediate(m);
                }

                // restore original
                curR.sharedMaterials = bakMats;
                curR.enabled = bakR.enabled;
                curR.shadowCastingMode = bakR.shadowCastingMode;
                curR.receiveShadows = bakR.receiveShadows;
            }
        }

        public static void RestoreChildLights(GameObject backup, GameObject current)
        {
            var bMap = MapByPath<Light>(backup);
            var cMap = MapByPath<Light>(current);

            foreach (var kv in cMap)
            {
                var path = kv.Key;
                var cur = kv.Value;

                if (!bMap.TryGetValue(path, out var bak) || !bak) continue;

                cur.enabled = bak.enabled;
                cur.color = bak.color;
                cur.range = bak.range;
                cur.intensity = bak.intensity;
                cur.spotAngle = bak.spotAngle;
                cur.type = bak.type;
            }
        }

        public static void RestoreChildParticleRenderers(GameObject backup, GameObject current)
        {
            var bMap = MapByPath<ParticleSystemRenderer>(backup);
            var cMap = MapByPath<ParticleSystemRenderer>(current);

            foreach (var kv in cMap)
            {
                var path = kv.Key;
                var cur = kv.Value;

                if (!bMap.TryGetValue(path, out var bak) || !bak) continue;

                cur.enabled = bak.enabled;
                cur.sharedMaterial = bak.sharedMaterial;
                cur.renderMode = bak.renderMode;
            }
        }

        public static void RestoreChildLightFlickerBaseColor(GameObject backup, GameObject current)
        {

            // map by path for MonoBehaviours whose type name is LightFlicker
            var bList = backup.GetComponentsInChildren<MonoBehaviour>(true)
                .Where(mb => mb && mb.GetType().Name == "LightFlicker")
                .ToList();
            var cList = current.GetComponentsInChildren<MonoBehaviour>(true)
                .Where(mb => mb && mb.GetType().Name == "LightFlicker")
                .ToList();

            // build path map
            var bMap = bList.ToDictionary(mb => GetPath(mb.transform, backup.transform), mb => mb);
            var cMap = cList.ToDictionary(mb => GetPath(mb.transform, current.transform), mb => mb);

            foreach (var kv in cMap)
            {
                var path = kv.Key;
                var cur = kv.Value;

                if (!bMap.TryGetValue(path, out var bak)) continue;

                var t = cur.GetType();
                var f = t.GetField("m_baseColor",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);

                if (f != null && f.FieldType == typeof(Color))
                    f.SetValue(cur, (Color)f.GetValue(bak));
            }
        }

        // unused
        public static void RestoreItemIcons(GameObject backup, GameObject current)
        {

            // Usually only one ItemDrop per item prefab, but handle multiple just in case
            var backupDrops = backup.GetComponentsInChildren<ItemDrop>(true);
            var currentDrops = current.GetComponentsInChildren<ItemDrop>(true);

            if (backupDrops == null || backupDrops.Length == 0 || currentDrops == null || currentDrops.Length == 0)
                return;

            // Map by transform-path so we match the correct ItemDrop if there are multiple
            var backupMap = MapByPath<ItemDrop>(backup);
            var currentMap = MapByPath<ItemDrop>(current);

            foreach (var kv in currentMap)
            {
                var path = kv.Key;
                var curDrop = kv.Value;

                if (!backupMap.TryGetValue(path, out var bakDrop) || !bakDrop)
                    continue;

                var curIcons = curDrop.m_itemData?.m_shared?.m_icons;
                var bakIcons = bakDrop.m_itemData?.m_shared?.m_icons;

                // Cleanup: destroy sprites/textures that are not part of backup
                CleanupIconSprites(curIcons, bakIcons);

                // Restore from backup (clone array to avoid aliasing)
                if (bakIcons != null)
                {
                    curDrop.m_itemData.m_shared.m_icons = (Sprite[])bakIcons.Clone();
                }
                else
                {
                    curDrop.m_itemData.m_shared.m_icons = null;
                }
            }
        }

        // unused
        private static void CleanupIconSprites(Sprite[] currentIcons, Sprite[] backupIcons)
        {
            if (currentIcons == null || currentIcons.Length == 0)
                return;

            // Put backup sprites in a set for fast reference checks
            HashSet<Sprite> backupSet = null;
            if (backupIcons != null && backupIcons.Length > 0)
                backupSet = new HashSet<Sprite>(backupIcons);

            for (int i = 0; i < currentIcons.Length; i++)
            {
                var s = currentIcons[i];
                if (!s) continue;

                // If the sprite exists in backup, it’s original -> keep it
                if (backupSet != null && backupSet.Contains(s))
                    continue;

                // Otherwise, it’s likely a dynamically created tinted sprite
                // We can attempt to destroy its texture (if it looks like our generated one)
                try
                {
                    var tex = s.texture;
                    if (tex && tex.name != null && tex.name.ToLower().EndsWith("_tinted"))
                    {
                        UnityEngine.Object.DestroyImmediate(tex);
                    }
                }
                catch {  }

                // Destroy the sprite asset itself (safe if it was dynamically created)
                UnityEngine.Object.DestroyImmediate(s);
            }
        }
        
        













    }
}
