using HarmonyLib;
using OfTamingAndBreeding.Components.Traits;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace OfTamingAndBreeding.Patches
{
    [HarmonyPatch]
    internal partial class DataReadyPatches : Core.PatchGroup<DataReadyPatches>
    {

        [HarmonyPatch(typeof(SpawnAbility), "Spawn", MethodType.Enumerator)]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> SpawnAbility_Spawn_Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            var callback = AccessTools.Method(typeof(DataReadyPatches), nameof(SpawnAbility_OnSpawned));

            var ownerField = original.DeclaringType
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .SingleOrDefault(f => f.FieldType == typeof(SpawnAbility));

            var codes = new List<CodeInstruction>(instructions);
            if (ownerField == null || callback == null)
            {
                Plugin.LogWarning("SpawnAbility transpiler: Required field or callback not found.");
                return codes;
            }

            int matches = codes.Count(IsSpawnAbilityInstantiate);
            if (matches != 1)
            {
                Plugin.LogWarning($"SpawnAbility Instantiate matches: {matches}. Patch skipped.");
                return codes;
            }

            var result = new List<CodeInstruction>();
            foreach (var code in codes)
            {
                result.Add(code);
                if (IsSpawnAbilityInstantiate(code))
                {
                    result.Add(new CodeInstruction(OpCodes.Ldarg_0));
                    result.Add(new CodeInstruction(OpCodes.Ldfld, ownerField));
                    result.Add(new CodeInstruction(OpCodes.Call, callback));
                }
            }
            return result;
        }

        private static bool IsSpawnAbilityInstantiate(CodeInstruction code)
        {
            if (code.opcode != OpCodes.Call && code.opcode != OpCodes.Callvirt)
                return false;

            if (!(code.operand is MethodInfo method))
                return false;

            if (method.Name != nameof(UnityEngine.Object.Instantiate))
                return false;

            if (method.DeclaringType != typeof(UnityEngine.Object))
                return false;

            var parameters = method.GetParameters();

            return parameters.Length == 3
                && parameters[1].ParameterType == typeof(Vector3)
                && parameters[2].ParameterType == typeof(Quaternion)
                && method.ReturnType == typeof(GameObject);
        }

        private static GameObject SpawnAbility_OnSpawned(GameObject spawned, SpawnAbility ability)
        {
            if (!spawned || !ability)
            {
                return spawned;
            }

            var owner = Traverse.Create(ability).Field("m_owner").GetValue<Character>();
            if (!owner || !owner.IsTamed())
            {
                return spawned;
            }

            if (spawned.TryGetComponent<Character>(out var character))
            {
                character.SetTamed(true);


                //todo: add yaml option (bool) if summons shall follow
                if (character.TryGetComponent<MonsterAI>(out var monsterAI))
                {
                    monsterAI.SetFollowTarget(owner.gameObject);
                }
                else if (character.TryGetComponent<AnimalAITrait>(out var animalAITrait))
                {
                    animalAITrait.SetFollowTarget(owner.gameObject);
                }
            }

            return spawned;
        }

    }
}
