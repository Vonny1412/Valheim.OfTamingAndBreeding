using HarmonyLib;
using OfTamingAndBreeding.Components;
using OfTamingAndBreeding.ValheimAPI;
using TMPro;
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

            var prefabName = Utils.GetPrefabName(character.gameObject.name);
            if (string.IsNullOrEmpty(prefabName)) return;

            var runner = character.GetComponent<AnimationClipOverlay>();
            if (runner)
            {
                character.SetLookDir(character.transform.forward);
                var baseAI = character.GetComponent<BaseAI>();
                if (baseAI)
                {
                    baseAI.StopMoving();
                }
                runner.PlayOverlay(__instance, speed: 1f);
            }

        }

    }
}
