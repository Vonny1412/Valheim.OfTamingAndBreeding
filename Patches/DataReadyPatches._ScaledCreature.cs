using HarmonyLib;
using OfTamingAndBreeding.Components;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace OfTamingAndBreeding.Patches
{
    [HarmonyPatch]
    internal partial class DataReadyPatches : Core.PatchGroup<DataReadyPatches>
    {

        //
        // Scaled Creatures
        //

        [HarmonyPatch(typeof(ZSyncAnimation), "SetFloat", new[] { typeof(int), typeof(float) })]
        [HarmonyPrefix]
        private static void ZSyncAnimation_SetFloat_Prefix(ZSyncAnimation __instance, int hash, ref float value)
        {
            if (__instance && ScaledCreature.TryGet(__instance.gameObject, out var scaled))
            {
                value *= scaled.m_animationScale;
            }
        }

        [HarmonyPatch(typeof(Attack), "ModifyDamage")]
        private static class Attack_ModifyDamage_Patch
        {
            private static void Postfix(HitData hitData, Character ___m_character)
            {
                if (___m_character && ScaledCreature.TryGet(___m_character.gameObject, out var scaledCreature))
                {
                    hitData.m_damage.Modify(scaledCreature.m_attackScale);
                }
            }
        }

        /*
         * Scales the physical dimensions of melee and area attacks for scaled creatures.
         *
         * Attack stores its ranges, offsets and hitbox sizes as world-space float values.
         * Scaling the creature's transform therefore does not automatically scale these
         * values, causing a visually smaller creature to retain the original attack reach.
         *
         * The transpiler intercepts reads of the relevant Attack fields and passes the
         * loaded value together with Attack.m_character to ScaleAttackDimension().
         *
         * Effectively, a field read such as:
         *
         *     m_attackRayWidth
         *
         * becomes:
         *
         *     ScaleAttackDimension(m_attackRayWidth, m_character)
         *
         * ScaleAttackDimension() applies ScaledCreature.m_scale when the attacking
         * character has a ScaledCreature component. Otherwise the original value is
         * returned unchanged.
         *
         * The original Attack fields are never modified. This keeps the patch stateless,
         * requires no restoration, and preserves the original vanilla attack logic.
         *
         * The same transpiler is applied to DoMeleeAttack() and DoAreaAttack() so all
         * relevant attack dimensions remain proportional to the creature's visual scale.
         */

        private static readonly HashSet<FieldInfo> s_scaledDimensionFields = new HashSet<FieldInfo>
        {
            AccessTools.Field(typeof(Attack), "m_attackRange"),
            AccessTools.Field(typeof(Attack), "m_attackHeight"),
            AccessTools.Field(typeof(Attack), "m_attackOffset"),
            AccessTools.Field(typeof(Attack), "m_attackRayWidth"),
            AccessTools.Field(typeof(Attack), "m_attackRayWidthCharExtra"),
            AccessTools.Field(typeof(Attack), "m_attackHeightChar1"),
            AccessTools.Field(typeof(Attack), "m_attackHeightChar2"),
        };

        private static readonly FieldInfo s_characterField = AccessTools.Field(typeof(Attack), "m_character");

        private static readonly MethodInfo s_scaleAttackDimension = AccessTools.Method(typeof(DataReadyPatches), nameof(ScaleAttackDimension));

        private static float ScaleAttackDimension(float value, Character character)
        {
            if (character && ScaledCreature.TryGet(character.gameObject, out var scaledCreature))
            {
                return value * scaledCreature.m_scale;
            }

            return value;
        }

        private static IEnumerable<CodeInstruction> ScaleAttackDimensions(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                yield return instruction;
                if (instruction.opcode == OpCodes.Ldfld && instruction.operand is FieldInfo field && s_scaledDimensionFields.Contains(field))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, s_characterField);
                    yield return new CodeInstruction(OpCodes.Call, s_scaleAttackDimension);
                }
            }
        }

        [HarmonyPatch(typeof(Attack), "DoAreaAttack")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> DoAreaAttack_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return ScaleAttackDimensions(instructions);
        }

        [HarmonyPatch(typeof(Attack), "DoMeleeAttack")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> DoMeleeAttack_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return ScaleAttackDimensions(instructions);
        }

        // projectiles part

        private static readonly FieldInfo s_projectileAoeField = AccessTools.Field(typeof(Projectile), "m_aoe");

        private static readonly FieldInfo s_projectileOwnerField = AccessTools.Field(typeof(Projectile), "m_owner");

        private static readonly MethodInfo s_scaleProjectileAoe = AccessTools.Method(typeof(DataReadyPatches), nameof(ScaleProjectileAoe));

        private static float ScaleProjectileAoe(float value, Character owner)
        {
            if (owner && ScaledCreature.TryGet(owner.gameObject, out var scaledCreature))
            {
                return value * scaledCreature.m_scale;
            }
            return value;
        }

        [HarmonyPatch(typeof(Projectile), "DoAOE")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Projectile_DoAOE_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                yield return instruction;
                if (instruction.opcode == OpCodes.Ldfld && Equals(instruction.operand, s_projectileAoeField))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, s_projectileOwnerField);
                    yield return new CodeInstruction(OpCodes.Call, s_scaleProjectileAoe);
                }
            }
        }








    }
}
