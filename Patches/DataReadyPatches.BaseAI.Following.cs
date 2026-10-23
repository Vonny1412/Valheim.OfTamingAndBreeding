using HarmonyLib;
using OfTamingAndBreeding.Network;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace OfTamingAndBreeding.Patches
{
    [HarmonyPatch]
    internal partial class DataReadyPatches : Core.PatchGroup<DataReadyPatches>
    {

        [HarmonyPatch(typeof(BaseAI), "Follow")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> BaseAI_Follow_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codes = new List<CodeInstruction>(instructions);
            var getMinDistance = AccessTools.Method(typeof(DataReadyPatches), nameof(GetFollowMinDistance));
            var getRunFactor = AccessTools.Method(typeof(DataReadyPatches), nameof(GetFollowRunDistanceFactor));
            var minDistance = generator.DeclareLocal(typeof(float));
            int minCount = 0;
            int runCount = 0;

            foreach (var code in codes)
            {
                if (code.opcode == OpCodes.Ldc_R4 && code.operand is float value)
                {
                    if (value == 3f) minCount++;
                    if (value == 10f) runCount++;
                }
            }

            if (minCount != 1 || runCount != 1)
            {
                Plugin.LogWarning("Transpiler failed: BaseAI.Follow - distance constants not found or ambiguous");
                return codes;
            }

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode != OpCodes.Ldc_R4 || !(codes[i].operand is float value))
                    continue;

                if (value == 3f)
                {
                    codes[i].opcode = OpCodes.Ldloc;
                    codes[i].operand = minDistance;
                }
                else if (value == 10f)
                {
                    codes[i].opcode = OpCodes.Ldloc;
                    codes[i].operand = minDistance;

                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, getRunFactor));
                    codes.Insert(i + 2, new CodeInstruction(OpCodes.Mul));

                    i += 2;
                }
            }

            codes.InsertRange(0, new[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, getMinDistance),
                new CodeInstruction(OpCodes.Stloc, minDistance)
            });

            return codes;
        }

        private static readonly Dictionary<string, float> s_followColliderRadii = new Dictionary<string, float>();

        private static float GetFollowMinDistance(BaseAI baseAI)
        {
            string prefabName = Utils.GetPrefabName(baseAI.gameObject);

            if (!s_followColliderRadii.TryGetValue(prefabName, out float radius))
            {
                radius = 0f;
                if (baseAI.TryGetComponent<CapsuleCollider>(out var collider))
                {
                    radius = GetColliderRadius(collider);
                }
                s_followColliderRadii[prefabName] = radius;
            }

            float min = Plugin.Configs.FollowMinDistanceMin.Value;
            float max = Plugin.Configs.FollowMinDistanceMax.Value;
            return Mathf.Clamp(radius * Plugin.Configs.FollowColliderRadiusFactor.Value, Mathf.Min(min, max), Mathf.Max(min, max));
        }

        private static float GetColliderRadius(CapsuleCollider collider)
        {
            Vector3 scale = collider.transform.lossyScale;

            float sx = Mathf.Abs(scale.x);
            float sy = Mathf.Abs(scale.y);
            float sz = Mathf.Abs(scale.z);
            var radialScale = collider.direction switch
            {
                0 => Mathf.Max(sy, sz),
                1 => Mathf.Max(sx, sz),
                _ => Mathf.Max(sx, sy),
            };
            return collider.radius * radialScale;
        }

        private static float GetFollowRunDistanceFactor()
        {
            return Plugin.Configs.FollowRunDistanceFactor.Value;
        }

        static DataReadyPatches()
        {
            NetworkSessionManager.OnSessionClosed += () => s_followColliderRadii.Clear();
        }
    }
}
