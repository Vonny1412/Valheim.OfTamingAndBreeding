using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Patches
{

    [HarmonyPatch(typeof(Tameable), "OnConsumedItem")]
    static class Tameable_OnConsumedItem_Patch
    {
        static bool Prefix(Tameable __instance, ItemDrop item)
        {

            if (!Helpers.ZNetHelper.TryGetZDO(__instance, out ZDO zdo, out ZNetView nview))
            {
                return true; // i dont care
            }

            if (nview.IsOwner())
            {
                var itemName = global::Utils.GetPrefabName(item.gameObject.name);
                //zdo.Set(Plugin.ZDO.s_lastConsumedItem, itemName);

                // handle multiplied fedDuration
                var prefabName = global::Utils.GetPrefabName(__instance.gameObject.name);
                var data = Data.Models.Creature.Get(prefabName);
                if (data != null && data.Tameable != null && data.MonsterAI != null && data.MonsterAI.ConsumeItems != null && data.MonsterAI.ConsumeItems.Length > 0)
                {

                    var fedDuration = data.Tameable.FedDuration;
                    foreach (var entry in data.MonsterAI.ConsumeItems)
                    {
                        if (entry.Prefab == itemName)
                        {
                            fedDuration *= entry.FedDurationMultiply;
                            break;
                        }
                    }
                    if (fedDuration >= 0)
                    {
                        // Intentionally allow 0:
                        // This means the creature will never become fed by this item.
                        // that way we can keep using taming system and can have items that do not trigger taming/procreation (like the original bjorn-behaviour)

                        __instance.m_fedDuration = fedDuration;
                        zdo.Set(Plugin.ZDOVars.s_fedDuration, fedDuration);
                    }
                }
            }
            else
            {
                var fedDuration = zdo.GetFloat(Plugin.ZDOVars.s_fedDuration, -1);
                if (fedDuration >= 0) // yes, we also allow 0
                {
                    __instance.m_fedDuration = fedDuration;
                }
                // we are alos updating the fed duration here:
                // Tameable_IsHungry_Patch
                // Tameable_Awake_Patch
            }

            // NOTE:
            // Tameable.OnConsumedItem is only triggered on the ZNetView owner.
            // Call chain:
            //   BaseAI.UpdateAI() -> owner check -> MonsterAI.UpdateConsumeItem() -> m_onConsumedItem
            // Non-owners never run UpdateConsumeItem and therefore never call OnConsumedItem.
            // Because of this, returning false here is safe and will not desync clients.
            // If droppedByAnyPlayer == 0 we intentionally block ResetFeedingTimer()
            // so world-spawned items cannot feed tameables.
            if (Plugin.Configs.RequireFoodDroppedByPlayer.Value)
            {
                int droppedByAnyPlayer = -1;
                if (item)
                {
                    var item_nview = item.GetComponent<ZNetView>();
                    if (item_nview && item_nview.IsValid())
                    {
                        var item_zdo = item_nview.GetZDO();
                        if (item_zdo != null)
                        {
                            droppedByAnyPlayer = item_zdo.GetInt(Plugin.ZDOVars.s_droppedByAnyPlayer, 0);
                        }
                    }
                }
                if (droppedByAnyPlayer < 0 && Contexts.ConsumeItemContext.HasValue && item && Contexts.ConsumeItemContext.LastItemInstanceId == item.GetInstanceID())
                {
                    droppedByAnyPlayer = Contexts.ConsumeItemContext.lastItemDroppedByAnyPlayer;
                }
                Contexts.ConsumeItemContext.Clear();
                if (droppedByAnyPlayer < 0)
                {
                    // we dont know if it has been dropped by player or not
                    // let valheim handle
                    return true;
                }
                if (droppedByAnyPlayer == 0)
                {
                    // definitly not dropped by player
                    // prevent ResetFeedingTimer
                    return false;
                }
                // dropped by player a player. let the creature consume it
                return true;
            }
            else
            {
                // let valheim handle
                Contexts.ConsumeItemContext.Clear();
                return true;
            }
        }
    }

    /** original method
    private void OnConsumedItem(ItemDrop item)
    {
        if (IsHungry())
        {
            m_sootheEffect.Create(m_character ? m_character.GetCenterPoint() : base.transform.position, Quaternion.identity);
        }

        ResetFeedingTimer();
    }
    **/

}
