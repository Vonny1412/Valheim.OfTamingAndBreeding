using HarmonyLib;
using OfTamingAndBreeding.Components.Traits;
using System;
using UnityEngine;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {

        [ThreadStatic]
        private static bool s_droppedByPlayer = false;

        [HarmonyPatch(typeof(Humanoid), "DropItem")]
        [HarmonyPrefix]
        private static void Humanoid_DropItem_Prefix(Humanoid __instance)
        {
            s_droppedByPlayer = __instance.IsPlayer();
        }

        [HarmonyPatch(typeof(Humanoid), "DropItem")]
        [HarmonyFinalizer]
        private static void Humanoid_DropItem_Finalizer()
        {
            s_droppedByPlayer = false;
        }

        [HarmonyPatch(typeof(ItemDrop), "DropItem")]
        [HarmonyPostfix]
        private static void ItemDrop_DropItem_Postfix(ItemDrop __instance, ItemDrop.ItemData item, int amount, Vector3 position, Quaternion rotation, ItemDrop __result)
        {
            // todo: are all these parameters neccessary?
            if (s_droppedByPlayer)
            {
                if (ItemDropTrait.TryGet(__result.gameObject, out var trait))
                {
                    trait.SetDroppedByPlayer(byPlayer: true);
                }
            }
        }









        [HarmonyPatch(typeof(ItemDrop), "RemoveOne")]
        [HarmonyPrefix]
        private static void ItemDrop_RemoveOne_Prefix(ItemDrop __instance)
        {
            // used for RequireFoodDroppedByPlayer-feature
            // because when a creature eats food with a stack size of 1 that item would be destroyed
            // thats why we need to patch this one to pass the flags to Tameable_OnConsumedItem_Patch
            Runtime.ItemConsumeContext.SetItem(__instance);
            // do return nothing (always call original method)
        }

        [HarmonyPatch(typeof(Tameable), "OnConsumedItem")]
        [HarmonyFinalizer]
        private static void Tameable_OnConsumedItem_Finalizer(Exception __exception)
        {
            Runtime.ItemConsumeContext.Clear();
        }

    }
}
