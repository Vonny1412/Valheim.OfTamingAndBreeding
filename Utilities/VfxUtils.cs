using System;
using System.Collections.Generic;
using UnityEngine;

namespace OfTamingAndBreeding.Utilities
{
    internal static class VfxUtils
    {

        private static bool IsActiveInPrefab(Transform transform, Transform root)
        {
            var current = transform;

            while (current && current != root)
            {
                if (!current.gameObject.activeSelf)
                {
                    return false;
                }

                current = current.parent;
            }

            return true;
        }

        public static void DebugVfx(GameObject root)
        {
            foreach (var ps in root.GetComponentsInChildren<ParticleSystem>(true))
            {
                if (IsActiveInPrefab(ps.transform, root.transform))
                {
                    Plugin.LogMessage($"  {nameof(ParticleSystem)}: {ps.gameObject.name}");
                }
            }
            foreach (var light in root.GetComponentsInChildren<Light>(true))
            {
                if (IsActiveInPrefab(light.transform, root.transform))
                {
                    Plugin.LogMessage($"  {nameof(Light)}: {light.gameObject.name}");
                }
            }
            foreach (var trail in root.GetComponentsInChildren<TrailRenderer>(true))
            {
                if (IsActiveInPrefab(trail.transform, root.transform))
                {
                    Plugin.LogMessage($"  {nameof(TrailRenderer)}: {trail.gameObject.name}");
                }
            }
            foreach (var line in root.GetComponentsInChildren<LineRenderer>(true))
            {
                if (IsActiveInPrefab(line.transform, root.transform))
                {
                    Plugin.LogMessage($"  {nameof(LineRenderer)}: {line.gameObject.name}");
                }
            }
        }

        public static void DisableVfx(GameObject root, IEnumerable<string> names)
        {
            if (names == null)
            {
                return;
            }

            var disableSet = new HashSet<string>(
                names,
                StringComparer.OrdinalIgnoreCase
            );

            foreach (var ps in root.GetComponentsInChildren<ParticleSystem>(true))
            {
                if (IsActiveInPrefab(ps.transform, root.transform) && disableSet.Contains(ps.gameObject.name))
                {
                    var emission = ps.emission;
                    emission.enabled = false;
                }
            }

            foreach (var light in root.GetComponentsInChildren<Light>(true))
            {
                if (IsActiveInPrefab(light.transform, root.transform) && disableSet.Contains(light.gameObject.name))
                {
                    light.enabled = false;
                    var lightLod = light.GetComponent<LightLod>();
                    if (lightLod)
                    {
                        lightLod.enabled = false;
                    }
                    // what about lightflicker?
                }
            }

            foreach (var trail in root.GetComponentsInChildren<TrailRenderer>(true))
            {
                if (IsActiveInPrefab(trail.transform, root.transform) && disableSet.Contains(trail.gameObject.name))
                {
                    trail.enabled = false;
                }
            }

            foreach (var line in root.GetComponentsInChildren<LineRenderer>(true))
            {
                if (IsActiveInPrefab(line.transform, root.transform) && disableSet.Contains(line.gameObject.name))
                {
                    line.enabled = false;
                }
            }
        }

        public static void ScaleVfx(GameObject root, float scale)
        {
            if (!root || Mathf.Approximately(scale, 1f))
                return;

            foreach (var ps in root.GetComponentsInChildren<ParticleSystem>(true))
            {
                
                // Scale emitter position inside the VFX prefab
                if (ps.transform != root.transform)
                {
                    ps.transform.localPosition *= scale;
                }
                
                var main = ps.main;

                main.startSizeMultiplier *= scale;
                main.startSpeedMultiplier *= scale;
                main.gravityModifierMultiplier *= scale;

                var shape = ps.shape;
                if (shape.enabled)
                {
                    // Important: emission offset
                    shape.position *= scale;

                    shape.radius *= scale;
                    shape.scale *= scale;
                }

                var vel = ps.velocityOverLifetime;
                if (vel.enabled)
                {
                    vel.xMultiplier *= scale;
                    vel.yMultiplier *= scale;
                    vel.zMultiplier *= scale;
                }
            }

            foreach (var l in root.GetComponentsInChildren<Light>(true))
            {
                l.range *= scale;
            }

            foreach (var tr in root.GetComponentsInChildren<TrailRenderer>(true))
            {
                tr.widthMultiplier *= scale;
            }

            foreach (var lr in root.GetComponentsInChildren<LineRenderer>(true))
            {
                lr.widthMultiplier *= scale;
            }
        }


