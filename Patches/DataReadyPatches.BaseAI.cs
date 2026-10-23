using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using OfTamingAndBreeding.Components.Traits;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {
        /*
        [HarmonyPatch(typeof(BaseAI), "Awake")]
        [HarmonyPostfix]
        private static void BaseAI_Awake_Postfix(BaseAI __instance)
        {

            // todo: add config for this or an ingame debug command

            var nview = __instance.GetZNetView();
            if (nview.IsOwner())
            {
                var tameable = __instance.GetComponent<Tameable>();
                if (tameable && tameable.m_commandable == false && __instance.GetPatrolPoint(out var point))
                {
                    __instance.ResetPatrolPoint();
                    Plugin.LogWarning($"ResetPatrolPoint: {__instance.name}");
                }
            }
        }
        */

        [HarmonyPatch(typeof(BaseAI), "UpdateAI")]
        [HarmonyPostfix] // this makes sure the rest of baseai is getting run
        private static void BaseAI_UpdateAI_Postfix(BaseAI __instance, float dt, ref bool __result)
        {
            if (__result == false)
            {
                return; // invalid afterall or blocked by other mod
            }

            if (BaseAITrait.TryGet(__instance.gameObject, out var trait) && trait.On_UpdateAI(dt))
            {
                __result = false;
            }
        }

        [HarmonyPatch(typeof(BaseAI), "IdleMovement")]
        [HarmonyPrefix]
        private static bool BaseAI_IdleMovement_Prefix(BaseAI __instance, bool __runOriginal, float dt)
        {
            if (!__runOriginal)
            {
                return false;
            }

            if (BaseAITrait.TryGet(__instance.gameObject, out var trait) && trait.On_IdleMovement(dt))
            {
                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(BaseAI), "IdleMovement")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> BaseAI_IdleMovement_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var original = AccessTools.Method(typeof(Character), nameof(Character.IsTamed));
            var replacement = AccessTools.Method(typeof(DataReadyPatches), nameof(IsTamedForIdleMovement));
            bool patched = false;

            foreach (var instruction in instructions)
            {
                if ((instruction.opcode == OpCodes.Callvirt || instruction.opcode == OpCodes.Call) && instruction.operand is System.Reflection.MethodInfo method && method == original)
                {
                    instruction.opcode = OpCodes.Call;
                    instruction.operand = replacement;
                    patched = true;
                }

                yield return instruction;
            }

            if (!patched)
            {
                Plugin.LogWarning("Transpiler failed: BaseAI.IdleMovement - Character.IsTamed() not found");
            }
        }

        private static bool IsTamedForIdleMovement(Character character)
        {
            return character.IsTamed() && (!BaseAITrait.TryGet(character.gameObject, out var trait) || !trait.m_tamedIdleNearSpawn);
        }








    }
}
