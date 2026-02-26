using OfTamingAndBreeding.Data.Models.SubData;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OfTamingAndBreeding.Components
{
    public sealed class OTAB_Creature : MonoBehaviour
    {

        //
        // Character
        //

        [SerializeField] internal bool m_stickToFaction = false;
        [SerializeField] internal IsEnemyCondition m_tamedCanAttackTamed = IsEnemyCondition.Never;
        [SerializeField] internal IsEnemyCondition m_tamedCanBeAttackedByTamed = IsEnemyCondition.Never;
        [SerializeField] internal IsEnemyCondition m_tamedCanAttackPlayer = IsEnemyCondition.Never;
        [SerializeField] internal bool m_changeGroupWhenTamed = false;
        [SerializeField] internal string m_changeGroupWhenTamedTo = "";
        [SerializeField] internal bool m_changeFactionWhenTamed = false;
        [SerializeField] internal Character.Faction m_changeFactionWhenTamedTo = Character.Faction.Players;
        

        //
        // MonsterAI
        //

        [SerializeField] internal bool m_tamedStayNearSpawn = false;
        [SerializeField] private int m_consumeItemDataIndex = -1;

        internal void SetCustomConsumeItems(StaticContext.CreatureDataContext.ConsumeItem[] consumeItems)
        {
            m_consumeItemDataIndex = StaticContext.CreatureDataContext.consumeItemData.Count;
            StaticContext.CreatureDataContext.consumeItemData.Add(consumeItems);
        }

        internal bool HasCustomConsumeItems(out StaticContext.CreatureDataContext.ConsumeItem[] consumeItems)
        {
            if (m_consumeItemDataIndex != -1)
            {
                consumeItems = StaticContext.CreatureDataContext.consumeItemData[m_consumeItemDataIndex];
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

        internal void SetPartnerList(Data.Models.CreatureData.ProcreationPartnerData[] partnerList)
        {
            m_partnerListIndex = StaticContext.CreatureDataContext.partnerData.Count;
            StaticContext.CreatureDataContext.partnerData.Add(partnerList);
        }

        internal bool HasPartnerList(out Data.Models.CreatureData.ProcreationPartnerData[] partnerList)
        {
            if (m_partnerListIndex != -1)
            {
                partnerList = StaticContext.CreatureDataContext.partnerData[m_partnerListIndex];
                return true;
            }
            partnerList = null;
            return false;
        }

        internal void SetOffspringList(Data.Models.CreatureData.ProcreationOffspringData[] offspringList)
        {
            m_offspringListIndex = StaticContext.CreatureDataContext.offspringData.Count;
            StaticContext.CreatureDataContext.offspringData.Add(offspringList);
        }

        internal bool HasOffspringList(out Data.Models.CreatureData.ProcreationOffspringData[] offspringList)
        {
            if (m_offspringListIndex != -1)
            {
                offspringList = StaticContext.CreatureDataContext.offspringData[m_offspringListIndex];
                return true;
            }
            offspringList = null;
            return false;
        }

        internal void SetMaxCreaturesPrefabs(string[] prefabNames)
        {
            m_maxCreaturesPrefabsIndex = StaticContext.CreatureDataContext.maxCreaturesPrefabs.Count;
            StaticContext.CreatureDataContext.maxCreaturesPrefabs.Add(prefabNames);
        }

        internal bool HasMaxCreaturesPrefabs(out string[] prefabNames)
        {
            if (m_maxCreaturesPrefabsIndex != -1)
            {
                prefabNames = StaticContext.CreatureDataContext.maxCreaturesPrefabs[m_maxCreaturesPrefabsIndex];
                return true;
            }
            prefabNames = null;
            return false;
        }








    }
}
