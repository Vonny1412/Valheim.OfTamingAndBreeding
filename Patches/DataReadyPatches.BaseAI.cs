using HarmonyLib;
using OfTamingAndBreeding.Components.Traits;
using static UnityEngine.Networking.UnityWebRequest;

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

            if (BaseAITrait.TryGet(__instance.gameObject, out var trait))
            {
                if (trait.On_UpdateAI(dt))
                {
                    __result = false;
                }
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

            if (BaseAITrait.TryGet(__instance.gameObject, out var trait))
            {
                if (trait.On_IdleMovement(dt))
                {
                    return false; // block original
                }
            }

            return true;
        }

    }
}
