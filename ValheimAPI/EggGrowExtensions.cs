using Jotunn.Managers;
using OfTamingAndBreeding.Data;
using OfTamingAndBreeding.Data.Models;
using OfTamingAndBreeding.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.Networking.UnityWebRequest;

namespace OfTamingAndBreeding.ValheimAPI
{
    internal static class EggGrowExtensions
    {
        internal sealed class EggGrowExtraData : Lifecycle.ExtraData<EggGrow, EggGrowExtraData>
        {

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ZNetView GetZNetView(this EggGrow that)
            => LowLevel.EggGrow.__IAPI_m_nview_Invoker.Get(that);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UpdateEffects(this EggGrow that, float grow)
            => LowLevel.EggGrow.__IAPI_UpdateEffects_Invoker1.Invoke(that, grow);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CanGrow(this EggGrow that)
            => LowLevel.EggGrow.__IAPI_CanGrow_Invoker1.Invoke(that);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemDrop GetItemDrop(this EggGrow that)
            => LowLevel.EggGrow.__IAPI_m_item_Invoker.Get(that);

        private static void RPC_UpdateEffects(this EggGrow eggGrow, long sender, float grow)
        {
            var m_nview = eggGrow.GetZNetView();
            if (m_nview && m_nview.IsValid())
            {
                eggGrow.UpdateEffects(grow);
            }
        }

        private static void RPC_HatchAndDestroy(this EggGrow eggGrow, long sender)
        {
            var m_nview = eggGrow.GetZNetView();
            if (m_nview && m_nview.IsValid())
            {
                eggGrow.m_hatchEffect?.Create(eggGrow.transform.position, eggGrow.transform.rotation);
                if (m_nview.IsOwner())
                {
                    m_nview.Destroy();
                }
            }
        }

        public static void Start_PatchPostfix(this EggGrow eggGrow)
        {
            var m_nview = eggGrow.GetZNetView();
            if (m_nview && m_nview.IsValid())
            {
                // sadly we need to wrap the target methods because valheim is doing this:
                // > m_action.DynamicInvoke(ZNetView.Deserialize(rpc, m_action.Method.GetParameters(), pkg));
                // the first param of the extension methods (this EggGrow eggGrow) is making problems while deserializing
                m_nview.Register<float>("RPC_UpdateEffects", (long sender, float grow) => eggGrow.RPC_UpdateEffects(sender, grow));
                m_nview.Register("RPC_HatchAndDestroy", (long sender) => eggGrow.RPC_HatchAndDestroy(sender));
            }

            var prefabName = Utils.GetPrefabName(eggGrow.gameObject.name);
            if (!Runtime.EggGrow.TryGetBaseGrowTime(prefabName, out float _))
            {
                var prefab = PrefabManager.Instance.GetPrefab(prefabName);
                var prefabEggGrow = prefab.GetComponent<EggGrow>();
                Runtime.EggGrow.SetBaseGrowTime(prefabName, prefabEggGrow.m_growTime);
            }

            eggGrow.UpdateGrowTime();
        }

        public static void UpdateGrowTime(this EggGrow eggGrow)
        {
            var m_nview = eggGrow.GetZNetView();
            if (!m_nview || !m_nview.IsValid()) return;
            var zdo = m_nview.GetZDO();
            if (zdo == null) return;

            var globalFactor = Plugin.Configs.GlobalGrowTimeFactor.Value;
            if (globalFactor < 0f)
            {
                eggGrow.UpdateGrowTime(1f); // back to base
                return;
            }
            var totalFactor = globalFactor;
            eggGrow.UpdateGrowTime(totalFactor);
        }

        private static void UpdateGrowTime(this EggGrow eggGrow, float totalFactor)
        {
            if (totalFactor >= 0) // yes, we do allow 0, too
            {
                var prefabName = Utils.GetPrefabName(eggGrow.gameObject.name);
                if (Runtime.EggGrow.TryGetBaseGrowTime(prefabName, out float baseGrowTime))
                {
                    eggGrow.m_growTime = baseGrowTime * totalFactor;
                }
            }
        }

        public static bool CanGrow_PatchPostfix(this EggGrow eggGrow)
        {
            var eggPosition = eggGrow.transform.position;
            var eggPrefabName = Utils.GetPrefabName(eggGrow.gameObject.name);

            var needsAnyBiome = Runtime.EggGrow.GetEggNeedsAnyBiome(eggPrefabName);
            if (needsAnyBiome != Heightmap.Biome.None && !Helpers.EnvironmentHelper.IsInBiome(eggPosition, needsAnyBiome))
            {
                return false;
            }

            var needsLiquid = Runtime.EggGrow.GetEggNeedsLiquid(eggPrefabName);
            if (needsLiquid != null)
            {
                switch (needsLiquid.Type)
                {
                    case Helpers.EnvironmentHelper.LiquidTypeEx.Water:
                        if (!Helpers.EnvironmentHelper.IsInWater(eggPosition, needsLiquid.Depth))
                        {
                            return false;
                        }
                        break;
                    case Helpers.EnvironmentHelper.LiquidTypeEx.Tar:
                        if (!Helpers.EnvironmentHelper.IsInTar(eggPosition, needsLiquid.Depth))
                        {
                            return false;
                        }
                        break;

                        // todo: lava?
                }
            }

            // ... maybe more to come
            return true;
        }



