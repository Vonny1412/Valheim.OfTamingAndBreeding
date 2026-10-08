using System.IO;
using System.Linq;
using UnityEngine;

namespace OfTamingAndBreeding.Utilities
{
    internal static class AnimationUtils
    {

        public static void DumpZSyncAnim(GameObject prefab, StreamWriter writer)
        {

            var zsa = prefab.GetComponent<ZSyncAnimation>();
            if (!zsa) {
                writer.WriteLine($"  (No ZSyncAnimation found)");
                return;
            }

            var a = zsa.GetComponentInChildren<Animator>(true);
            if (!a || !a.runtimeAnimatorController)
            {
                writer.WriteLine($"  (No Animator found)");
                return;
            }

            var ctrl = a.runtimeAnimatorController;

            foreach (var c in ctrl.animationClips.Distinct())
            {
                writer.WriteLine($"  '{c.name}' ({c.length:0.00}s)");
            }
        }

        public static bool AnimationExists(GameObject prefab, string clipName, out AnimationClip animClip)
        {
            animClip = null;

            if (!prefab || string.IsNullOrEmpty(clipName))
                return false;

            var animator = prefab.GetComponentInChildren<Animator>(true);
            if (!animator)
                return false;

            var controller = animator.runtimeAnimatorController;
            if (!controller)
                return false;

            animClip = controller.animationClips.First(c => c && c.name == clipName);
            return animClip != null;
        }
    }
}
