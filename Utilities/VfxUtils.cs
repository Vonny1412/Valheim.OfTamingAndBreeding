using System;
using UnityEngine;

namespace OfTamingAndBreeding.Utilities
{
    internal static class VfxUtils
    {

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
                current.range = backup.range;
            });

            RestoreComponents<TrailRenderer>(currentRoot, backupRoot, (current, backup) =>
            {
                current.widthMultiplier = backup.widthMultiplier;
            });

            RestoreComponents<LineRenderer>(currentRoot, backupRoot, (current, backup) =>
            {
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
