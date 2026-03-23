using OfTamingAndBreeding.Components.Base;
using System;
using UnityEngine;

namespace OfTamingAndBreeding.Components
{

    public class ScaledCreature : OTABComponent<ScaledCreature>
    {
        [SerializeField] public float m_effectScale = 1f;
        [SerializeField] public float m_animationScale = 1f;

        private void Awake()
        {
            Register(this);
        }

        private void OnDestroy()
        {
            Unregister(this);
        }

    }
    
}
