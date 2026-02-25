using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OfTamingAndBreeding.Helpers
{
    internal static class EffectsHelper
    {





        public static EffectList.EffectData CreateEffectData(GameObject prefab)
            => prefab ? new EffectList.EffectData
            {
                m_prefab = prefab,
                m_attach = false,
                m_childTransform = "",
                m_enabled = true,
                m_follow = false,
                m_inheritParentRotation = false,
                m_inheritParentScale = false,
                m_multiplyParentVisualScale = false,
                m_randomRotation = false,
                m_scale = false,
                m_variant = -1,
            } : null;

        public static EffectList.EffectData GetEffect(string prefabName)
            => CreateEffectData(PrefabManager.Instance.GetPrefab(prefabName));

        public static EffectList.EffectData[] CreateEffectList(string[] prefabNames)
            => prefabNames.Where((n) => n != null).Select(GetEffect).ToArray();

        public static EffectList.EffectData[] CreateEffectList(GameObject[] prefabNames)
            => prefabNames.Where((n) => n != null).Select(CreateEffectData).ToArray();

        public static GameObject GetVisualOnlyEffect(string prefabName, string cloneName)
        {
            var clone = PrefabManager.Instance.GetPrefab(cloneName);
            if (clone)
            {
                return clone;
            }
            clone = PrefabManager.Instance.CreateClonedPrefab(cloneName, prefabName);
            foreach (var zsfx in clone.GetComponentsInChildren<ZSFX>(true)) UnityEngine.Object.Destroy(zsfx);
            foreach (var a in clone.GetComponentsInChildren<AudioSource>(true)) UnityEngine.Object.Destroy(a);
            return clone;
        }

        /*

        // todo: remove me

-- Boar Tameable m_petEffect
fx_boar_pet

-- Boar Tameable m_sootheEffect
vfx_creature_soothed

-- Boar Tameable m_tamedEffect
fx_creature_tamed

-- Boar Procreation m_loveEffects
sfx_boar_love
vfx_boar_love

-- Boar Procreation m_birthEffects
sfx_boar_birth
vfx_boar_birth

-- Wolf Tameable m_petEffect
fx_wolf_pet

-- Wolf Tameable m_sootheEffect
vfx_creature_soothed

-- Wolf Tameable m_tamedEffect
fx_creature_tamed

-- Wolf Procreation m_loveEffects
sfx_wolf_love
vfx_boar_love

-- Wolf Procreation m_birthEffects
sfx_wolf_birth
vfx_boar_birth

-- Lox Tameable m_petEffect
fx_lox_pet

-- Lox Tameable m_sootheEffect
vfx_lox_soothed

-- Lox Tameable m_tamedEffect
fx_lox_tamed

-- Lox Procreation m_loveEffects
sfx_lox_breath
vfx_lox_love

-- Lox Procreation m_birthEffects
sfx_lox_shout
vfx_boar_birth

-- Hen Tameable m_petEffect
fx_chicken_pet

-- Hen Tameable m_sootheEffect
vfx_creature_soothed

-- Hen Tameable m_tamedEffect
fx_creature_tamed

-- Hen Procreation m_loveEffects
fx_hen_love

-- Hen Procreation m_birthEffects
fx_chicken_lay_egg

-- Asksvin Tameable m_petEffect
fx_asksvin_pet

-- Asksvin Tameable m_sootheEffect
fx_asksvin_soothed

-- Asksvin Tameable m_tamedEffect
fx_asksvin_tamed

-- Asksvin Procreation m_loveEffects
sfx_asksvin_idle
fx_asksvin_tamed

-- Asksvin Procreation m_birthEffects
sfx_asksvin_idle
vfx_boar_birth


        */

        internal static EffectList FindEffectList<T>(string prefabName, string fieldName) where T : UnityEngine.Object
        {
            var prefab = ZNetScene.instance.GetPrefab(prefabName);
            if (!prefab)
            {
                Plugin.LogWarning($"GetEffectData(): Unknown prefab '{prefabName}'");
                return null;
            }
            T procreation = prefab.GetComponent<T>();
            if (!procreation)
            {
                Plugin.LogWarning($"GetEffectData(): No Component '{nameof(T)}' for prefab '{prefabName}'");
                return null;
            }
            var fieldInfo = typeof(T).GetField(fieldName);
            if (fieldInfo == null)
            {
                Plugin.LogWarning($"GetEffectData(): No Component Field '{nameof(T)}.{fieldName}' for prefab '{prefabName}'");
                return null;
            }
            return (EffectList)fieldInfo.GetValue(procreation);
        }

        internal static GameObject FindEffectPrefab<T>(string prefabName, string fieldName, int index) where T : UnityEngine.Object
        {
            var effList = FindEffectList<T>(prefabName, fieldName);
            if (effList == null || index >= effList.m_effectPrefabs.Length)
            {
                return null;
            }
            return effList.m_effectPrefabs[index].m_prefab;
        }

    }
}
