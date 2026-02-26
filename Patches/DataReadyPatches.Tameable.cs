using HarmonyLib;
using OfTamingAndBreeding.Components;
using OfTamingAndBreeding.Components.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {

        [HarmonyPatch(typeof(Tameable), "Awake")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void Tameable_Awake_Postfix(Tameable __instance)
        {
            __instance.Awake_PatchPostfix();
        }

        [HarmonyPatch(typeof(Tameable), "DecreaseRemainingTime")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        private static bool Tameable_DecreaseRemainingTime_Prefix(Tameable __instance, ref float time)
        {
            if (__instance.TryGetComponent<OTAB_Creature>(out var creature))
            {
                if (creature.m_tamingDisabled == true)
                {
                    return false;
                }
            }

            time *= __instance.GetRemainingTimeDecreaseFactor();
            return true;
        }

        [HarmonyPatch(typeof(Tameable), "IsHungry")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        private static bool Tameable_IsHungry_Prefix(Tameable __instance)
        {
            //__instance.UpdateFedDuration();
            // i dont remember why i was updating the fed duration at this point
            // maybe i dont need to?
            return true;
        }

        [HarmonyPatch(typeof(Tameable), "OnConsumedItem")]
        [HarmonyPrefix]
        private static bool Tameable_OnConsumedItem_Prefix(Tameable __instance, ItemDrop item)
        {
            return __instance.OnConsumedItem_PrefixPatch(item);
        }

        [HarmonyPatch(typeof(Tameable), "OnConsumedItem")]
        [HarmonyFinalizer]
        private static void Tameable_OnConsumedItem_Finalizer(Exception __exception)
        {
            TameableExtensions.ConsumeContext.Clear();
        }

        [HarmonyPatch(typeof(Tameable), "RPC_Command")]
        [HarmonyPrefix]
        private static bool Tameable_RPC_Command_Prefix(Tameable __instance, long sender, ZDOID characterID, bool message)
        {
            return __instance.RPC_Command_PatchPrefix(sender, characterID, message);
        }

        [HarmonyPatch(typeof(Tameable), "Tame")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        private static bool Tameable_Tame_Prefix(Tameable __instance)
        {
            //var prefabName = Utils.GetPrefabName(__instance.gameObject.name);
            //if (Runtime.Tameable.GetTamingDisabled(prefabName))
            //{
            //return false;
            //}
            return __instance.Tame_PatchPrefix();
        }

        [HarmonyPatch(typeof(Tameable), "Tame")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void Tameable_Tame_Postfix(Tameable __instance)
        {
            __instance.Tame_PatchPostfix();
        }

        [HarmonyPatch(typeof(Tameable), "TamingUpdate")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        private static bool Tameable_TamingUpdate_Prefix(Tameable __instance)
        {
            if (__instance.TryGetComponent<OTAB_Creature>(out var creature))
            {
                if (creature.m_tamingDisabled == true)
                {
                    return false;
                }
            }

            return __instance.TamingUpdate_PatchPrefix();
        }

    }
}
