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
            // Resolve the methods used for spawn initialization and summon counting.
            var spawnCallback = AccessTools.Method(typeof(DataReadyPatches), nameof(SpawnAbility_OnSpawned));
            var countCallback = AccessTools.Method(typeof(DataReadyPatches), nameof(SpawnAbility_GetInstanceCount));
            var vanillaCount = AccessTools.Method(typeof(SpawnSystem), nameof(SpawnSystem.GetNrOfInstances),
                new[] { typeof(GameObject), typeof(Vector3), typeof(float), typeof(bool), typeof(bool)});

            // Retrieve the SpawnAbility instance from the compiler-generated coroutine.
            var ownerField = original.DeclaringType
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .SingleOrDefault(f => f.FieldType == typeof(SpawnAbility));

            var codes = new List<CodeInstruction>(instructions);

            if (ownerField == null || spawnCallback == null || countCallback == null || vanillaCount == null)
            {
                Plugin.LogWarning("SpawnAbility transpiler: Required members not found.");
                return codes;
            }

            // Validate both injection points to avoid patching unexpected IL.
            int spawnMatches = codes.Count(IsSpawnAbilityInstantiate);
            int countMatches = codes.Count(c => (c.opcode == OpCodes.Call || c.opcode == OpCodes.Callvirt) && Equals(c.operand, vanillaCount));

            if (spawnMatches != 1)
                Plugin.LogWarning($"SpawnAbility Instantiate matches: {spawnMatches}.");

            if (countMatches != 1)
                Plugin.LogWarning($"SpawnAbility instance count matches: {countMatches}.");

            var result = new List<CodeInstruction>();

            foreach (var code in codes)
            {
                if (spawnMatches == 1 && IsSpawnAbilityInstantiate(code))
                {
                    // Process newly spawned creatures while preserving the original GameObject return value.
                    result.Add(code);
                    result.Add(new CodeInstruction(OpCodes.Ldarg_0));
                    result.Add(new CodeInstruction(OpCodes.Ldfld, ownerField));
                    result.Add(new CodeInstruction(OpCodes.Call, spawnCallback));
                }
                else if (countMatches == 1 && (code.opcode == OpCodes.Call || code.opcode == OpCodes.Callvirt) && Equals(code.operand, vanillaCount))
                {
                    // Replace vanilla counting with a tame-aware version, passing the summoner as an extra argument.
                    result.Add(new CodeInstruction(OpCodes.Ldarg_0));
                    result.Add(new CodeInstruction(OpCodes.Ldfld, ownerField));
                    code.opcode = OpCodes.Call;
                    code.operand = countCallback;
                    result.Add(code);
                }
                else
                {
                    result.Add(code);
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

        private static int SpawnAbility_GetInstanceCount(GameObject prefab, Vector3 center, float maxRange, bool eventCreaturesOnly, bool procreationOnly, SpawnAbility ability)
        {
            if (Plugin.Configs.TamedSummonsUseSeparateLimit.Value == false)
            {
                return SpawnSystem.GetNrOfInstances(prefab, center, maxRange, eventCreaturesOnly, procreationOnly);
            }

            var owner = Traverse.Create(ability).Field("m_owner").GetValue<Character>();
            if (!owner || owner is Player || !prefab || !prefab.GetComponent<BaseAI>() || !prefab.GetComponent<Character>())
            {
                return SpawnSystem.GetNrOfInstances(prefab, center, maxRange, eventCreaturesOnly, procreationOnly);
            }

            bool tamed = owner.IsTamed();
            string prefabName = prefab.name + "(Clone)";
            int count = 0;

            foreach (var ai in BaseAI.BaseAIInstances)
            {
                if (!ai || ai.gameObject.name != prefabName)
                    continue;

                if (maxRange > 0f && Vector3.Distance(center, ai.transform.position) > maxRange)
                    continue;

                if (eventCreaturesOnly && ai is MonsterAI monsterAI && !monsterAI.IsEventCreature())
                    continue;

                if (procreationOnly)
                {
                    var procreation = ai.GetComponent<Procreation>();
                    if (procreation && !procreation.ReadyForProcreation())
                        continue;
                }

                var character = ai.GetComponent<Character>();
                if (character && character.IsTamed() == tamed)
                    count++;
            }

            return count;
        }

    }
}
