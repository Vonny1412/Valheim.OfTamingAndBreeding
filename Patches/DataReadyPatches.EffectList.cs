using HarmonyLib;
using OfTamingAndBreeding.Components;
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

        [HarmonyPatch(typeof(EffectList), "Create")]
        [HarmonyPrefix]
        private static void EffectList_Create_Prefix(Transform baseParent, ref float scale)
        {
            if (baseParent && baseParent.TryGetComponent<OTAB_ScaledCreature>(out var scaled))
            {
                scale = scaled.m_customEffectScale;
            }
        }

    }
}
