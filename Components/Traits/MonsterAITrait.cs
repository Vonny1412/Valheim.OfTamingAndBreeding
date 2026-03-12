using OfTamingAndBreeding.Components.Base;
using System;

namespace OfTamingAndBreeding.Components.Traits
{
    public class MonsterAITrait : OTABComponent<MonsterAITrait>
    {

        // set in awake
        [NonSerialized] private ZNetView m_nview = null;
        [NonSerialized] private MonsterAI m_monsterAI = null;
        [NonSerialized] private TameableTrait m_tameableTrait = null;
        [NonSerialized] private Humanoid m_humanoid = null;

        private void Awake()
        {
            m_nview = GetComponent<ZNetView>();
            m_monsterAI = GetComponent<MonsterAI>();
            m_tameableTrait = GetComponent<TameableTrait>();
            m_humanoid = GetComponent<Humanoid>();
        }

        // todo: is this component still neccessary ???
        // keep it for now but remove it if realy unneccessary

    }
}
