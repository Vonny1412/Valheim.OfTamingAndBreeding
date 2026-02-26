using HarmonyLib;
using OfTamingAndBreeding.Components.Extensions;
using OfTamingAndBreeding.ValheimAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {

        [HarmonyPatch(typeof(BaseAI), "Awake")]
        [HarmonyPostfix]
        private static void BaseAI_Awake_Postfix(BaseAI __instance)
        {
            var nview = __instance.GetZNetView();
            if (nview.IsOwner())
            {
                // todo: add config for this or an ingame debug command
                /*
                var tameable = __instance.GetComponent<Tameable>();
                if (tameable && tameable.m_commandable == false && __instance.GetPatrolPoint(out var point))
                {
                    __instance.ResetPatrolPoint();
                    Plugin.LogWarning($"ResetPatrolPoint: {__instance.name}");
                }
                */
            }
        }

        [HarmonyPatch(typeof(BaseAI), "IdleMovement")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        private static bool BaseAI_IdleMovement_Prefix(BaseAI __instance, float dt)
        {
            return __instance.IdleMovement_PatchPrefix(dt);
        }

        [HarmonyPatch(typeof(BaseAI), "IsEnemy", new[] { typeof(Character), typeof(Character) })]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        private static bool BaseAI_IsEnemy_Prefix(BaseAI __instance, Character a, Character b, ref bool __result)
        {
            return __instance.IsEnemy_PatchPrefix(a, b, ref __result);
        }

        [HarmonyPatch(typeof(BaseAI), "IsEnemy", new[] { typeof(Character), typeof(Character) })]
        [HarmonyFinalizer]
        private static void BaseAI_IsEnemy_Finalizer(Exception __exception)
        {
            BaseAIExtensions.IsEnemyContext.Cleanup();
        }

    }
}
