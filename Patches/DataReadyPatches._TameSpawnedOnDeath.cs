using HarmonyLib;
using OfTamingAndBreeding.Components.Traits;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace OfTamingAndBreeding.Patches
{
    [HarmonyPatch]
    internal partial class DataReadyPatches : Core.PatchGroup<DataReadyPatches>
    {

        [ThreadStatic]
        private static CharacterTrait s_currentDeathDropTrait;


        [HarmonyPatch(typeof(CharacterDrop), "OnDeath")]
        [HarmonyPrefix]
        private static void CharacterDrop_OnDeath_Prefix(CharacterDrop __instance)
        {
            s_currentDeathDropTrait = null;
            if (CharacterTrait.TryGet(__instance.gameObject, out var characterTrait) && characterTrait.m_tameSpawnedOnDeath && characterTrait.GetCharacter().IsTamed())
            {
                s_currentDeathDropTrait = characterTrait;
            }
        }

        [HarmonyPatch(typeof(CharacterDrop), "OnDeath")]
        [HarmonyFinalizer]
        private static void CharacterDrop_OnDeath_Finalizer()
        {
            s_currentDeathDropTrait = null;
        }

        private static void TameSpawnedOnDeath(GameObject spawnedObject)
        {
            if (s_currentDeathDropTrait && spawnedObject.TryGetComponent<Character>(out var character))
            {
                character.SetTamed(true);
            }
        }

        [HarmonyPatch(typeof(CharacterDrop), "DropItems")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> CharacterDrop_DropItems_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var processSpawnedObject = AccessTools.Method(typeof(DataReadyPatches), nameof(TameSpawnedOnDeath), new[] { typeof(GameObject) });
            var patched = false;

            foreach (var instruction in instructions)
            {
                yield return instruction;

                if (!patched &&
                    instruction.operand is MethodInfo method &&
                    method.Name == nameof(UnityEngine.Object.Instantiate) &&
                    method.IsGenericMethod &&
                    method.GetGenericArguments().Length == 1 &&
                    method.GetGenericArguments()[0] == typeof(GameObject))
                {
                    yield return new CodeInstruction(OpCodes.Dup);
                    yield return new CodeInstruction(OpCodes.Call, processSpawnedObject);
                    patched = true;
                }
            }

            if (!patched)
            {
                Plugin.LogError("Failed to patch CharacterDrop.DropItems: Instantiate<GameObject> call not found.");
            }
        }

    }
}
