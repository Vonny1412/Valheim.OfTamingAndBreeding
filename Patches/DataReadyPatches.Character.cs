using HarmonyLib;
using OfTamingAndBreeding.Components.Traits;
using System;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {

        [HarmonyPatch(typeof(Character), "RPC_SetTamed")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void Character_RPC_SetTamed_Postfix(Character __instance, bool tamed)
        {
            if (!tamed)
            {
                return;
            }
            //var trait = CharacterTrait.GetUnsafe(__instance.gameObject);
            if (CharacterTrait.TryGet(__instance.gameObject, out var trait))
            {
                trait.OnTamed();
            }
        }

    }
}
