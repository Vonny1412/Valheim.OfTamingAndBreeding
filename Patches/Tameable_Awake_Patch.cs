using HarmonyLib;
using OfTamingAndBreeding.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Patches
{
    [HarmonyPatch(typeof(Tameable), "Awake")]
    static class Tameable_Awake_Patch
    {
        [HarmonyPriority(Priority.Last)]
        static void Postfix(Tameable __instance)
        {
            var tameableAPI = Internals.TameableAPI.GetOrCreate(__instance);
            tameableAPI.UpdateFedDuration();

            var animalAI = __instance.GetComponent<AnimalAI>();
            if (animalAI != null) // the tameable is an animal
            {
                // check if this animal is an animal that we are handling
                if (Internals.AnimalAIAPI.TryGet(animalAI, out Internals.AnimalAIAPI animalAIAPI))
                {
                    // used for making the animal tameable
                    tameableAPI.animalAIAPI = animalAIAPI;
                    tameableAPI.animalAIAPI.m_onConsumedItem = (Action<ItemDrop>)Delegate.Combine(animalAIAPI.m_onConsumedItem, new Action<ItemDrop>(tameableAPI.OnConsumedItem));
                }
            }

        }
    }

    /** original method
    private void Awake()
    {
        m_nview = GetComponent<ZNetView>();
        m_character = GetComponent<Character>();
        m_monsterAI = GetComponent<MonsterAI>();
        m_piece = GetComponent<Piece>();
        if ((bool)m_character)
        {
            Character character = m_character;
            character.m_onDeath = (Action)Delegate.Combine(character.m_onDeath, new Action(OnDeath));
        }

        if ((bool)m_monsterAI)
        {
            MonsterAI monsterAI = m_monsterAI;
            monsterAI.m_onConsumedItem = (Action<ItemDrop>)Delegate.Combine(monsterAI.m_onConsumedItem, new Action<ItemDrop>(OnConsumedItem));
        }

        if (m_nview.IsValid())
        {
            m_nview.Register<ZDOID, bool>("Command", RPC_Command);
            m_nview.Register<string, string>("SetName", RPC_SetName);
            m_nview.Register("RPC_UnSummon", RPC_UnSummon);
            if (m_saddle != null)
            {
                m_nview.Register("AddSaddle", RPC_AddSaddle);
                m_nview.Register<bool>("SetSaddle", RPC_SetSaddle);
                SetSaddle(HaveSaddle());
            }

            InvokeRepeating("TamingUpdate", 3f, 3f);
        }

        if (m_startsTamed && (bool)m_character)
        {
            m_character.SetTamed(tamed: true);
        }

        if (m_randomStartingName.Count > 0 && m_nview.IsValid() && m_nview.GetZDO().GetString(ZDOVars.s_tamedName).Length == 0)
        {
            SetText(Localization.instance.Localize(m_randomStartingName[UnityEngine.Random.Range(0, m_randomStartingName.Count)]));
        }
    }
    **/

        }
