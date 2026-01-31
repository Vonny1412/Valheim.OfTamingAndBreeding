using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OfTamingAndBreeding.Patches
{
    [HarmonyPatch(typeof(CharacterAnimEvent), "CustomFixedUpdate")]
    static class CharacterAnimEvent_CustomFixedUpdate_Patch
    {
        static void Postfix(CharacterAnimEvent __instance)
        {

            var root = __instance.transform.root;
            if (root == null) return;

            var animator = __instance.GetComponent<Animator>();
            if (animator == null) return;

            if (Contexts.DataContext.GetAnimationScaling(Utils.GetPrefabName(__instance.gameObject.name), out float scale))
            {
                animator.speed = scale; // the animations do look nice now!
            }

        }
    }
}
