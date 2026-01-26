using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OfTamingAndBreeding.Internals.Behaviors
{
    internal class ConsumeBehavior
    {

        private static readonly int m_itemMask = LayerMask.GetMask("item");

        public class WeightedFood : Data.Models.SubData.IRandomData
        {
            public float Weight { get; set; }
            public ItemDrop itemDrop { get; set; }
        }

        private static readonly Collider[] colliders = new Collider[128]; // 128 should be enough for now

        public static ItemDrop FindClosestConsumableItem(BaseAI baseAI, float maxRange, Func<ItemDrop.ItemData, bool> canConsume)
        {
            var pos = baseAI.transform.position;

            int count = Physics.OverlapSphereNonAlloc(pos, maxRange, colliders, m_itemMask);
            if (count <= 0)
                return null;

            ItemDrop chosen = null;
            float totalWeight = 0f;

            for (int i = 0; i < count; i++)
            {
                Collider col = colliders[i];
                if (!col)
                    continue;

                var rb = col.attachedRigidbody;
                if (!rb)
                    continue;

                ItemDrop item = rb.GetComponent<ItemDrop>();
                if (!item)
                    continue;

                // Same as vanilla: ZNetView must be valid + item must be consumable
                var nview = item.GetComponent<ZNetView>();
                if (!nview || !nview.IsValid())
                    continue;

                var data = item.m_itemData;
                if (data == null || !canConsume(data))
                    continue;

                //float dist = (item.transform.position - pos).sqrMagnitude;
                float dist = Vector3.Distance(item.transform.position, pos); // this costs more performance but z-distance could be important
                if (dist > maxRange)
                    continue;

                // Higher weight when closer (linear)
                float w = maxRange - dist;
                if (w <= 0f)
                    continue;

                // One-pass weighted selection (roulette/reservoir)
                totalWeight += w;
                if (UnityEngine.Random.value * totalWeight <= w)
                {
                    chosen = item;
                }
            }

            if (chosen != null)
            {
                var havePath = Internals.API.BaseAI.__IAPI_HavePath_Invoker1.Invoke(baseAI, new object[] { chosen.transform.position });
                if (havePath)
                    return chosen;
            }

            return null;
        }






    }
}
