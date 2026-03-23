using OfTamingAndBreeding.Components.Base;
using System;

namespace OfTamingAndBreeding.Components.Traits
{
    public class MonsterAITrait : OTABComponent<MonsterAITrait>
    {

        // set in awake
        [NonSerialized] private ZNetView m_nview = null;
        [NonSerialized] private MonsterAI m_monsterAI = null;
        [NonSerialized] private BaseAITrait m_baseAITrait = null;

        private void Awake()
        {
            m_nview = GetComponent<ZNetView>();
            m_monsterAI = GetComponent<MonsterAI>();
            m_baseAITrait = GetComponent<BaseAITrait>();

            Register(this);
        }

        private void OnDestroy()
        {
            Unregister(this);
        }

        public ItemDrop FindConsumeableItem()
        {
            if (Plugin.Configs.UseBetterSearchForFood.Value == true)
            {
                return m_baseAITrait.FindNearbyConsumableItem(m_monsterAI.m_consumeSearchRange, m_monsterAI.m_consumeItems);
            }
            return m_baseAITrait.FindClosestConsumableItem(m_monsterAI.m_consumeSearchRange, m_monsterAI.m_consumeItems);
        }

    }
}
