using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OfTamingAndBreeding.Helpers
{
    public static class AnimationHelper
    {
        public static Animator FindAnimatorFromZSync(ZSyncAnimation zsa)
        {
            if (!zsa) return null;
            var a = zsa.GetComponentInChildren<Animator>(true);
            return a;
        }

        public static void DumpZSyncAnim(ZSyncAnimation zsa, string tag = "")
        {
            if (!zsa) { Plugin.LogMessage($"{tag} ZSyncAnimation: <null>"); return; }

            var a = FindAnimatorFromZSync(zsa);
            Plugin.LogMessage($"{tag} ZSyncAnimation on {zsa.name}, Animator={(a ? a.name : "<null>")}");

            if (!a || !a.runtimeAnimatorController)
            {
                Plugin.LogMessage($"{tag} No RuntimeAnimatorController found.");
                return;
            }

            var ctrl = a.runtimeAnimatorController;

            Plugin.LogMessage($"{tag} Params:");
            foreach (var p in a.parameters)
                Plugin.LogMessage($"{tag}  - {p.name} [{p.type}]");

            Plugin.LogMessage($"{tag} Clips:");
            foreach (var c in ctrl.animationClips.Distinct())
                Plugin.LogMessage($"{tag}  - {c.name} ({c.length:0.00}s loop={(c.isLooping ? "yes" : "no")})");
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
