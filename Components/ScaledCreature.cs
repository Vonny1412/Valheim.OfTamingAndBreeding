using OfTamingAndBreeding.Components.Core;
using UnityEngine;

namespace OfTamingAndBreeding.Components
{

    public class ScaledCreature : OTABComponent<ScaledCreature>
    {
        [SerializeField] public float m_scale = 1f;
        [SerializeField] public float m_animationScale = 1f;
        [SerializeField] public float m_attackScale = 1f;

        private CapsuleCollider m_collider;
        private Transform m_visual;

        private void Awake()
        {
            var character = GetComponent<Character>();
            m_collider = GetComponent<CapsuleCollider>();
            m_visual = character.GetVisual().transform;

            Register();
        }

        private void OnDestroy()
        {
            Unregister();
        }

        public Vector3 GetTopPoint()
        {
            return transform.TransformPoint(m_collider.center) + m_visual.up * (m_collider.height * 0.5f * m_scale);
        }


        
    }
    
}
