using HarmonyLib;
using OfTamingAndBreeding.Components;
using OfTamingAndBreeding.Components.Extensions;
using OfTamingAndBreeding.Components.Traits;
using OfTamingAndBreeding.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;


namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {

        // Character.GetTopPoint() does not account for transform scaling when
        // adding the collider height. Replace it only for EnemyHud positioning.
        [HarmonyPatch(typeof(EnemyHud), "UpdateHuds")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> EnemyHud_UpdateHuds_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var getTopPoint = AccessTools.Method(typeof(Character), nameof(Character.GetTopPoint));
            var getHudTopPoint = AccessTools.Method(typeof(DataReadyPatches), nameof(GetHudTopPoint));
            foreach (var instruction in instructions)
            {
                if (instruction.Calls(getTopPoint))
                {
                    yield return new CodeInstruction(OpCodes.Call, getHudTopPoint);
                }
                else
                {
                    yield return instruction;
                }
            }
        }
        private static Vector3 GetHudTopPoint(Character character)
        {
            if (ScaledCreature.TryGet(character.gameObject, out var scaledCreature))
            {
                return scaledCreature.GetTopPoint();
            }
            return character.GetTopPoint();
        }

        private static Sprite m_frustratedIconSprite;

        [HarmonyPatch(typeof(EnemyHud), "ShowHud")]
        [HarmonyPrefix]
        private static void EnemyHud_ShowHud_Prefix(EnemyHud __instance, Character c, ref IDictionary __state)
        {
            //if (c.IsBoss() || c.IsPlayer())
            if (c.IsPlayer())
            {
                return;
            }
            object huds = __instance.GetHuds();
            if (huds is IDictionary hudsDict && !hudsDict.Contains(c))
            {
                __state = hudsDict;
            }
        }

        [HarmonyPatch(typeof(EnemyHud), "ShowHud")]
        [HarmonyPostfix]
        private static void EnemyHud_ShowHud_Postfix(EnemyHud __instance, Character c, IDictionary __state)
        {
            if (__state == null || !BaseAITrait.TryGet(c.gameObject, out var trait))
            {
                return;
            }

            object hudData = __state[c];
            Traverse hudTraverse = Traverse.Create(hudData);
            GameObject gui = hudTraverse.Field("m_gui").GetValue<GameObject>();
            RectTransform aware = hudTraverse.Field("m_aware").GetValue<RectTransform>();

            if (!aware || !gui || gui.transform.Find("Frustrated"))
            {
                return;
            }

            if (!m_frustratedIconSprite)
            {
                m_frustratedIconSprite = SpriteUtils.LoadSpriteFromResource("OfTamingAndBreeding.Resources.Icons.Frustrated.png", "OTAB_FrustratedIcon");
            }
            if (!m_frustratedIconSprite)
            {
                return;
            }

            Transform awareParent = aware.parent;
            GameObject awareGate = new GameObject("OTAB_AwareGate", typeof(RectTransform));
            RectTransform awareGateTransform = awareGate.GetComponent<RectTransform>();
            awareGateTransform.SetParent(awareParent, false);
            awareGateTransform.anchorMin = Vector2.zero;
            awareGateTransform.anchorMax = Vector2.one;
            awareGateTransform.offsetMin = Vector2.zero;
            awareGateTransform.offsetMax = Vector2.zero;
            aware.SetParent(awareGateTransform, true);

            GameObject frustrated = UnityEngine.Object.Instantiate(aware.gameObject, awareParent);
            frustrated.name = "Frustrated";

            UnityEngine.UI.Image image = frustrated.GetComponent<UnityEngine.UI.Image>();
            image.sprite = m_frustratedIconSprite;
            image.color = new Color(1f, 0.4f, 0.05f, 1f);

            frustrated.SetActive(false);

            trait.SetFrustratedHud(frustrated, awareGate);
        }

    }
}
