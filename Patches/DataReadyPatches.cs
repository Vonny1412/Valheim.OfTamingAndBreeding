using HarmonyLib;
using OfTamingAndBreeding.Data;
using OfTamingAndBreeding.ValheimAPI;
using OfTamingAndBreeding.ValheimAPI.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using YamlDotNet.Core.Tokens;

namespace OfTamingAndBreeding.Patches
{
    [HarmonyPatch]
    internal class DataReadyPatches : PatchGroup<DataReadyPatches>
    {
        public static new void Install() => PatchGroup<DataReadyPatches>.Install();
        public static new void Uninstall() => PatchGroup<DataReadyPatches>.Uninstall();


        //
        // Animator
        //


        [HarmonyPatch(typeof(Animator), "SetTrigger", new[] { typeof(string) })]
        [HarmonyPostfix]
        static void Animator_SetTrigger_Postfix(Animator __instance, string name)
        {
            if (name != "consume") return;

            var character = __instance.GetComponentInParent<Character>();
            if (!character) return;

            var prefabName = Utils.GetPrefabName(character.gameObject.name);
            if (string.IsNullOrEmpty(prefabName)) return;

            var runner = character.GetComponent<ValheimAPI.Custom.OTAB_ConsumeClipOverlay>();
            if (runner)
            {
                runner.PlayOverlay(__instance, speed: 1f);
            }

        }

        //
        // BaseAI
        //

        [HarmonyPatch(typeof(BaseAI), "Awake")]
        [HarmonyPostfix]
        static void BaseAI_Awake_Postfix(BaseAI __instance)
        {
            var nview = __instance.GetZNetView();
            if (nview.IsOwner())
            {
                // todo: add config for this or an ingame debug command
                /*
                var tameable = __instance.GetComponent<Tameable>();
                if (tameable && tameable.m_commandable == false && __instance.GetPatrolPoint(out var point))
                {
                    __instance.ResetPatrolPoint();
                    Plugin.LogWarning($"ResetPatrolPoint: {__instance.name}");
                }
                */
            }
        }

