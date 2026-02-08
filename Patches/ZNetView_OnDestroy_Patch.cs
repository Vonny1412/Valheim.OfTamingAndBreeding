using HarmonyLib;
using OfTamingAndBreeding.Internals.Lifecycle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Patches
{
    [HarmonyPatch(typeof(ZNetView), "OnDestroy")]
    static class ZNetView_OnDestroy_Patch
    {
        static void Prefix(ZNetView __instance)
        {
            if (!Internals.Lifecycle.CleanupMarks.IsMarked(__instance))
                return;

            Internals.Lifecycle.CleanupMarks.Unmark(__instance);

            var go = __instance.gameObject;
            if (!go) return;

            var animalAI = go.GetComponent<AnimalAI>();
            if (animalAI) Internals.AnimalAIAPI.Remove(animalAI);

            var character = go.GetComponent<Character>();
            if (character) Internals.CharacterAPI.Remove(character);

            var eggGrow = go.GetComponent<EggGrow>();
            if (eggGrow) Internals.EggGrowAPI.Remove(eggGrow);

            var growup = go.GetComponent<Growup>();
            if (growup) Internals.GrowupAPI.Remove(growup);

            var procreation = go.GetComponent<Procreation>();
            if (procreation) Internals.ProcreationAPI.Remove(procreation);

            var tameable = go.GetComponent<Tameable>();
            if (tameable) Internals.TameableAPI.Remove(tameable);

        }
    }
}
