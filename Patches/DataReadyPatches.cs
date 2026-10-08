using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace OfTamingAndBreeding.Patches
{
    [HarmonyPatch]
    internal partial class DataReadyPatches : Core.PatchGroup<DataReadyPatches>
    {
        internal static new void Install() => Core.PatchGroup<DataReadyPatches>.Install();
        internal static new void Uninstall() => Core.PatchGroup<DataReadyPatches>.Uninstall();






        [HarmonyPatch(typeof(BaseAI), "Follow")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> BaseAI_Follow_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var callback = AccessTools.Method(typeof(DataReadyPatches), nameof(GetFollowDistance));

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_R4 && codes[i].operand is float value && value == 3f)
                {
                    codes[i] = new CodeInstruction(OpCodes.Ldarg_0);
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, callback));
                    break;
                }
            }

            // todo: check if const has been found

            return codes;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float GetFollowDistance(BaseAI baseAI)
        {
            // todo: add follow distance yaml option
            return 5f;
        }





    }
}
