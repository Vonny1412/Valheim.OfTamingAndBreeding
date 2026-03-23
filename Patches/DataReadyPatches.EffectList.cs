using HarmonyLib;
using OfTamingAndBreeding.Components;
using UnityEngine;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {

        [HarmonyPatch(typeof(EffectList), "Create")]
        [HarmonyPrefix]
        private static void EffectList_Create_Prefix(Transform baseParent, ref float scale)
        {
            //if (baseParent && baseParent.TryGetComponent<ScaledCreature>(out var scaled))
            if (baseParent && ScaledCreature.TryGet(baseParent.gameObject, out var scaled))
            {
                scale = scaled.m_effectScale;
            }
        }

    }
}
