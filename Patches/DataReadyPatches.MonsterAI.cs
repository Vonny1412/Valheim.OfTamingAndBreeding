using HarmonyLib;
using OfTamingAndBreeding.Components.Extensions;
using OfTamingAndBreeding.Components.Traits;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {
        [HarmonyPatch(typeof(MonsterAI), "FindClosestConsumableItem")]
        [HarmonyPrefix]
        private static bool MonsterAI_FindClosestConsumableItem_Prefix(MonsterAI __instance, bool __runOriginal, ref ItemDrop __result)
        {
            if (!__runOriginal)
            {
                return false;
            }

            // note: for animals the find-consume-item logic is handled here:
            // BaseAI_IdleMovement_Prefix -> if (trait.IdleMovement(dt))
            // -> BaseAITrait.IdleMovement() -> if (m_animalAITrait && m_animalAITrait.IdleMovement(dt))
            // -> AnimalAITrait.IdleMovement() -> if (UpdateConsumeItem(dt)) return true;

            if (BaseAITrait.TryGet(__instance.gameObject, out var trait))
            {
                __result = trait.FindClosestConsumableItem(__instance.m_consumeSearchRange, __instance.m_consumeItems);
                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(MonsterAI), "UpdateConsumeItem")]
        [HarmonyPrefix]
        private static bool MonsterAI_UpdateConsumeItem_Prefix(MonsterAI __instance, float dt, ref bool __result)
        {
            if (!BaseAITrait.TryGet(__instance.gameObject, out var trait))
            {
                return true;
            }
            if (trait.UpdateContainerConsumeItem(dt))
            {
                __result = true;
                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(MonsterAI), "UpdateConsumeItem")]
        [HarmonyPostfix]
        private static void MonsterAI_UpdateConsumeItem_Postfix(MonsterAI __instance, float dt, ref bool __result)
        {
            if (__result || (bool)__instance.GetConsumeTarget() || __instance.IsAlerted() || __instance.GetTargetCreature() || __instance.GetStaticTarget())
            {
                return;
            }
            if (!BaseAITrait.TryGet(__instance.gameObject, out var trait))
            {
                return;
            }
            if (trait.TryFindContainerConsumeTarget(dt))
            {
                __result = true;
            }
        }





        [HarmonyPatch(typeof(MonsterAI), nameof(MonsterAI.UpdateAI))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> MonsterAI_UpdateAI_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codes = new List<CodeInstruction>(instructions);

            var targetField = AccessTools.Field( typeof(MonsterAI), "m_fleeIfHurtWhenTargetCantBeReached");
            var consumeMethod = AccessTools.Method(typeof(MonsterAI), "UpdateConsumeItem", new[] { typeof(Humanoid), typeof(float) });
            var helperMethod = AccessTools.Method(typeof(DataReadyPatches), nameof(MonsterAI_UpdateVeryHungry));

            if (targetField == null || consumeMethod == null || helperMethod == null)
            {
                Plugin.LogWarning("Transpiler failed: MonsterAI.UpdateAI - member not found");
                return codes;
            }

            // Find the Humanoid local used by the vanilla UpdateConsumeItem call.
            CodeInstruction humanoidLoad = null;

            for (int j = 2; j < codes.Count; j++)
            {
                if ((codes[j].opcode == OpCodes.Call || codes[j].opcode == OpCodes.Callvirt) && Equals(codes[j].operand, consumeMethod))
                {
                    var candidate = codes[j - 2];

                    if (candidate.opcode != OpCodes.Ldloc_0 &&
                        candidate.opcode != OpCodes.Ldloc_1 &&
                        candidate.opcode != OpCodes.Ldloc_2 &&
                        candidate.opcode != OpCodes.Ldloc_3 &&
                        candidate.opcode != OpCodes.Ldloc &&
                        candidate.opcode != OpCodes.Ldloc_S)
                    {
                        Plugin.LogWarning("Transpiler failed: MonsterAI.UpdateAI - unexpected Humanoid load");
                        return codes;
                    }

                    humanoidLoad = candidate.Clone();
                    humanoidLoad.labels.Clear();
                    humanoidLoad.blocks.Clear();
                    break;
                }
            }

            if (humanoidLoad == null)
            {
                Plugin.LogWarning("Transpiler failed: MonsterAI.UpdateAI - Humanoid local not found");
                return codes;
            }

            // Find the insertion point before the flee-if-hurt block.
            for (int i = 1; i < codes.Count; i++)
            {
                if (!codes[i].LoadsField(targetField))
                    continue;

                int insertIndex = i - 1;
                Label continueLabel = generator.DefineLabel();

                var injected = new List<CodeInstruction>
                {
                    new(OpCodes.Ldarg_0),
                    humanoidLoad,
                    new(OpCodes.Ldarg_1),
                    new(OpCodes.Call, helperMethod),
                    new(OpCodes.Brfalse, continueLabel),
                    new(OpCodes.Ldc_I4_1),
                    new(OpCodes.Ret)
                };

                // Preserve branches targeting the original instruction.
                injected[0].labels.AddRange(codes[insertIndex].labels);
                codes[insertIndex].labels.Clear();

                // Continue with the original vanilla instructions.
                codes[insertIndex].labels.Add(continueLabel);

                codes.InsertRange(insertIndex, injected);
                return codes;
            }

            Plugin.LogWarning("Transpiler failed: MonsterAI.UpdateAI - insertion point not found");
            return codes;
        }

        private static bool MonsterAI_UpdateVeryHungry(MonsterAI monsterAI, Humanoid humanoid, float dt)
        {
            if (!monsterAI.GetTargetCreature())
            {
                return false;
            }

            if (!TameableTrait.TryGet(monsterAI.gameObject, out var trait) || !trait.IsVeryHungry())
            {
                return false;
            }

            return monsterAI.UpdateConsumeItem(humanoid, dt);
        }





    }
}