        public static bool GrowUpdate_PatchPrefix(this EggGrow eggGrow)
        {
            var m_nview = eggGrow.GetZNetView();
            if (!m_nview.IsOwner())
            {
                return true;
            }

            var zdo = m_nview.GetZDO();
            var m_item = eggGrow.GetItemDrop();

            var s_growStart = zdo.GetFloat(ZDOVars.s_growStart, 0f);
            if (m_item.m_itemData.m_stack > 1)
            {
                eggGrow.UpdateEffects(s_growStart);
                return false;
            }

            var prefabName = Utils.GetPrefabName(eggGrow.gameObject.name);

            var z_EggBehavior = zdo.GetInt(Plugin.ZDOVars.z_EggBehavior, Plugin.ZDOVars.EggBehavior.Unknown);
            if (z_EggBehavior == Plugin.ZDOVars.EggBehavior.Unknown)
            {
                // determine behavior

                var data = Egg.Get(prefabName);
                if (data == null)
                {
                    z_EggBehavior = ZNetHelper.SetInt(zdo, Plugin.ZDOVars.z_EggBehavior, Plugin.ZDOVars.EggBehavior.Vanilla);
                }
                else
                {
                    var foundRandom = Data.Models.SubData.RandomData.FindRandom<Egg.EggGrowGrownData>(data.EggGrow.Grown, out Egg.EggGrowGrownData grownEntry);
                    if (!foundRandom) // should not happen but whatever
                    {
                        z_EggBehavior = ZNetHelper.SetInt(zdo, Plugin.ZDOVars.z_EggBehavior, Plugin.ZDOVars.EggBehavior.Vanilla, z_EggBehavior);
                        return false;
                    }

                    var z_eggGrownPrefab = ZNetHelper.SetString(zdo, Plugin.ZDOVars.z_eggGrownPrefab, grownEntry.Prefab);
                    var z_eggGrownTamed = ZNetHelper.SetInt(zdo, Plugin.ZDOVars.z_eggGrownTamed, grownEntry.Tamed ? 1 : 0);
                    var z_eggShowHatchEffect = ZNetHelper.SetInt(zdo, Plugin.ZDOVars.z_eggShowHatchEffect, grownEntry.ShowHatchEffect ? 1 : 0);

                    z_EggBehavior = ZNetHelper.SetInt(zdo, Plugin.ZDOVars.z_EggBehavior, Plugin.ZDOVars.EggBehavior.OTAB, z_EggBehavior);
                }
            }

            var timeSeconds = (float)ZNet.instance.GetTimeSeconds();
            if (eggGrow.CanGrow())
            {
                if (s_growStart == 0f)
                {
                    s_growStart = ZNetHelper.SetFloat(zdo, ZDOVars.s_growStart, timeSeconds, s_growStart);
                }
            }
            else
            {
                s_growStart = ZNetHelper.SetFloat(zdo, ZDOVars.s_growStart, 0f, s_growStart);
            }

            m_nview.InvokeRPC(ZNetView.Everybody, "RPC_UpdateEffects", s_growStart+0);
            //eggGrow.UpdateEffects(s_growStart);

            if (s_growStart > 0f && timeSeconds > (s_growStart + eggGrow.m_growTime))
            {
                var position = eggGrow.transform.position;
                var rotation = eggGrow.transform.rotation;
                var showHatchEffect = true;

                switch (z_EggBehavior)
                {

                    case Plugin.ZDOVars.EggBehavior.Vanilla:
                        {
                            if (eggGrow.m_grownPrefab == null)
                            {
                                Plugin.LogWarning($"Egg '{prefabName}#{m_nview.GetInstanceID()}' m_grownPrefab is null");
                                return false;
                            }
                        }
                        break;

                    case Plugin.ZDOVars.EggBehavior.OTAB:
                        {
                            var z_eggGrownPrefab = zdo.GetString(Plugin.ZDOVars.z_eggGrownPrefab, "");
                            var z_eggGrownTamed = zdo.GetInt(Plugin.ZDOVars.z_eggGrownTamed, 1);
                            var z_eggShowHatchEffect = zdo.GetInt(Plugin.ZDOVars.z_eggShowHatchEffect, 1);

                            eggGrow.m_grownPrefab = ZNetScene.instance.GetPrefab(z_eggGrownPrefab);
                            eggGrow.m_tamed = z_eggGrownTamed == 1;
                            showHatchEffect = z_eggShowHatchEffect == 1;

                            if (eggGrow.m_grownPrefab == null)
                            {
                                // this should not happen but just to be save!
                                Plugin.LogWarning($"Egg '{prefabName}#{m_nview.GetInstanceID()}' z_eggGrownPrefab '{z_eggGrownPrefab}' does not exist");
                                return false;
                            }
                        }
                        break;

                }

                if (showHatchEffect)
                {
                    // just jiggle a lil bit
                    rotation = Quaternion.Slerp(rotation, UnityEngine.Random.rotation, 0.08f);
                }

                GameObject spawned = UnityEngine.Object.Instantiate(eggGrow.m_grownPrefab, position, rotation);
                Character spawnedCharacter = spawned.GetComponent<Character>();

                if ((bool)spawnedCharacter)
                {
                    spawnedCharacter.SetTamed(eggGrow.m_tamed);
                    spawnedCharacter.SetLevel(m_item.m_itemData.m_quality);

                    var spawnedTameable = spawnedCharacter.GetComponent<Tameable>();
                }
                else
                {
                    spawned.GetComponent<ItemDrop>()?.SetQuality(m_item.m_itemData.m_quality);
                }

                ThirdParty.Mods.CllCBridge.PassTraits(zdo, spawned);

                if (showHatchEffect)
                {
                    m_nview.InvokeRPC(ZNetView.Everybody, "RPC_HatchAndDestroy");
                    //eggGrow.m_hatchEffect?.Create(position, rotation);
                }

                //m_nview.Destroy();
            }
            return false;
        }

    }
}
