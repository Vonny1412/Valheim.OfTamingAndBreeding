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
            if (__state == null)
            {
                return;
            }

            object hudData = __state[c];
            Traverse hudTraverse = Traverse.Create(hudData);
            GameObject gui = hudTraverse.Field("m_gui").GetValue<GameObject>();
            RectTransform aware = hudTraverse.Field("m_aware").GetValue<RectTransform>();

            if (!aware || !gui)
            {
                return;
            }

            if (gui.transform.Find("Confined"))
            {
                return;
            }

            Sprite sprite = GetConfinedIconSprite("OfTamingAndBreeding.Resources.Icons.Confined.png", "OTAB_ConfinedIcon");
            if (!sprite)
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

            GameObject confined = UnityEngine.Object.Instantiate(aware.gameObject, awareParent);
            confined.name = "Confined";

            UnityEngine.UI.Image image = confined.GetComponent<UnityEngine.UI.Image>();
            image.sprite = sprite;
            image.color = new Color(1f, 0.4f, 0.05f, 1f);

            confined.SetActive(false);

            BaseAITrait baseAITrait = BaseAITrait.GetUnsafe(c.gameObject);
            baseAITrait.SetConfinedHud(confined, awareGate);
        }

        private static Sprite m_confinedIconSprite;

        private static Sprite GetConfinedIconSprite(string path, string name)
        {
            if (m_confinedIconSprite)
            {
                return m_confinedIconSprite;
            }

            m_confinedIconSprite = SpriteUtils.LoadSpriteFromResource(path, name);
            if (!m_confinedIconSprite)
            {
                Plugin.LogError($"Could not load embedded icon '{path}'");
            }

            return m_confinedIconSprite;
        }

    }
}