        public static void RestoreVfx(GameObject currentRoot, GameObject backupRoot)
        {
            if (!currentRoot || !backupRoot)
                return;

            RestoreComponents<ParticleSystem>(currentRoot, backupRoot, (current, backup) =>
            {
                var currentEmission = current.emission;
                var backupEmission = backup.emission;
                currentEmission.enabled = backupEmission.enabled;

                if (current.transform != currentRoot.transform)
                {
                    current.transform.localPosition = backup.transform.localPosition;
                }

                var currentMain = current.main;
                var backupMain = backup.main;

                currentMain.startSizeMultiplier = backupMain.startSizeMultiplier;
                currentMain.startSpeedMultiplier = backupMain.startSpeedMultiplier;
                currentMain.gravityModifierMultiplier = backupMain.gravityModifierMultiplier;

                var currentShape = current.shape;
                var backupShape = backup.shape;

                if (currentShape.enabled)
                {
                    currentShape.position = backupShape.position;
                    currentShape.radius = backupShape.radius;
                    currentShape.scale = backupShape.scale;
                }

                var currentVelocity = current.velocityOverLifetime;
                var backupVelocity = backup.velocityOverLifetime;

                if (currentVelocity.enabled && backupVelocity.enabled)
                {
                    currentVelocity.xMultiplier = backupVelocity.xMultiplier;
                    currentVelocity.yMultiplier = backupVelocity.yMultiplier;
                    currentVelocity.zMultiplier = backupVelocity.zMultiplier;
                }
            });

            RestoreComponents<Light>(currentRoot, backupRoot, (current, backup) =>
            {
                current.enabled = backup.enabled;
                current.range = backup.range;
                var currentLightLod = current.GetComponent<LightLod>();
                var backupLightLod = backup.GetComponent<LightLod>();
                if (currentLightLod && backupLightLod)
                {
                    currentLightLod.enabled = backupLightLod.enabled;
                }
            });

            RestoreComponents<TrailRenderer>(currentRoot, backupRoot, (current, backup) =>
            {
                current.enabled = backup.enabled;
                current.widthMultiplier = backup.widthMultiplier;
            });

            RestoreComponents<LineRenderer>(currentRoot, backupRoot, (current, backup) =>
            {
                current.enabled = backup.enabled;
                current.widthMultiplier = backup.widthMultiplier;
            });
        }

        private static void RestoreComponents<T>(
            GameObject currentRoot,
            GameObject backupRoot,
            Action<T, T> restore)
            where T : Component
        {
            var currentComponents = currentRoot.GetComponentsInChildren<T>(true);

            foreach (var current in currentComponents)
            {
                var path = GetRelativePath(currentRoot.transform, current.transform);
                var backupTransform = backupRoot.transform.Find(path);

                if (!backupTransform)
                    continue;

                var currentOnTransform = current.transform.GetComponents<T>();
                var backupOnTransform = backupTransform.GetComponents<T>();

                var index = Array.IndexOf(currentOnTransform, current);

                if (index < 0 || index >= backupOnTransform.Length)
                    continue;

                restore(current, backupOnTransform[index]);
            }
        }

        private static string GetRelativePath(Transform root, Transform target)
        {
            if (target == root)
                return "";

            var path = target.name;
            var current = target.parent;

            while (current && current != root)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }


    }
}
