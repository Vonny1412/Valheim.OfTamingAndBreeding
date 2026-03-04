using HarmonyLib;
using OfTamingAndBreeding.Components;
using UnityEngine;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {

        [HarmonyPatch(typeof(Animator), "SetTrigger", new[] { typeof(string) })]
        [HarmonyPostfix]
        private static void Animator_SetTrigger_Postfix(Animator __instance, string name)
        {
            if (name != "consume") return;

            var character = __instance.GetComponentInParent<Character>();
            if (!character) return;

            var prefabName = global::Utils.GetPrefabName(character.gameObject.name);
            if (string.IsNullOrEmpty(prefabName)) return;

            var runner = character.GetComponent<ConsumeAnimationClipOverlay>();
            if (runner)
            {
                runner.PlayOverlay(__instance, speed: 1f);
            }

        }

    }
}
