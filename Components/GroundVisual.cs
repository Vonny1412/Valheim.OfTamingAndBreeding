using OfTamingAndBreeding.Components.Core;
using UnityEngine;

namespace OfTamingAndBreeding.Components
{
    public class GroundVisual : OTABComponent<GroundVisual>
    {
        [SerializeField] public Sprite m_sprite;
        [SerializeField] public float m_size = 1f;
        [SerializeField] public Vector3 m_offset = new Vector3(0f, 0.02f, 0f);

        private GameObject m_visual;

        private void Awake()
        {
            Register(this);
        }

        private void Start()
        {
            if (m_sprite)
            {
                CreateVisual();
            }
        }

        private void LateUpdate()
        {
            if (!m_visual)
            {
                return;
            }
            // Follow position, but NOT item rotation
            m_visual.transform.position = transform.position + m_offset;
            m_visual.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }

        private void OnDestroy()
        {
            if (m_visual)
            {
                Destroy(m_visual);
            }
            Unregister(this);
        }

        private void CreateVisual()
        {
            if (!m_sprite)
            {
                return;
            }
            m_visual = new GameObject("OTAB_GroundVisual");
            var renderer = m_visual.AddComponent<SpriteRenderer>();
            renderer.sprite = m_sprite;
            m_visual.transform.localScale = Vector3.one * m_size;
        }

        public void SetSprite(Sprite sprite)
        {
            m_sprite = sprite;
            if (!m_visual)
            {
                CreateVisual();
            }
            else
            {
                m_visual.GetComponent<SpriteRenderer>().sprite = sprite;
            }
        }
    }
}