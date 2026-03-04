using OfTamingAndBreeding.Components.Base;
using UnityEngine;

namespace OfTamingAndBreeding.Components
{
    public class ScaledCreature : OTABComponent<ScaledCreature>
    {
        [SerializeField] public float m_effectScale = 1f;
        [SerializeField] public float m_animationScale = 1f;
    }
}
