using HarmonyLib;
using OfTamingAndBreeding.Components.Traits;
using System;
using System.Collections.Generic;
using UnityEngine;
using static CharacterAnimEvent;

namespace OfTamingAndBreeding.Patches
{
    [HarmonyPatch]
    internal partial class DataReadyPatches : Base.PatchGroup<DataReadyPatches>
    {
        internal static new void Install() => Base.PatchGroup<DataReadyPatches>.Install();
        internal static new void Uninstall() => Base.PatchGroup<DataReadyPatches>.Uninstall();







        /*



        [HarmonyPatch(typeof(ZNetView), "Awake")]
        [HarmonyPrefix]
        private static void ZNetView_Awake_Prefix(ZNetView __instance)
        {
            TryDumpChickenAnimator();

        }

        private static bool s_dumpedChickenAnimator;

        private static void TryDumpChickenAnimator()
        {
            if (s_dumpedChickenAnimator)
            {
                return;
            }

            if (!ZNetScene.instance)
            {
                return;
            }

            GameObject chicken = ZNetScene.instance.GetPrefab("Chicken".GetStableHashCode());
            if (!chicken)
            {
                return;
            }

            s_dumpedChickenAnimator = true;

            Plugin.LogWarning("=== DumpChickenAnimator ===");
            DumpAnimatorClips(chicken);
            DumpClipEvents(chicken, "Idle Eat");
        }

        public static void DumpClipEvents(GameObject root, string targetClipName)
        {
            Plugin.LogServerWarning("DumpClipEvents");
            if (!root)
            {
                return;
            }

            foreach (var animator in root.GetComponentsInChildren<Animator>(true))
            {
                if (!animator || !animator.runtimeAnimatorController)
                {
                    continue;
                }

                foreach (var clip in animator.runtimeAnimatorController.animationClips)
                {
                    if (!clip || !string.Equals(clip.name, targetClipName, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    Plugin.LogWarning($"Clip events for {clip.name}:");

                    foreach (var ev in clip.events)
                    {
                        string objName = "<null>";
                        if (ev.objectReferenceParameter is GameObject go && go)
                        {
                            objName = Utils.GetPrefabName(go.name);
                        }

                        Plugin.LogWarning(
                            $"  func={ev.functionName} time={ev.time:0.###} string={ev.stringParameter} obj={objName}");
                    }
                }
            }
        }
        public static void DumpAnimatorClips(GameObject root)
        {
            if (!root)
            {
                Plugin.LogWarning("DumpAnimatorClips: root is null");
                return;
            }

            foreach (var animator in root.GetComponentsInChildren<Animator>(true))
            {
                if (!animator || !animator.runtimeAnimatorController)
                {
                    continue;
                }

                string path = GetTransformPath(root.transform, animator.transform);
                Plugin.LogWarning($"Animator found at {path}");
                Plugin.LogWarning($"Controller: {animator.runtimeAnimatorController.name}");

                foreach (var clip in animator.runtimeAnimatorController.animationClips)
                {
                    if (!clip)
                    {
                        continue;
                    }

                    Plugin.LogWarning($"  Clip: {clip.name}");
                }
            }
        }

        private static string GetTransformPath(Transform root, Transform current)
        {
            if (current == root)
            {
                return root.name;
            }

            var parts = new List<string>();
            Transform t = current;
            while (t && t != root)
            {
                parts.Add(t.name);
                t = t.parent;
            }

            parts.Add(root.name);
            parts.Reverse();
            return string.Join("/", parts);
        }
        */


















        /*

        [HarmonyPatch(typeof(AnimationEffect), "Effect", new[] { typeof(AnimationEvent) })]
        [HarmonyPrefix]
        private static void AnimationEffect_Effect_Prefix(AnimationEffect __instance, AnimationEvent e)
        {
            if (!__instance || e == null)
            {
                return;
            }

            GameObject fxPrefab = e.objectReferenceParameter as GameObject;
            if (!fxPrefab)
            {
                return;
            }

            string ownerName = Utils.GetPrefabName(__instance.gameObject.name);
            string fxName = Utils.GetPrefabName(fxPrefab.name);
            string clipName = "<no clip>";
            string stringParam = e.stringParameter ?? "";
            Vector3 pos = __instance.transform.position;

            try
            {
                if (e.animatorClipInfo.clip)
                {
                    clipName = e.animatorClipInfo.clip.name;
                }
            }
            catch
            {
            }

            Plugin.LogWarning(
                $"AnimEffect owner={ownerName} clip={clipName} fx={fxName} joint={stringParam} pos=({pos.x:0.0},{pos.y:0.0},{pos.z:0.0}) znetview=true");
        }

        */


