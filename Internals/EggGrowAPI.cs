using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using OfTamingAndBreeding.Data.Models;
using OfTamingAndBreeding.Helpers;

namespace OfTamingAndBreeding.Internals
{

    internal class EggGrowAPI : API.EggGrow
    {

        private static readonly ConditionalWeakTable<EggGrow, EggGrowAPI> instances
            = new ConditionalWeakTable<EggGrow, EggGrowAPI>();
        public static EggGrowAPI GetOrCreate(EggGrow __instance)
            => instances.GetValue(__instance, (EggGrow inst) => new EggGrowAPI(inst));
        public static bool TryGet(EggGrow __instance, out EggGrowAPI api)
            => instances.TryGetValue(__instance, out api);

        public EggGrowAPI(EggGrow __instance) : base(__instance)
        {
        }

        public bool GrowUpdate_Prefix(ZDO zdo)
        {
            var s_growStart = zdo.GetFloat(ZDOVars.s_growStart, 0f);
            if (m_item.m_itemData.m_stack > 1)
            {
                UpdateEffects(s_growStart);
                return false;
            }

            var z_EggBehavior = zdo.GetInt(Plugin.ZDOVars.z_EggBehavior, Plugin.ZDOVars.EggBehavior.Unknown);

            if (z_EggBehavior == Plugin.ZDOVars.EggBehavior.Unknown)
            {
                // determine behavior

                var data = Egg.Get(Utils.GetPrefabName(name));
                if (data == null)
                {
                    if (!m_grownPrefab)
                    {
                        return true;
                    }
                    z_EggBehavior = ZNetHelper.SetInt(zdo, Plugin.ZDOVars.z_EggBehavior, Plugin.ZDOVars.EggBehavior.Vanilla);
                }
                else
                {

                    var z_eggGrownPrefab = zdo.GetString(Plugin.ZDOVars.z_eggGrownPrefab, "");
                    var z_eggGrownTamed = zdo.GetInt(Plugin.ZDOVars.z_eggGrownTamed, 1);
                    var z_eggGrownTamedBool = z_eggGrownTamed == 1;
                    var z_eggShowHatchEffect = zdo.GetInt(Plugin.ZDOVars.z_eggShowHatchEffect, 1);
                    var z_eggShowHatchEffectBool = z_eggShowHatchEffect == 1;

                    var foundRandom = Data.Models.SubData.RandomData.FindRandom<Egg.EggGrowGrownData>(data.EggGrow.Grown, out Egg.EggGrowGrownData grownEntry);
                    if (!foundRandom) // should not happen but whatever
                    {
                        z_EggBehavior = ZNetHelper.SetInt(zdo, Plugin.ZDOVars.z_EggBehavior, Plugin.ZDOVars.EggBehavior.Vanilla, z_EggBehavior);
                        return false;
                    }

                    z_eggGrownPrefab = ZNetHelper.SetString(zdo, Plugin.ZDOVars.z_eggGrownPrefab, grownEntry.Prefab, z_eggGrownPrefab);
                    var grown = ZNetScene.instance.GetPrefab(z_eggGrownPrefab);

                    z_eggGrownTamed = ZNetHelper.SetInt(zdo, Plugin.ZDOVars.z_eggGrownTamed, grownEntry.Tamed ? 1 : 0, z_eggGrownTamed);
                    z_eggGrownTamedBool = z_eggGrownTamed == 1;

                    z_eggShowHatchEffect = ZNetHelper.SetInt(zdo, Plugin.ZDOVars.z_eggShowHatchEffect, grownEntry.ShowHatchEffect ? 1 : 0, z_eggShowHatchEffect);
                    z_eggShowHatchEffectBool = z_eggShowHatchEffect == 1;

                    var eggGrownEggData = Egg.Get(z_eggGrownPrefab);
                    if (eggGrownEggData == null)
                    {
                        // egg grown is not a registered egg
                        // use default egg grow up
                        z_EggBehavior = ZNetHelper.SetInt(zdo, Plugin.ZDOVars.z_EggBehavior, Plugin.ZDOVars.EggBehavior.Vanilla, z_EggBehavior);
                    }
                    else
                    {
                        // its an[other] egg!
                        z_EggBehavior = ZNetHelper.SetInt(zdo, Plugin.ZDOVars.z_EggBehavior, Plugin.ZDOVars.EggBehavior.Matrjoschka, z_EggBehavior);
                    }

                    // update values
                    m_grownPrefab = grown;
                    m_tamed = z_eggGrownTamedBool;
                    if (m_hatchEffect != null)
                    {
                        foreach (var eff in m_hatchEffect.m_effectPrefabs)
                        {
                            eff.m_enabled = z_eggShowHatchEffectBool;
                        }
                    }

                }

            }

            float num = s_growStart;
            if (CanGrow())
            {
                if (num == 0f)
                {
                    num = (float)ZNet.instance.GetTimeSeconds();
                }
            }
            else
            {
                num = 0f;
            }

            zdo.Set(ZDOVars.s_growStart, num);
            UpdateEffects(num);

            if (num > 0f && ZNet.instance.GetTimeSeconds() > (double)(num + m_growTime))
            {
                switch (z_EggBehavior)
                {

                    case Plugin.ZDOVars.EggBehavior.Vanilla:
                        HandleVanilla(zdo);
                        break;

                    case Plugin.ZDOVars.EggBehavior.Matrjoschka:
                        HandleMatrjoschka(zdo);
                        break;

                }
            }
            return false;
        }

