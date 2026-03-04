using HarmonyLib;
using OfTamingAndBreeding.Components.Traits;
using System;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {

        [HarmonyPatch(typeof(Tameable), "OnConsumedItem")]
        [HarmonyPrefix]
        private static bool Tameable_OnConsumedItem_Prefix(Tameable __instance, ItemDrop item)
        {
            var trait = __instance.GetComponent<TameableTrait>();
            if (trait.OnConsumedItem(item))
            {
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(Tameable), "OnConsumedItem")]
        [HarmonyFinalizer]
        private static void Tameable_OnConsumedItem_Finalizer(Exception __exception)
        {
            StaticContext.ItemConsumeContext.Clear();
        }

        [HarmonyPatch(typeof(Tameable), "TamingUpdate")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        private static bool Tameable_TamingUpdate_Prefix(Tameable __instance)
        {
            var trait = __instance.GetComponent<TameableTrait>();
            if (trait.OnTamingUpdate())
            {
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(Tameable), "DecreaseRemainingTime")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        private static bool Tameable_DecreaseRemainingTime_Prefix(Tameable __instance, ref float time)
        {
            var trait = __instance.GetComponent<TameableTrait>();
            if (trait.IsTamingDisabled())
            {
                return false;
            }
            time *= trait.GetRemainingTimeDecreaseFactor();
            return true;
        }

        [HarmonyPatch(typeof(Tameable), "Tame")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        private static bool Tameable_Tame_Prefix(Tameable __instance)
        {
            var trait = __instance.GetComponent<TameableTrait>();
            if (trait.OnTame())
            {
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(Tameable), "Tame")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void Tameable_Tame_Postfix(Tameable __instance)
        {
            var trait = __instance.GetComponent<TameableTrait>();
            trait.OnTamed();
        }

        [HarmonyPatch(typeof(Tameable), "RPC_Command")]
        [HarmonyPrefix]
        private static bool Tameable_RPC_Command_Prefix(Tameable __instance, long sender, ZDOID characterID, bool message)
        {
            var trait = __instance.GetComponent<TameableTrait>();
            if (trait.RPC_Command(sender, characterID, message))
            {
                return false;
            }
            return true;
        }






        // todo: continue cleanup






        /* // todo: replaced
        [HarmonyPatch(typeof(Tameable), "Awake")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void Tameable_Awake_Postfix(Tameable __instance)
        {
            __instance.Awake_PatchPostfix();
        }
        */

        [HarmonyPatch(typeof(Tameable), "IsHungry")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        private static bool Tameable_IsHungry_Prefix(Tameable __instance)
        {
            // todo: remove me?
            //__instance.UpdateFedDuration();
            // i dont remember why i was updating the fed duration at this point
            // maybe i dont need to?
            return true;
        }

    }
}
