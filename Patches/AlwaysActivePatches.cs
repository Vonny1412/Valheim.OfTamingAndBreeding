using HarmonyLib;
using OfTamingAndBreeding.ValheimAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace OfTamingAndBreeding.Patches
{
    [HarmonyPatch]
    internal class AlwaysActivePatches : PatchGroup<AlwaysActivePatches>
    {
        public static new void Install() => PatchGroup<AlwaysActivePatches>.Install();
        public static new void Uninstall() => PatchGroup<AlwaysActivePatches>.Uninstall();


        //
        // Character
        //

        [HarmonyPatch(typeof(Character), "GetHoverName")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        static void Character_GetHoverName_Postfix(Character __instance, ref string __result)
        {
            __instance.GetHoverName_PatchPostfix(ref __result);
        }

        [HarmonyPatch(typeof(Character), "GetHoverText")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        static void Character_GetHoverText_Postfix(Character __instance, ref string __result)
        {
            __instance.GetHoverText_PatchPostfix(ref __result);
        }


        //
        // EggGrow
        //

        [HarmonyPatch(typeof(EggGrow), "GetHoverText")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        static bool EggGrow_GetHoverText_Prefix(EggGrow __instance, ref string __result)
        {
            var item = __instance.GetItemDrop();
            if (!item)
            {
                __result = "";
                return false;
            }
            // egg status is getting handled in ItemDrop_GetHoverText_Patch
            __result = item.GetHoverText();
            return false;
        }


        //
        // Hud
        //

        [HarmonyPatch(typeof(Hud), "UpdateCrosshair")]
        [HarmonyPostfix]
        static void Hud_UpdateCrosshair_Postfix(Hud __instance)
        {
            var hover = AccessTools.Field(typeof(Hud), "m_hoverName")?.GetValue(__instance) as TextMeshProUGUI;
            if (hover == null) return;

            float width = 500; // todo: maybe add config for this?
            var rt = hover.rectTransform;
            if (rt.rect.width < width)
            {
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
                hover.SetLayoutDirty();
                hover.SetVerticesDirty();
            }
        }


        //
        // ItemDrop
        //

        [HarmonyPatch(typeof(ItemDrop), "GetHoverText")]
        [HarmonyPostfix]
        static void ItemDrop_GetHoverText_Postfix(ItemDrop __instance, ref string __result)
        {
            __instance.GetHoverText_PatchPostfix(ref __result);
        }


        //
        // ZNet
        //

        [HarmonyPatch(typeof(ZNet), "OnDestroy")]
        [HarmonyPrefix]
        static void ZNet_OnDestroy_Prefix()
        {
            Plugin.CloseSession();
        }

        [HarmonyPatch(typeof(ZNet), "RPC_PeerInfo")]
        [HarmonyPostfix]
        static void ZNet_RPC_PeerInfo_Postfix()
        {
            if (ZNet.instance == null)
            {
                // wtf?
                Plugin.LogWarning("ZNet.instance is still null on ZNet.RPC_PeerInfo");
                return;
            }
            if (!ZNet.instance.IsServer())
            {
                Plugin.RequestHandshakeWithServer();
            }
        }

        [HarmonyPatch(typeof(ZNet), "Start")]
        [HarmonyPostfix]
        static void ZNet_Start_Postfix()
        {
            Plugin.InitSession();
        }


        //
        // ZNetScene
        //

        [HarmonyPatch(typeof(ZNetScene), "CreateObjects")]
        [HarmonyPrefix]
        static bool ZNetScene_CreateObjects_Prefix(ZNetScene __instance, List<ZDO> currentNearObjects, List<ZDO> currentDistantObjects)
        {
            // do not block even when otab is not ready
            // because we need to cache objects while client is waiting for otab data from server

            // IMPORTANT: We must not let ZNetScene instantiate network objects before OTAB server data is applied.
            // Otherwise components (Awake/Start) would run with wrong vanilla values.
            // We therefore defer CreateObjects until DataOrchestrator marks dataLoaded == true.
            return __instance.CreateObjects_PatchPrefix(currentNearObjects, currentDistantObjects);
        }


    }
}
