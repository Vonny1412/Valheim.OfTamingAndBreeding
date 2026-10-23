using Jotunn.Managers;
using OfTamingAndBreeding.Components;
using OfTamingAndBreeding.Components.Traits;
using OfTamingAndBreeding.Data.Models;
using OfTamingAndBreeding.Processing.Core;
using OfTamingAndBreeding.Processing.Registry;
using OfTamingAndBreeding.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OfTamingAndBreeding.Processing
{
    internal partial class CreatureProcessor : DataProcessor<CreatureFile>
    {

        private void PrepareClone(string creatureName, CreatureFile data, UnityEngine.GameObject creature)
        {
            var model = $"{nameof(CreatureFile)}.{creatureName}";

            var creatureCharacter = creature.GetComponent<Character>();
            var creatureBaseAI = creature.GetComponent<BaseAI>();
            var creatureMonsterAI = creature.GetComponent<MonsterAI>();
            var creatureAnimalAI = creature.GetComponent<AnimalAI>();


            if (data.Clone.RemoveMonsterAI == true && creatureMonsterAI)
            {
                // BaseAI fields
                var baseAISnapshot = new Common.FieldsSnapshot<BaseAI>(creatureMonsterAI);

                // MonsterAI fields
                var m_avoidLand = creatureMonsterAI.m_avoidLand;
                var m_fleeInLava = creatureMonsterAI.m_fleeInLava;

                OTABPrefabRegistry.Instance.DestroyComponentIfExists<MonsterAI>(creatureName, creature);
                creatureAnimalAI = OTABPrefabRegistry.Instance.GetOrAddComponent<AnimalAI>(creatureName, creature);
                creatureMonsterAI = null;

                // BaseAI fields
                baseAISnapshot.ApplyTo(creatureAnimalAI);

                // MonsterAI fields
                var animalAITrait = AnimalAITrait.GetOrAddComponent(creature);
                animalAITrait.m_avoidLand = m_avoidLand;
                animalAITrait.m_fleeInLava = m_fleeInLava;

            }

            if (creature.TryGetComponent<CharacterDrop>(out var charDrop))
            {
                var drops = new List<CharacterDrop.Drop>();
                foreach (var source in charDrop.m_drops)
                {
                    var isSpecial =
                        creatureCharacter.m_boss ||
                        source.m_onePerPlayer ||
                        source.m_prefab.name.StartsWith("trophy", StringComparison.OrdinalIgnoreCase);
                    if (isSpecial)
                    {
                        continue;
                    }
                    drops.Add(new CharacterDrop.Drop
                    {
                        m_prefab = source.m_prefab,
                        m_amountMin = 0,
                        m_amountMax = source.m_amountMax > 1 ? (int)(source.m_amountMax / 2f + 0.5f) : source.m_amountMax,
                        m_chance = source.m_chance / 2f,
                        m_onePerPlayer = source.m_onePerPlayer,
                        m_levelMultiplier = false,
                        m_dontScale = true,
                    });
                }

                charDrop.m_drops = drops;
            }

            // display higher level creatures always as level 1 creature
            var levelFx = creature.GetComponentInChildren<LevelEffects>(true);
            if (levelFx != null)
            {
                levelFx.enabled = false;
            }

            var footStep = creature.GetComponent<FootStep>();
            if (footStep)
            {
                //footStep.enabled = false; this would result in NRE
                footStep.m_effects = footStep.m_effects.Select(effect => new FootStep.StepEffect
                {
                    m_name = effect.m_name,
                    m_motionType = effect.m_motionType,
                    m_material = effect.m_material,
                    m_effectPrefabs = Array.Empty<GameObject>(),
                }).ToList();
            }

            if (data.Clone.RemoveEffects != null)
            {
                VfxUtils.DisableVfx(creature, data.Clone.RemoveEffects);
            }

            Plugin.LogDebug($"{model}.{nameof(data.Clone)}: Setting Character values");
            if (data.Clone.Name != null) creatureCharacter.m_name = data.Clone.Name;


            var comp1 = creature.GetComponent<MovementDamage>();
            if (comp1)
            {
                // disable faders walk damage
                comp1.enabled = false;
                if (comp1.m_runDamageObject)
                {
                    comp1.m_runDamageObject.SetActive(false);
                }
            }

            if (data.Clone.Scale.HasValue)
            {
                var setScale = data.Clone.Scale.Value;

                Plugin.LogDebug($"{model}.{nameof(data.Clone)}: Setting custom scaling to {setScale}");

                creature.transform.localScale *= setScale;

                creatureCharacter.m_speed *= setScale;

                creatureCharacter.m_crouchSpeed *= setScale;
                creatureCharacter.m_walkSpeed *= setScale;
                creatureCharacter.m_runSpeed *= setScale;
                creatureCharacter.m_swimSpeed *= setScale;
                creatureCharacter.m_flySlowSpeed *= setScale;
                creatureCharacter.m_flyFastSpeed *= setScale;
                creatureBaseAI.m_randomMoveRange *= setScale;

                Plugin.LogDebug($"{model}.{nameof(data.Clone)}: Setting vfx scaling");
                VfxUtils.ScaleVfx(creature, setScale); // scale model particles

                Plugin.LogDebug($"{model}.{nameof(data.Clone)}: Setting death effects scaling");
                creatureCharacter.m_deathEffects = EffectUtils.CloneEffectList(creatureCharacter.m_deathEffects);
                foreach (var eff in creatureCharacter.m_deathEffects.m_effectPrefabs)
                {
                    var originalEffect = eff.m_prefab;

                    var clonedEffectName = $"{creatureName}_{originalEffect.name}";
                    var clonedEffect = PrefabManager.Instance.GetPrefab(clonedEffectName);

                    if (clonedEffect == null)
                    {
                        clonedEffect = PrefabManager.Instance.CreateClonedPrefab(
                            clonedEffectName,
                            originalEffect.name
                        );
                    }
                    else
                    {
                        VfxUtils.RestoreVfx(clonedEffect, originalEffect);
                    }
                    VfxUtils.ScaleVfx(clonedEffect, setScale);
                    eff.m_prefab = clonedEffect;

                    // ragdoll

                    var ragdoll = clonedEffect.GetComponent<Ragdoll>();
                    var originalRagdoll = originalEffect.GetComponent<Ragdoll>();

                    if (ragdoll && originalRagdoll)
                    {
                        ragdoll.transform.localScale = originalRagdoll.transform.localScale * setScale;
                        ragdoll.m_removeEffect = EffectUtils.CloneEffectList(originalRagdoll.m_removeEffect);

                        foreach (var eff2 in ragdoll.m_removeEffect.m_effectPrefabs)
                        {
                            var originalRemoveEffect = eff2.m_prefab;

                            var clonedRemoveEffectName = $"{creatureName}_{originalEffect.name}_{originalRemoveEffect.name}";
                            var clonedRemoveEffect = PrefabManager.Instance.GetPrefab(clonedRemoveEffectName);
                            if (clonedRemoveEffect == null)
                            {
                                clonedRemoveEffect = PrefabManager.Instance.CreateClonedPrefab(clonedRemoveEffectName, originalRemoveEffect.name);
                            }
                            else
                            {
                                VfxUtils.RestoreVfx(clonedRemoveEffect, originalRemoveEffect);
                            }

                            VfxUtils.ScaleVfx(clonedRemoveEffect, setScale);
                            eff2.m_prefab = clonedRemoveEffect;
                        }
                    }
                }

                var scaledCreature = OTABPrefabRegistry.Instance.GetOrAddComponent<ScaledCreature>(creatureName, creature);
                scaledCreature.m_scale = setScale;
                scaledCreature.m_animationScale = 1 / setScale;

                // do not hide the creature if we just go some steps away
                var lodGroup = creatureCharacter.transform.Find("Visual")?.GetComponent<LODGroup>();
                if (lodGroup)
                {
                    // todo: is this beeing restored correctly?
                    lodGroup.size /= setScale;
                }

                // stats

                var setHealthScale = setScale;
                var setAttackScale = setScale;
                creatureCharacter.m_health *= setHealthScale;
                scaledCreature.m_attackScale = setAttackScale;









            }


            // additional stuff

            creatureCharacter.m_boss = false;
            creatureCharacter.m_bossEvent = "";
            creatureCharacter.m_defeatSetGlobalKey = "";
            creatureCharacter.m_dreamCinematic = "";
            creatureCharacter.m_killedForAchievements = Utils.AchievementInclusion.Excluded;
            creatureBaseAI.m_spawnMessage = "";
            creatureBaseAI.m_deathMessage = "";

            if (creatureMonsterAI)
            {
                creatureMonsterAI.m_enableHuntPlayer = false;
            }

            /*
             * just testing to make a creature pickable
             * 
             * do not remove, do not uncomment
             * 
            if (offspringName == "OTAB_Bat_pup")
            {
                var itemDrop = PrefabRegistry.Instance.GetOrAddComponent<ItemDrop>(
                    offspringName,
                    offspring
                );

                itemDrop.m_itemData = new ItemDrop.ItemData();
                itemDrop.m_itemData.m_shared = new ItemDrop.ItemData.SharedData
                {
                    m_name = "$OTAB_enemy_bat_pup",
                    m_description = ",
                    m_itemType = ItemDrop.ItemData.ItemType.Misc,
                    m_maxStackSize = 1,
                    m_maxQuality = 1,
                    m_weight = 2f,
                    m_teleportable = true,
                };

                itemDrop.m_itemData.m_stack = 1;
                itemDrop.m_itemData.m_quality = 1;
                itemDrop.m_itemData.m_variant = 0;
            }
            */

        }

    }
}
