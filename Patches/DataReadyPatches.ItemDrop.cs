using HarmonyLib;
using OfTamingAndBreeding.Components;
using OfTamingAndBreeding.Components.Extensions;
using OfTamingAndBreeding.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {

        [HarmonyPatch(typeof(ItemDrop), "DropItem")]
        [HarmonyPostfix]
        private static void ItemDrop_DropItem_Postfix(ItemDrop __instance, ItemDrop.ItemData item, int amount, Vector3 position, Quaternion rotation, ItemDrop __result)
        {
            // do not use __instance !!!
            // because __result is the item that has been dropped
            __result.HandleItemDropped();
        }

        [HarmonyPatch(typeof(ItemDrop), "RemoveOne")]
        [HarmonyPrefix]
        private static void ItemDrop_RemoveOne_Prefix(ItemDrop __instance)
        {
            // used for RequireFoodDroppedByPlayer-feature
            // because when a creature eats food with a stack size of 1 that item would be destroyed
            // thats why we need to patch this one to pass the flags to Tameable_OnConsumedItem_Patch
            __instance.RemoveOne_PatchPrefix();
            // do return nothing (always call original method)
        }

        [HarmonyPatch(typeof(ItemDrop), "SetQuality")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void ItemDrop_SetQuality_Postfix(ItemDrop __instance)
        {
            if (__instance.TryGetComponent<OTAB_ScaledEgg>(out var scaler))
            {
                // we need to multiply because localScale has already been set to variable scaling according to stuff like quality
                __instance.transform.localScale *= scaler.m_customScale;
            }
        }

    }
}
