using OfTamingAndBreeding.Components.Base;
using System;
using UnityEngine;

namespace OfTamingAndBreeding.Components
{
    public class ScaledEgg : OTABComponent<ScaledEgg>
    {
        [SerializeField] public float m_scale = 1;

        private void Awake()
        {
            var itemDrop = GetComponent<ItemDrop>();
            if (itemDrop)
            {
                itemDrop.transform.localScale *= m_scale;
            }

            Register(this);
        }

        private void OnDestroy()
        {
            Unregister(this);
        }

    }
}
