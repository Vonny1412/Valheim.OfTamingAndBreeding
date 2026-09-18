using HarmonyLib;
using OfTamingAndBreeding.Components;
using OfTamingAndBreeding.Components.Extensions;
using OfTamingAndBreeding.Components.Traits;
using System.Collections;
using System.Reflection;
using UnityEngine;

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
            //if (__instance && __instance.TryGetComponent<ScaledCreature>(out var scaled))
            if (__instance && ScaledCreature.TryGet(__instance.gameObject, out var scaled))
            {
                value *= scaled.m_animationScale;
            }
        }















        [HarmonyPatch(typeof(EnemyHud), "ShowHud")]
        [HarmonyPrefix]
        private static void EnemyHud_ShowHud_Prefix(EnemyHud __instance, Character c, ref IDictionary __state)
        {
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
            if (hudData == null)
            {
                return;
            }

            if (c.IsBoss() || c.IsPlayer())
            {
                return;
            }

            var hudTraverse = Traverse.Create(hudData);
            var gui = hudTraverse.Field("m_gui").GetValue<GameObject>();
            var aware = hudTraverse.Field("m_aware").GetValue<RectTransform>();
            if (!aware || !gui)
            {
                return;
            }

            // Already added to this HUD instance?
            if (gui.transform.Find("Jammed"))
            {
                return;
            }

            var sprite = GetJammedIconSprite();
            if (!sprite)
            {
                return;
            }

            var awareParent = aware.parent;

            var awareGate = new GameObject("OTAB_AwareGate", typeof(RectTransform));
            var awareGateTransform = awareGate.GetComponent<RectTransform>();

            awareGateTransform.SetParent(awareParent, false);
            awareGateTransform.anchorMin = Vector2.zero;
            awareGateTransform.anchorMax = Vector2.one;
            awareGateTransform.offsetMin = Vector2.zero;
            awareGateTransform.offsetMax = Vector2.zero;

            aware.SetParent(awareGateTransform, true);

            var jammed = UnityEngine.Object.Instantiate(aware.gameObject, awareParent);
            jammed.name = "Jammed";

            var image = jammed.GetComponent<UnityEngine.UI.Image>();
            image.sprite = sprite;
            image.color = new Color(1f, 0.4f, 0.05f, 1f);

            jammed.SetActive(false);

            var baseAITrait = BaseAITrait.GetUnsafe(c.gameObject);
            baseAITrait.SetJammedHud(jammed, awareGate);
        }

        private static Sprite m_jammedIconSprite;

        private static Sprite GetJammedIconSprite()
        {
            if (m_jammedIconSprite)
            {
                return m_jammedIconSprite;
            }

            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("OfTamingAndBreeding.Resources.Icons.Jammed.png");
            if (stream == null)
            {
                Plugin.LogError("Could not find embedded Jammed icon");
                return null;
            }

            var data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);

            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);

            if (!ImageConversion.LoadImage(texture, data))
            {
                UnityEngine.Object.Destroy(texture);
                Plugin.LogError("Could not load embedded Jammed icon");
                return null;
            }

            texture.name = "OTAB_JammedIcon";
            m_jammedIconSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
            m_jammedIconSprite.name = "OTAB_JammedIcon";



            return m_jammedIconSprite;
        }




    }
}