        /*

        [HarmonyPatch(typeof(ZNetScene), "CreateObject")]
        [HarmonyPostfix]
        private static void ZNetScene_CreateObject_Postfix(ZNetScene __instance, ZDO zdo)
        {
            if (zdo == null) return;

            int prefabHash = zdo.GetPrefab();
            if (prefabHash == 0) return;

            GameObject prefab = __instance.GetPrefab(prefabHash);
            string prefabName = prefab ? prefab.name : $"<unknown:{prefabHash}>";

            Vector3 pos = zdo.GetPosition();
            ZDOID id = zdo.m_uid;

            if (!ShouldLog(prefabName, pos))
            {
                return;
            }

            Plugin.LogServerWarning(
                $"CreateObject prefab={prefabName} id={id} pos=({pos.x:0.0},{pos.y:0.0},{pos.z:0.0})");
        }

        


        [HarmonyPatch(typeof(ZDOMan), "CreateNewZDO", new[] { typeof(ZDOID), typeof(Vector3), typeof(int) })]
        [HarmonyPostfix]
        private static void ZDOMan_CreateNewZDO_Postfix(ZDO __result, ZDOID uid, Vector3 position, int prefabHashIn)
        {
            string prefabName = prefabHashIn != 0 && ZNetScene.instance
                ? ZNetScene.instance.GetPrefab(prefabHashIn)?.name ?? $"<unknown:{prefabHashIn}>"
                : "<none>";

            if (!ShouldLog(prefabName, position))
            {
                return;
            }

            Plugin.LogServerWarning(
                $"CreateNewZDO prefab={prefabName} id={uid} pos=({position.x:0.0},{position.y:0.0},{position.z:0.0})");
        }

        [HarmonyPatch(typeof(ZDOMan), "DestroyZDO")]
        [HarmonyPrefix]
        private static void ZDOMan_DestroyZDO_Prefix(ZDO zdo)
        {
            if (zdo == null)
            {
                return;
            }

            int prefabHash = zdo.GetPrefab();
            string prefabName = prefabHash != 0 && ZNetScene.instance
                ? ZNetScene.instance.GetPrefab(prefabHash)?.name ?? $"<unknown:{prefabHash}>"
                : "<none>";

            Vector3 pos = zdo.GetPosition();

            if (!ShouldLog(prefabName, pos))
            {
                return;
            }

            Plugin.LogServerWarning(
                $"DestroyZDO prefab={prefabName} id={zdo.m_uid} pos=({pos.x:0.0},{pos.y:0.0},{pos.z:0.0})");
        }

        [HarmonyPatch(typeof(ZNetScene), "Destroy", new[] { typeof(GameObject) })]
        [HarmonyPrefix]
        private static void ZNetScene_Destroy_Prefix(GameObject go)
        {
            if (!go) return;

            ZNetView nview = go.GetComponent<ZNetView>();
            string prefabName = Utils.GetPrefabName(go.name);

            if (nview && nview.GetZDO() != null)
            {
                Vector3 pos = nview.GetZDO().GetPosition();

                if (!ShouldLog(prefabName, pos))
                {
                    return;
                }

                Plugin.LogServerWarning(
                    $"DestroyInstance prefab={prefabName} id={nview.GetZDO().m_uid} pos=({pos.x:0.0},{pos.y:0.0},{pos.z:0.0})");
            }
            else
            {
                if (!ShouldLog(prefabName, Vector3.zero))
                {
                    return;
                }

                Plugin.LogServerWarning($"DestroyInstance prefab={prefabName} id=<no zdo>");
            }
        }

        private static bool ShouldLog(string prefabName, Vector3 pos)
        {
            if (string.IsNullOrEmpty(prefabName))
            {
                return false;
            }

            if (prefabName.IndexOf("chicken", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            if (prefabName.IndexOf("egg", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            return false;
        }


        */



    }
}
