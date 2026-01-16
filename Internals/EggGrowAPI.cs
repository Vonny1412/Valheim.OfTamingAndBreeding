using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OfTamingAndBreeding.Internals
{
    
    internal class EggGrowAPI : API.EggGrow
    {

        private static readonly ConditionalWeakTable<EggGrow, EggGrowAPI> instances
            = new ConditionalWeakTable<EggGrow, EggGrowAPI>();
        public static EggGrowAPI GetOrCreate(EggGrow __instance)
            => instances.GetValue(__instance, (EggGrow inst) => new EggGrowAPI(inst));
        public static bool TryGetAPI(EggGrow __instance, out EggGrowAPI api)
            => instances.TryGetValue(__instance, out api);

        public EggGrowAPI(EggGrow __instance) : base(__instance)
        {
        }

        public bool GrowUpdate_Prefix(ZDO zdo)
        {

            var data = Data.Models.Egg.Get(Utils.GetPrefabName(name));
            if (data == null)
            {
                // no custom handling
                zdo.Set(Plugin.ZDOVars.s_EggBehavior, Plugin.ZDOVars.EggBehavior.Vanilla);
                return true;
            }

            var grownName = zdo.GetString(Plugin.ZDOVars.s_eggGrownPrefab, "");
            grownName = Utils.GetPrefabName(grownName);
            var grown = ZNetScene.instance.GetPrefab(grownName);
            if (grown == null)
            {
                zdo.Set(ZDOVars.s_growStart, 0f);
                // trigger new choosing
            }

            float growStart = zdo.GetFloat(ZDOVars.s_growStart, 0f);
            if (growStart == 0f)
            {

                // select new grown
                var grownEntry = Data.Models.SubData.WeightEntry.GetRandom<Data.Models.Egg.EggGrowGrownData>(data.EggGrow.grown);
                if (grownEntry == null)
                {
                    // should not happen but whatever
                    zdo.Set(Plugin.ZDOVars.s_EggBehavior, Plugin.ZDOVars.EggBehavior.Vanilla); // vanilla
                    return false;
                }

                grownName = grownEntry.prefab;
                grown = ZNetScene.instance.GetPrefab(grownName);

                zdo.Set(Plugin.ZDOVars.s_eggGrownPrefab, grownName);
                zdo.Set(Plugin.ZDOVars.s_eggGrownTamed, grownEntry.tamed ? 1 : 0);
                zdo.Set(Plugin.ZDOVars.s_eggShowHatchEffect, grownEntry.showHatchEffect ? 1 : 0);

                var eggGrownEggData = Data.Models.Egg.Get(grownName);
                if (eggGrownEggData == null)
                {
                    zdo.Set(Plugin.ZDOVars.s_EggBehavior, Plugin.ZDOVars.EggBehavior.Vanilla); // vanilla
                }
                else
                {
                    zdo.Set(Plugin.ZDOVars.s_EggBehavior, Plugin.ZDOVars.EggBehavior.Matrjoschka); // we gonna take care!
                }
            }

            // update values
            var grownTamed = zdo.GetInt(Plugin.ZDOVars.s_eggGrownTamed, 1) == 1;
            var showHatchEffect = zdo.GetInt(Plugin.ZDOVars.s_eggShowHatchEffect, 1) == 1;
            m_grownPrefab = grown;
            m_tamed = grownTamed;
            if (m_hatchEffect != null)
            {
                foreach (var eff in m_hatchEffect.m_effectPrefabs)
                {
                    eff.m_enabled = showHatchEffect;
                }
            }

            var behavior = zdo.GetInt(Plugin.ZDOVars.s_EggBehavior, Plugin.ZDOVars.EggBehavior.Unknown);
            switch(behavior)
            {

                case Plugin.ZDOVars.EggBehavior.Matrjoschka:
                    HandleMatrjoschka(zdo);
                    return false;

            }

            return true; // Plugin.ZDO.EggBehavior.Vanilla
        }

        private void HandleMatrjoschka(ZDO zdo)
        {
            float num = zdo.GetFloat(ZDOVars.s_growStart);
            if (m_item.m_itemData.m_stack > 1)
            {
                UpdateEffects(num);
                return;
            }

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
                var grownPrefab = m_grownPrefab;

                var rotation = transform.rotation;

                var showHatchEffect = zdo.GetInt(Plugin.ZDOVars.s_eggShowHatchEffect, 1) == 1;
                if (showHatchEffect)
                {
                    rotation = Quaternion.Slerp(
                        rotation,
                        UnityEngine.Random.rotation,
                        0.08f // strength (0..1)
                    );
                }

                GameObject gameObject = UnityEngine.Object.Instantiate(grownPrefab, transform.position, rotation);

                Character component = gameObject.GetComponent<Character>();
                if (component)
                {
                    // just in case we did something wrong
                    component.SetTamed(m_tamed);
                    component.SetLevel(m_item.m_itemData.m_quality);
                }
                else
                {
                    gameObject.GetComponent<ItemDrop>()?.SetQuality(m_item.m_itemData.m_quality);
                    var itemdrop = gameObject.GetComponent<ItemDrop>();
                    itemdrop.m_autoPickup = ((EggGrow)__IAPI_instance).GetComponent<ItemDrop>().m_autoPickup;
                }
                m_hatchEffect.Create(transform.position, transform.rotation);
                m_nview.Destroy();
            }

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
