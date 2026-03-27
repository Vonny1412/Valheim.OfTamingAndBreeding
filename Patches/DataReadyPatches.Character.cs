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
            // GetUnsafe() will fail for offsprings since the awake method has not been called yet
            //var trait = CharacterTrait.GetUnsafe(__instance.gameObject);
            if (CharacterTrait.TryGet(__instance.gameObject, out var trait))
            {
                // this will only be run for creatures that actually got tamed
                // not for offsprings that just hatched
                trait.RPC_SetTamed(tamed);
            }
        }
        
    }
}
