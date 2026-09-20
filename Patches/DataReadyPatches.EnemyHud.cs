using HarmonyLib;
using OfTamingAndBreeding.Components.Extensions;
using OfTamingAndBreeding.Components.Traits;
using OfTamingAndBreeding.Utilities;
using System.Collections;
using UnityEngine;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {

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
