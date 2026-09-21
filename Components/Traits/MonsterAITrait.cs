using OfTamingAndBreeding.Components.Core;
using System;

// warning: MonsterAITrait is currently not registered in typeregistry because its currently not used
//todo: remove me


namespace OfTamingAndBreeding.Components.Traits
{
    public class MonsterAITrait : OTABComponent<MonsterAITrait>
    {

        // set in awake
        [NonSerialized] private ZNetView m_nview = null;
        [NonSerialized] private MonsterAI m_monsterAI = null;
        private void Awake()
        {
            m_nview = GetComponent<ZNetView>();
            m_monsterAI = GetComponent<MonsterAI>();

            Register(this);
        }

        private void OnDestroy()
        {
            Unregister(this);
        }

        /*
        // handled in patch
        public ItemDrop FindConsumeableItem()
        {
            return m_baseAITrait.FindClosestConsumableItem(m_monsterAI.m_consumeSearchRange, m_monsterAI.m_consumeItems);
        }
        */

    }
}
