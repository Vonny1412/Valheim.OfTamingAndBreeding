using HarmonyLib;
using OfTamingAndBreeding.Components.Traits;
using UnityEngine;
using static Version;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {

        [HarmonyPatch(typeof(Tameable), "OnConsumedItem")]
        [HarmonyPrefix]
        private static bool Tameable_OnConsumedItem_Prefix(Tameable __instance, ItemDrop item)
        {
            if (TameableTrait.TryGet(__instance.gameObject, out var trait))
            {
                if (trait.On_ConsumedItem(item))
                {
                    return false;
                }
            }

            return true;
        }

        [HarmonyPatch(typeof(Tameable), "TamingUpdate")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        private static bool Tameable_TamingUpdate_Prefix(Tameable __instance, bool __runOriginal)
        {
            if (!__runOriginal)
            {
                return false;
            }

            if (TameableTrait.TryGet(__instance.gameObject, out var trait))
            {
                if (trait.On_TamingUpdate())
                {
                    return false;
                }
            }

            return true;
        }

        [HarmonyPatch(typeof(Tameable), "DecreaseRemainingTime")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        private static bool Tameable_DecreaseRemainingTime_Prefix(Tameable __instance, bool __runOriginal, ref float time)
        {
            if (!__runOriginal)
            {
                return false;
            }

            if (TameableTrait.TryGet(__instance.gameObject, out var trait))
            {
                if (trait.IsTamingDisabled())
                {
                    return false;
                }
                if (trait.CanBeTamed() == false)
                {
                    return false;
                }
                time *= trait.GetRemainingTimeDecreaseFactor();
            }

            return true;
        }

        [HarmonyPatch(typeof(Tameable), "Tame")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void Tameable_Tame_Postfix(Tameable __instance)
        {
            if (TameableTrait.TryGet(__instance.gameObject, out var trait))
            {
                trait.On_Tame();
            }
        }

        [HarmonyPatch(typeof(Tameable), "RPC_Command")]
        [HarmonyPrefix]
        private static bool Tameable_RPC_Command_Prefix(Tameable __instance, long sender, ZDOID characterID, bool message)
        {
            if (TameableTrait.TryGet(__instance.gameObject, out var trait))
            {
                if (trait.On_RPC_Command(sender, characterID, message))
                {
                    return false;
                }
            }

            return true;
        }

    }
}
