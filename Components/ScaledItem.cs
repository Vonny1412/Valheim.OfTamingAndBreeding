using OfTamingAndBreeding.Components.Core;
using System;
using UnityEngine;


//todo: cleanup



namespace OfTamingAndBreeding.Components
{
    public class ScaledItem : OTABComponent<ScaledItem>
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
