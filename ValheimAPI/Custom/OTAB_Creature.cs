using OfTamingAndBreeding.Data.Models.SubData;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OfTamingAndBreeding.ValheimAPI.Custom
{
    public sealed class OTAB_Creature : MonoBehaviour
    {

        //
        // MonsterAI
        //

        [SerializeField] internal bool m_tamedStayNearSpawn = false;
        [SerializeField] private int m_consumeItemDataIndex = -1;

        internal void SetCustomConsumeItems(Data.Models.Creature.MonsterAIConsumItemData[] consumeItems)
        {
            m_consumeItemDataIndex = Data.Runtime.MonsterAI.consumeItemData.Count;
            Data.Runtime.MonsterAI.consumeItemData.Add(consumeItems);
        }

        internal bool HasCustomConsumeItems(out Data.Models.Creature.MonsterAIConsumItemData[] consumeItems)
        {
            if (m_consumeItemDataIndex != -1)
            {
                consumeItems = Data.Runtime.MonsterAI.consumeItemData[m_consumeItemDataIndex];
                return true;
            }
            consumeItems = null;
            return false;
        }


        //
        // Tameable
        //

        [SerializeField] internal bool m_fedTimerDisabled = false;
        [SerializeField] internal bool m_tamingDisabled = false;
        [SerializeField] internal float m_starvingGraceFactor = -1;



        //
        // Procreation
        //

        [SerializeField] internal long m_partnerRecheckTicks = 0;
        [SerializeField] internal bool m_procreateWhileSwimming = true;
        [SerializeField] internal int m_maxSiblingsPerPregnancy = 0;
        [SerializeField] internal float m_extraSiblingChance = 0;
        [SerializeField] private int m_partnerListIndex = -1;
        [SerializeField] private int m_offspringListIndex = -1;
        [SerializeField] private int m_maxCreaturesPrefabsIndex = -1;

        internal void SetPartnerList(Data.Models.Creature.ProcreationPartnerData[] partnerList)
        {
            m_partnerListIndex = Data.Runtime.MonsterAI.partnerData.Count;
            Data.Runtime.MonsterAI.partnerData.Add(partnerList);
        }

        internal bool HasPartnerList(out Data.Models.Creature.ProcreationPartnerData[] partnerList)
        {
            if (m_partnerListIndex != -1)
            {
                partnerList = Data.Runtime.MonsterAI.partnerData[m_partnerListIndex];
                return true;
            }
            partnerList = null;
            return false;
        }

        internal void SetOffspringList(Data.Models.Creature.ProcreationOffspringData[] offspringList)
        {
            m_offspringListIndex = Data.Runtime.MonsterAI.offspringData.Count;
            Data.Runtime.MonsterAI.offspringData.Add(offspringList);
        }

        internal bool HasOffspringList(out Data.Models.Creature.ProcreationOffspringData[] offspringList)
        {
            if (m_offspringListIndex != -1)
            {
                offspringList = Data.Runtime.MonsterAI.offspringData[m_offspringListIndex];
                return true;
            }
            offspringList = null;
            return false;
        }

        internal void SetMaxCreaturesPrefabs(string[] prefabNames)
        {
            m_maxCreaturesPrefabsIndex = Data.Runtime.MonsterAI.maxCreaturesPrefabs.Count;
            Data.Runtime.MonsterAI.maxCreaturesPrefabs.Add(prefabNames);
        }

        internal bool HasMaxCreaturesPrefabs(out string[] prefabNames)
        {
            if (m_maxCreaturesPrefabsIndex != -1)
            {
                prefabNames = Data.Runtime.MonsterAI.maxCreaturesPrefabs[m_maxCreaturesPrefabsIndex];
                return true;
            }
            prefabNames = null;
            return false;
        }








    }
}
