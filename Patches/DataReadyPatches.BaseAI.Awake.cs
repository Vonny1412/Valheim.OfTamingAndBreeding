using HarmonyLib;
using OfTamingAndBreeding.Components.Extensions;
using System;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {

        [HarmonyPatch(typeof(BaseAI), "Awake")]
        [HarmonyPostfix]
        private static void BaseAI_Awake_Postfix(BaseAI __instance)
        {
            // todo: add config for this or an ingame debug command
            /*
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
            */
        }

    }
}
