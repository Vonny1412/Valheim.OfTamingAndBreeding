using HarmonyLib;
using OfTamingAndBreeding.Components.Traits;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {

        [HarmonyPatch(typeof(Procreation), "IsDue")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        private static void Procreation_IsDue_Prefix(Procreation __instance)
        {
            var trait = ProcreationTrait.GetUnsafe(__instance.gameObject);
            trait.SetRealPregnancyDuration(__instance.m_pregnancyDuration);
        }

        [HarmonyPatch(typeof(Procreation), "Procreate")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        private static bool Procreation_Procreate_Prefix(Procreation __instance, bool __runOriginal)
        {
            if (!__runOriginal)
            {
                return false;
            }

            var trait = ProcreationTrait.GetUnsafe(__instance.gameObject);
            trait.SetRealPregnancyChance(__instance.m_pregnancyChance);
            trait.OnProcreate();

            return false;
        }


        /*

        public bool ReadyForProcreation()
        {
            if (m_tameable.IsTamed() && !IsPregnant())
            {
                return !m_tameable.IsHungry();
            }

            return false;
        }

        */

    }
}
