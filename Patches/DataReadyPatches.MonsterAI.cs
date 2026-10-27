using HarmonyLib;
using OfTamingAndBreeding.Components.Extensions;
using OfTamingAndBreeding.Components.Traits;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {

        [HarmonyPatch(typeof(MonsterAI), "FindClosestConsumableItem")]
        [HarmonyPrefix]
        private static bool MonsterAI_FindClosestConsumableItem_Prefix(MonsterAI __instance, bool __runOriginal, Character ___m_character, ref ItemDrop __result)
        {
            if (!__runOriginal)
            {
                return false;
            }

            if (Plugin.Configs.WakeUpForFood.Value && !___m_character.CanMove())
            {
                __result = null;
                return false;
            }

            if (BaseAITrait.TryGet(__instance.gameObject, out var trait))
            {
                __result = trait.FindClosestConsumableItem(__instance.m_consumeSearchRange, __instance.m_consumeItems);
                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(MonsterAI), "UpdateConsumeItem")]
        [HarmonyPostfix]
        private static void MonsterAI_UpdateConsumeItem_Postfix(MonsterAI __instance, float dt, Character ___m_character, ItemDrop ___m_consumeTarget, ref bool __result)
        {
            if (__result || (bool)___m_consumeTarget || __instance.IsAlerted() || __instance.GetTargetCreature() || __instance.GetStaticTarget())
            {
                return;
            }
            if (BaseAITrait.TryGet(__instance.gameObject, out var trait) && trait.UpdateContainerConsumeItem(dt))
            {
                __result = true;
            }
        }

        //---------------------------------

        [HarmonyPatch(typeof(MonsterAI), nameof(MonsterAI.UpdateAI))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> MonsterAI_UpdateAI_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codes = new List<CodeInstruction>(instructions);

            PatchSleepingFood(codes);
            PatchVeryHungry(codes, generator);

            return codes;
        }

        //---------------------------------

        private static void PatchSleepingFood(List<CodeInstruction> codes)
        {
            var matcher = new CodeMatcher(codes);
            matcher.MatchForward(false, new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(MonsterAI), "UpdateSleep")));
            if (!matcher.IsValid)
            {
                throw new Exception("Could not find MonsterAI.UpdateSleep call.");
            }

            matcher.Advance(1).Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DataReadyPatches), nameof(UpdateSleepingFood)))
            );

            codes.Clear();
            codes.AddRange(matcher.InstructionEnumeration());
        }

        private static readonly AccessTools.FieldRef<MonsterAI, float> ConsumeSearchTimer = AccessTools.FieldRefAccess<MonsterAI, float>("m_consumeSearchTimer");
        private static readonly AccessTools.FieldRef<MonsterAI, Tameable> Tameable = AccessTools.FieldRefAccess<MonsterAI, Tameable>("m_tamable");

        private static void UpdateSleepingFood(MonsterAI instance, float dt)
        {
            if (!instance.IsSleeping() || !Plugin.Configs.WakeUpForFood.Value)
            {
                return;
            }

            if (instance.m_consumeItems == null || instance.m_consumeItems.Count == 0)
            {
                return;
            }

            ref float consumeSearchTimer = ref ConsumeSearchTimer(instance);
            consumeSearchTimer += dt;
            if (consumeSearchTimer > instance.m_consumeSearchInterval)
            {
                consumeSearchTimer = 0f;

                var tameable = Tameable(instance);
                if (tameable && !tameable.IsHungry())
                {
                    return;
                }

                if (!BaseAITrait.TryGet(instance.gameObject, out var trait))
                {
                    return;
                }

                var itemDrop = trait.FindClosestConsumableItem(instance.m_consumeSearchRange, instance.m_consumeItems);
                if (itemDrop)
                {
                    instance.SetConsumeTarget(itemDrop);
                    instance.WakeUp();
                }
            }
        }

        //---------------------------------

        private static void PatchVeryHungry(List<CodeInstruction> codes, ILGenerator generator)
        {
            var targetField = AccessTools.Field( typeof(MonsterAI), "m_fleeIfHurtWhenTargetCantBeReached");
            var consumeMethod = AccessTools.Method(typeof(MonsterAI), "UpdateConsumeItem", new[] { typeof(Humanoid), typeof(float) });
            var helperMethod = AccessTools.Method(typeof(DataReadyPatches), nameof(MonsterAI_UpdateVeryHungry));

            if (targetField == null || consumeMethod == null || helperMethod == null)
            {
                Plugin.LogWarning("Transpiler failed: MonsterAI.UpdateAI - member not found");
                return;
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
                        return;
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
                return;
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
                return;
            }

            Plugin.LogWarning("Transpiler failed: MonsterAI.UpdateAI - insertion point not found");
            return;
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

        //---------------------------------

    }
}