        private void HandleVanilla(ZDO zdo)
        {
            var position = transform.position;
            var rotation = transform.rotation;

            GameObject spawned = UnityEngine.Object.Instantiate(m_grownPrefab, position, rotation);
            Character spawnedCharacter = spawned.GetComponent<Character>();

            if ((bool)spawnedCharacter)
            {
                spawnedCharacter.SetTamed(m_tamed);
                spawnedCharacter.SetLevel(m_item.m_itemData.m_quality);
            }

            ThirdParty.Mods.CllCBridge.PassTraits(zdo, spawned);

            m_hatchEffect.Create(position, rotation);
            m_nview.Destroy();
        }

        private void HandleMatrjoschka(ZDO zdo)
        {
            var grownPrefab = m_grownPrefab;
            var rotation = transform.rotation;

            var showHatchEffect = zdo.GetInt(Plugin.ZDOVars.z_eggShowHatchEffect, 1) == 1;
            if (showHatchEffect)
            {
                rotation = Quaternion.Slerp(
                    rotation,
                    UnityEngine.Random.rotation,
                    0.08f // maybe add a cinfig field in yaml for that one? hmmm naaaaa... maybe one day
                );
            }

            GameObject spawned = UnityEngine.Object.Instantiate(grownPrefab, transform.position, rotation);
            Character spawnedCharacter = spawned.GetComponent<Character>();

            if ((bool)spawnedCharacter)
            {
                // just in case we did something wrong
                spawnedCharacter.SetTamed(m_tamed);
                spawnedCharacter.SetLevel(m_item.m_itemData.m_quality);
            }
            else
            {
                spawned.GetComponent<ItemDrop>()?.SetQuality(m_item.m_itemData.m_quality);
                var itemdrop = spawned.GetComponent<ItemDrop>();
                itemdrop.m_autoPickup = ((EggGrow)__IAPI_instance).GetComponent<ItemDrop>().m_autoPickup;
            }

            ThirdParty.Mods.CllCBridge.PassTraits(zdo, spawned);

            m_hatchEffect.Create(transform.position, transform.rotation);
            m_nview.Destroy();
        }

        /** note: important part of Procreation.Procreate()
         
        Vector3 vector = base.transform.forward;
        if (m_spawnRandomDirection)
        {
            float f = UnityEngine.Random.Range(0f, (float)Math.PI * 2f);
            vector = new Vector3(Mathf.Cos(f), 0f, Mathf.Sin(f));
        }

        float num = ((m_spawnOffsetMax > 0f) ? UnityEngine.Random.Range(m_spawnOffset, m_spawnOffsetMax) : m_spawnOffset);
        GameObject gameObject = UnityEngine.Object.Instantiate(original, base.transform.position - vector * num, Quaternion.LookRotation(-base.transform.forward, Vector3.up));
        Character component = gameObject.GetComponent<Character>();
        if ((bool)component)
        {
            component.SetTamed(m_tameable.IsTamed());
            component.SetLevel(Mathf.Max(m_minOffspringLevel, m_character ? m_character.GetLevel() : m_minOffspringLevel));
        }
        else
        {
            gameObject.GetComponent<ItemDrop>()?.SetQuality(Mathf.Max(m_minOffspringLevel, m_character ? m_character.GetLevel() : m_minOffspringLevel));
        }

        **/

    }

}