        [HarmonyPatch(typeof(BaseAI), "IdleMovement")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        static bool BaseAI_IdleMovement_Prefix(BaseAI __instance, float dt)
        {
            return __instance.IdleMovement_PatchPrefix(dt);
        }

        [HarmonyPatch(typeof(BaseAI), "IsEnemy", new[] { typeof(Character), typeof(Character) })]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        static bool BaseAI_IsEnemy_Prefix(BaseAI __instance, Character a, Character b, ref bool __result)
        {
            return __instance.IsEnemy_PatchPrefix(a, b, ref __result);
        }

        [HarmonyPatch(typeof(BaseAI), "IsEnemy", new[] { typeof(Character), typeof(Character) })]
        [HarmonyFinalizer]
        static void BaseAI_IsEnemy_Finalizer(Exception __exception)
        {
            BaseAIExtensions.IsEnemyContext.Cleanup();
        }

        //
        // Character
        //

        [HarmonyPatch(typeof(Character), "Awake")]
        [HarmonyPostfix]
        static void Character_Awake_Postfix(Character __instance)
        {
            __instance.SetCharacterStuffIfTamed();
        }

        /*
        [HarmonyPatch(typeof(Character), "GetGroup")]
        [HarmonyPrefix]
        static bool Character_GetGroup_Prefix(Character __instance, ref string __result)
        {
            if (__instance.IsTamed())
            {
                var prefabName = Utils.GetPrefabName(__instance.gameObject.name);
                if (Runtime.Character.TryGetGroupWhenTamed(prefabName, out string group))
                {
                    __result = group;
                    return false;
                }
            }
            return true;
        }
        */

        [HarmonyPatch(typeof(Character), "IsTamed", new Type[0])]
        [HarmonyPostfix]
        static void Character_IsTamed_Postfix(Character __instance, ref bool __result)
        {
            if (!__result) return; // not tamed by default

            // we temporarly need to change the original returned value
            if (BaseAIExtensions.IsEnemyContext.Active && __instance == BaseAIExtensions.IsEnemyContext.TargetInstance)
            {
                __result = false; // temporary untamed
            }
        }


        //
        // EffectList
        //

        [HarmonyPatch(typeof(EffectList), "Create")]
        [HarmonyPrefix]
        static void OTAB_EffectList_Create_Prefix(Transform baseParent, ref float scale)
        {
            if (baseParent && baseParent.TryGetComponent<OTAB_ScaledCreature>(out var scaled))
            {
                scale = scaled.m_customEffectScale;
            }
        }


        //
        // EggGrow
        //

        [HarmonyPatch(typeof(EggGrow), "CanGrow")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        static void EggGrow_CanGrow_Postfix(EggGrow __instance, ref bool __result)
        {
            // postfix = less calls
            if (__result == false)
            {
                // cannot grow afterall
                return;
            }
            __result = __instance.CanGrow_PatchPostfix();
        }

        [HarmonyPatch(typeof(EggGrow), "GrowUpdate")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        static bool EggGrow_GrowUpdate_Prefix(EggGrow __instance)
        {
            return __instance.GrowUpdate_PatchPrefix();
        }

        [HarmonyPatch(typeof(EggGrow), "Start")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        static void EggGrow_Start_Postfix(EggGrow __instance)
        {
            __instance.Start_PatchPostfix();
        }

        [HarmonyPatch(typeof(EggGrow), "UpdateEffects")]
        [HarmonyPostfix]
        static void EggGrow_UpdateEffects_Postfix(EggGrow __instance, float grow)
        {
            foreach (var r in __instance.GetComponentsInChildren<ParticleSystemRenderer>(true))
            {
                r.enabled = grow == 0;
            }
        }


        //
        // Growup
        //

        [HarmonyPatch(typeof(Growup), "GrowUpdate")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        static bool Growup_GrowUpdate_Prefix(Growup __instance)
        {
            return __instance.GrowUpdate_PatchPrefix();
        }

        [HarmonyPatch(typeof(Growup), "Start")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        static void Growup_Start_Postfix(Growup __instance)
        {
            __instance.Start_PatchPostfix();
        }


        //
        // Humanoid
        //

        [HarmonyPatch(typeof(Humanoid), "DropItem")]
        [HarmonyPrefix]
        static void Humanoid_DropItem_Prefix(Humanoid __instance)
        {
            // entry point for RequireFoodDroppedByPlayer-feature
            ItemDropExtensions.DropContext.DroppedByPlayer = __instance.IsPlayer() ? 1 : 0;
        }

        [HarmonyPatch(typeof(Humanoid), "DropItem")]
        [HarmonyFinalizer]
        static void Humanoid_DropItem_Finalizer()
        {
            ItemDropExtensions.DropContext.Clear();
        }

        //
        // Inventory
        //

        [HarmonyPatch(typeof(Inventory), "AddItem", new[] { typeof(ItemDrop.ItemData), typeof(int), typeof(int), typeof(int) })]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        static bool Inventory_AddItem_Prefix(Inventory __instance, ItemDrop.ItemData item, int amount, int x, int y, ref bool __result)
        {
            ItemDrop.ItemData itemAt = __instance.GetItemAt(x, y);
            if (itemAt == null) return true;
            if (item?.m_shared == null || itemAt?.m_shared == null) return true;
            if (item.m_shared.m_name != itemAt.m_shared.m_name) return true;
            if (Data.Runtime.ItemData.IsRegisteredEggBySharedName(item.m_shared.m_name) == false)
            {
                return true;
            }
            // OTAB eggs store level in ItemData.quality.
            // Valheim ignores quality when MaxQuality == 1 -> mixed stacks can "promote".
            // Prevent stacking OTAB eggs with different quality.
            if (itemAt.m_quality != item.m_quality)
            {
                __result = false;
                return false;
            }
            return true;
        }

        //
        // InventoryGrid
        //

        [HarmonyPatch(typeof(InventoryGrid), "DropItem", new[] { typeof(Inventory), typeof(ItemDrop.ItemData), typeof(int), typeof(Vector2i) })]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        static bool InventoryGrid_DropItem_Prefix(InventoryGrid __instance, Inventory fromInventory, ItemDrop.ItemData item, int amount, Vector2i pos, ref bool __result)
        {
            var m_inventory = __instance.GetInventory();
            ItemDrop.ItemData itemAt = m_inventory.GetItemAt(pos.x, pos.y);
            if (itemAt == null) return true;
            if (item?.m_shared == null || itemAt?.m_shared == null) return true;
            if (item.m_shared.m_name != itemAt.m_shared.m_name) return true;

            if (Data.Runtime.ItemData.IsRegisteredEggBySharedName(item.m_shared.m_name))
            {
                // both are otab-eggs
                if (itemAt.m_quality != item.m_quality)
                {
                    // default behavior
                    // we are just removing the limitation of quality>1
                    // maybe a user has forgotten to change egg max quality
                    // this is just a saveguard to prevent stacking two eggs of different quality
                    fromInventory.RemoveItem(item);
                    fromInventory.MoveItemToThis(m_inventory, itemAt, itemAt.m_stack, item.m_gridPos.x, item.m_gridPos.y);
                    m_inventory.MoveItemToThis(fromInventory, item, amount, pos.x, pos.y);

                    __result = true;
                    return false;
                }
            }

            return true;
        }

        //
        // ItemDrop
        //

        [HarmonyPatch(typeof(ItemDrop), "DropItem")]
        [HarmonyPostfix]
        static void ItemDrop_DropItem_Postfix(ItemDrop __instance, ItemDrop.ItemData item, int amount, Vector3 position, Quaternion rotation, ItemDrop __result)
        {
            // do not use __instance !!!
            // because __result is the item that has been dropped
            __result.HandleItemDropped();
        }

        [HarmonyPatch(typeof(ItemDrop), "RemoveOne")]
        [HarmonyPrefix]
        static void ItemDrop_RemoveOne_Prefix(ItemDrop __instance)
        {
            // used for RequireFoodDroppedByPlayer-feature
            // because when a creature eats food with a stack size of 1 that item would be destroyed
            // thats why we need to patch this one to pass the flags to Tameable_OnConsumedItem_Patch
            __instance.RemoveOne_PatchPrefix();
            // do return nothing (always call original method)
        }

        [HarmonyPatch(typeof(ItemDrop), "SetQuality")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        static void ItemDrop_SetQuality_Postfix(ItemDrop __instance)
        {
            // we need to multiply because localScale has already been set to variable scaling according to stuff like quality
            var prefabName = Utils.GetPrefabName(__instance.gameObject.name);
            __instance.transform.localScale *= Runtime.ItemData.GetCustomScale(prefabName);
        }


        //
        // MonsterAI
        //

        [HarmonyPatch(typeof(MonsterAI), "FindClosestConsumableItem")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        static bool MonsterAI_FindClosestConsumableItem_Prefix(MonsterAI __instance, ref ItemDrop __result)
        {
            if (Plugin.Configs.UseBetterSearchForFood.Value == true)
            {
                __result = __instance.FindNearbyConsumableItem(__instance.m_consumeSearchRange, __instance.m_consumeItems);
            }
            else
            {
                __result = __instance.FindClosestConsumableItem(__instance.m_consumeSearchRange, __instance.m_consumeItems);
            }
            // skrew it, just replace it completly!
            // because i dont like the vanilla CanConsume-method
            return false;
        }

        [HarmonyPatch(typeof(MonsterAI), "UpdateAI")]
        [HarmonyPrefix]
        static bool MonsterAI_UpdateAI_Prefix(MonsterAI __instance, float dt)
        {
            return __instance.UpdateAI_PatchPrefix(dt);
        }


        //
        // Procreation
        //

        [HarmonyPatch(typeof(Procreation), "Awake")]
        [HarmonyPostfix]
        static void Procreation_Awake_Postfix(Procreation __instance)
        {
            __instance.Awake_PatchPostfix();
        }

        [HarmonyPatch(typeof(Procreation), "IsDue")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        static void Procreation_IsDue_Prefix(Procreation __instance)
        {
            var trait = __instance.GetComponent<ValheimAPI.Custom.OTAB_ProcreationTrait>();
            trait.m_realPregnancyDuration = __instance.m_pregnancyDuration;
        }

        [HarmonyPatch(typeof(Procreation), "Procreate")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        static bool Procreation_Procreate_Prefix(Procreation __instance)
        {
            try
            {
                return __instance.Procreate_PatchPrefix(); // override vanilla
            }
            catch (Exception ex)
            {
                Plugin.LogFatal($"Procreation_Procreate_Prefix: {ex}");
                return true; // fail-open: allow vanilla + other mods
            }
        }


        //
        // Tameable
        //

        [HarmonyPatch(typeof(Tameable), "Awake")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        static void Tameable_Awake_Postfix(Tameable __instance)
        {
            __instance.Awake_PatchPostfix();
        }

        [HarmonyPatch(typeof(Tameable), "DecreaseRemainingTime")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        static bool Tameable_DecreaseRemainingTime_Prefix(Tameable __instance, ref float time)
        {
            if (__instance.TryGetComponent<OTAB_Creature>(out var creature))
            {
                if (creature.m_tamingDisabled == true)
                {
                    return false;
                }
            }

            time *= __instance.GetRemainingTimeDecreaseFactor();
            return true;
        }

        [HarmonyPatch(typeof(Tameable), "IsHungry")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        static bool Tameable_IsHungry_Prefix(Tameable __instance)
        {
            //__instance.UpdateFedDuration();
            // i dont remember why i was updating the fed duration at this point
            // maybe i dont need to?
            return true;
        }

        [HarmonyPatch(typeof(Tameable), "OnConsumedItem")]
        [HarmonyPrefix]
        static bool Tameable_OnConsumedItem_Prefix(Tameable __instance, ItemDrop item)
        {
            return __instance.OnConsumedItem_PrefixPatch(item);
        }

        [HarmonyPatch(typeof(Tameable), "OnConsumedItem")]
        [HarmonyFinalizer]
        static void Tameable_OnConsumedItem_Finalizer(Exception __exception)
        {
            TameableExtensions.ConsumeContext.Clear();
        }

        [HarmonyPatch(typeof(Tameable), "RPC_Command")]
        [HarmonyPrefix]
        static bool Tameable_RPC_Command_Prefix(Tameable __instance, long sender, ZDOID characterID, bool message)
        {
            return __instance.RPC_Command_PatchPrefix(sender, characterID, message);
        }

        [HarmonyPatch(typeof(Tameable), "Tame")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        static bool Tameable_Tame_Prefix(Tameable __instance)
        {
            //var prefabName = Utils.GetPrefabName(__instance.gameObject.name);
            //if (Runtime.Tameable.GetTamingDisabled(prefabName))
            //{
            //return false;
            //}
            return __instance.Tame_PatchPrefix();
        }

        [HarmonyPatch(typeof(Tameable), "Tame")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        static void Tameable_Tame_Postfix(Tameable __instance)
        {
            __instance.Tame_PatchPostfix();
        }

        [HarmonyPatch(typeof(Tameable), "TamingUpdate")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        static bool Tameable_TamingUpdate_Prefix(Tameable __instance)
        {
            if (__instance.TryGetComponent<OTAB_Creature>(out var creature))
            {
                if (creature.m_tamingDisabled == true)
                {
                    return false;
                }
            }

            return __instance.TamingUpdate_PatchPrefix();
        }


        //
        // ZNetView
        //

        [HarmonyPatch(typeof(ZNetView), "OnDestroy")]
        [HarmonyPrefix]
        static void ZNetView_OnDestroy_Prefix(ZNetView __instance)
        {
            if (!ValheimAPI.Lifecycle.CleanupMarks.IsMarked(__instance))
                return;

            ValheimAPI.Lifecycle.CleanupMarks.Unmark(__instance);

            var go = __instance.gameObject;
            if (!go) return;


            // todo: remove all extradata classes and add them to custom components

            var eggGrow = go.GetComponent<EggGrow>();
            if (eggGrow) ValheimAPI.EggGrowExtensions.EggGrowExtraData.Remove(eggGrow);

            var baseAI = go.GetComponent<BaseAI>();
            if (baseAI) ValheimAPI.BaseAIExtensions.BaseAIExtraData.Remove(baseAI);

            var character = go.GetComponent<Character>();
            if (character) ValheimAPI.CharacterExtensions.CharacterExtraData.Remove(character);

            var growup = go.GetComponent<Growup>();
            if (growup) ValheimAPI.GrowupExtensions.GrowupExtraData.Remove(growup);

            var itemDrop = go.GetComponent<ItemDrop>();
            if (itemDrop) ValheimAPI.ItemDropExtensions.ItemDropExtraData.Remove(itemDrop);

        }


        //
        // ZSyncAnimation
        //

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
        static void ZSyncAnimation_SetFloat_Prefix(ZSyncAnimation __instance, int hash, ref float value)
        {
            if (__instance && __instance.TryGetComponent<OTAB_ScaledCreature>(out var scaled))
            {
                value *= scaled.m_customAnimationScale;
            }
        }

    }
}
