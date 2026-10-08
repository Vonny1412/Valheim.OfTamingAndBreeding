using OfTamingAndBreeding.Components.Core;
using UnityEngine;

namespace OfTamingAndBreeding.Components
{
    public class AttachedSprite : OTABComponent<AttachedSprite>
    {
        [SerializeField] public Sprite m_sprite;
        [SerializeField] public float m_size = 1f;
        [SerializeField] public Vector3 m_offset = new Vector3(0f, 0.02f, 0f);

        private GameObject m_visual;
        private Rigidbody m_body;
        private SpriteRenderer m_renderer;

        private void Awake()
        {
            Register();

            m_body = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            if (m_sprite)
            {
                CreateRenderer();
            }
        }

        private void LateUpdate()
        {
            if (!m_visual)
            {
                return;
            }

            bool visible = !m_body || m_body.IsSleeping();
            if (m_renderer.enabled != visible)
            {
                m_renderer.enabled = visible;
            }
            if (!visible)
            {
                return;
            }


            // Follow position, but NOT item rotation
            //m_visual.transform.position = transform.position + m_offset;
            //m_visual.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            // Follow position and Y rotation, but always stay flat
            m_visual.transform.position = transform.position + m_offset;
            var yaw = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
            m_visual.transform.rotation = yaw * flat;
        }
        private readonly Quaternion flat = Quaternion.Euler(90f, 0f, 0f);

        private void OnDestroy()
        {
            if (m_visual)
            {
                Destroy(m_visual);
            }
            Unregister();
        }

        private void CreateRenderer()
        {
            if (!m_sprite)
            {
                return;
            }

            m_visual = new GameObject("OTAB_AttachedSprite");

            m_renderer = m_visual.AddComponent<SpriteRenderer>();
            m_renderer.sprite = m_sprite;

            m_visual.transform.localScale = Vector3.one * m_size;
        }

    }
}