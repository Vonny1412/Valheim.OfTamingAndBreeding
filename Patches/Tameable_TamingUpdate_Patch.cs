using HarmonyLib;
using OfTamingAndBreeding.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Patches
{
    [HarmonyPatch(typeof(Tameable), "TamingUpdate")]
    static class Tameable_TamingUpdate_Patch
    {
        [HarmonyPriority(Priority.Last)]
        static bool Prefix(Tameable __instance)
        {
            var prefabName = Utils.GetPrefabName(__instance.gameObject.name);
            if (Runtime.Tameable.GetTamingDisabled(prefabName))
            {
                // taming completly disabled
                return false;
            }

            var tameableAPI = Internals.TameableAPI.GetOrCreate(__instance);
            if (tameableAPI.IsAnimal())
            {
                // custom Tameable.TamingUpdate()
                tameableAPI.TamingAnimalUpdate();
                return false;
            }
            return true;
        }
    }

    /** original method
    private void TamingUpdate()
    {
        if (m_nview.IsValid() && m_nview.IsOwner() && !IsTamed() && !IsHungry() && (bool)m_monsterAI && !m_monsterAI.IsAlerted())
        {
            m_monsterAI.SetDespawnInDay(despawn: false);
            m_monsterAI.SetEventCreature(despawn: false);
            DecreaseRemainingTime(3f);
            if (GetRemainingTime() <= 0f)
            {
                Tame();
            }
            else
            {
                m_sootheEffect.Create(base.transform.position, base.transform.rotation);
            }
        }
    }
    **/

}
