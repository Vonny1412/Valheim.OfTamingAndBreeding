using OfTamingAndBreeding.Components.Base;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace OfTamingAndBreeding.Features
{
    internal static class LocalIdleAnimations
    {

        // todo: make seperate mod fot this

        internal static void RemoveIdleEvents()
        {
            foreach (var prefab in ZNetScene.instance.m_prefabs)
            {
                if (prefab.TryGetComponent<BaseAI>(out var baseAI))
                {
                    RemoveIdleEventsForPrefab(prefab);
                    RemoveIdleSoundsForPrefab(baseAI);
                }
            }
        }

        internal static void RemoveIdleEventsForPrefab(GameObject prefab)
        {
            //Plugin.LogServerWarning($"{prefab.name}");
            foreach (var animator in prefab.GetComponentsInChildren<Animator>(true))
            {
                if (!animator || !animator.runtimeAnimatorController)
                {
                    continue;
                }
                foreach (var clip in animator.runtimeAnimatorController.animationClips)
                {
                    if (!clip || !clip.name.StartsWith("idle", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    foreach (var ev in clip.events)
                    {
                        if (ev.objectReferenceParameter is GameObject go && go && go.TryGetComponent<ZNetView>(out var nview))
                        {
                            //Plugin.LogServerWarning($"  {clip.name} > {go.name}");
                            UnityEngine.Object.DestroyImmediate(nview);
                        }
                    }
                }
            }
        }

        private static void RemoveIdleSoundsForPrefab(BaseAI baseAI)
        {
            foreach (var p in baseAI.m_idleSound.m_effectPrefabs)
            {
                if (p.m_prefab.TryGetComponent<ZNetView>(out var nview))
                {
                    //Plugin.LogServerWarning($"  m_idleSound > {p.m_prefab.name}");
                    UnityEngine.Object.DestroyImmediate(nview);
                }
            }
        }

    }
}
