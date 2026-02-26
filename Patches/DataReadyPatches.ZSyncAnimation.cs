using HarmonyLib;
using OfTamingAndBreeding.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {

        /*
        private static readonly Dictionary<int, string> animNames = new Dictionary<int, string>() {
            { ZSyncAnimation.GetHash("forward_speed"), "forward_speed" },
            { ZSyncAnimation.GetHash("sideway_speed"), "sideway_speed" },
            { ZSyncAnimation.GetHash("anim_speed"), "anim_speed" },
            { ZSyncAnimation.GetHash("turn_speed"), "turn_speed" },
            { ZSyncAnimation.GetHash("inWater"), "inWater" },
            { ZSyncAnimation.GetHash("onGround"), "onGround" },
            { ZSyncAnimation.GetHash("encumbered"), "encumbered" },
            { ZSyncAnimation.GetHash("flying"), "flying" },
            { ZSyncAnimation.GetHash("statef"), "statef" },
            { ZSyncAnimation.GetHash("statei"), "statei" },
            { ZSyncAnimation.GetHash("blocking"), "blocking" },
            { ZSyncAnimation.GetHash("attack"), "attack" },
            { ZSyncAnimation.GetHash("flapping"), "flapping" },
            { ZSyncAnimation.GetHash("idle"), "idle" },
        };
        */

        [HarmonyPatch(typeof(ZSyncAnimation), "SetFloat", new[] { typeof(int), typeof(float) })]
        [HarmonyPrefix]
        private static void ZSyncAnimation_SetFloat_Prefix(ZSyncAnimation __instance, int hash, ref float value)
        {
            if (__instance && __instance.TryGetComponent<OTAB_ScaledCreature>(out var scaled))
            {
                value *= scaled.m_customAnimationScale;
            }
        }

    }
}
