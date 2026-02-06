using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OfTamingAndBreeding.Patches
{

    /*
    [HarmonyPatch(typeof(CharacterAnimEvent), "CustomFixedUpdate")]
    static class CharacterAnimEvent_CustomFixedUpdate_Patch
    {
        static void Postfix(CharacterAnimEvent __instance)
        {

            //var root = __instance.transform.root;
            //if (root == null) return;

            var animator = __instance.GetComponent<Animator>();
            if (animator == null) return;

            if (Contexts.DataContext.GetAnimationScaling(Utils.GetPrefabName(__instance.gameObject.name), out float scale))
            {
                animator.speed = scale; // the animations do look nice now!
            }

        }
    }
    */

    //
    // ScaleLocomotion
    //

    [HarmonyPatch(typeof(ZSyncAnimation), "SetFloat", new[] { typeof(int), typeof(float) })]
    static class ZSyncAnimation_SetFloat_Patch
    {

        /*
        private static readonly int forward_speed = ZSyncAnimation.GetHash("forward_speed");
        private static readonly int sideway_speed = ZSyncAnimation.GetHash("sideway_speed");
        private static readonly int anim_speed = ZSyncAnimation.GetHash("anim_speed");
        private static readonly int turn_speed = ZSyncAnimation.GetHash("turn_speed");
        private static readonly int inWater = ZSyncAnimation.GetHash("inWater");
        private static readonly int onGround = ZSyncAnimation.GetHash("onGround");
        private static readonly int encumbered = ZSyncAnimation.GetHash("encumbered");
        private static readonly int flying = ZSyncAnimation.GetHash("flying");
        */

        static void Prefix(ZSyncAnimation __instance, int hash, ref float value)
        {
            if (!__instance) return;

            //if (hash != s_forwardSpeedID && hash != s_sidewaySpeedID && hash != s_animSpeedID)
            //    return;

            var name = Utils.GetPrefabName(__instance.gameObject.name);
            if (!Contexts.DataContext.GetAnimationScaling(name, out float scale))
                return;

            value *= scale;
        }
    }


}
