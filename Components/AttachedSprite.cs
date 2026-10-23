using OfTamingAndBreeding.Components.Core;
using OfTamingAndBreeding.ValheimAPI;
using System;
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

        private readonly Quaternion flat = Quaternion.Euler(90f, 0f, 0f);

        private bool m_preserveVisual;
        private float m_preserveVisualUntil;

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

            // Ignore temporary physics reactions
            if (Time.time < m_preserveVisualUntil)
            {
                return;
            }

            bool sleeping = !m_body || m_body.IsSleeping();

            if (!sleeping)
            {
                m_preserveVisual = false;
            }

            if (m_renderer.enabled != sleeping)
            {
                m_renderer.enabled = sleeping;
            }

            if (!sleeping || m_preserveVisual)
            {
                return;
            }

            var yaw = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
            m_visual.transform.position = transform.position + yaw * m_offset;
            m_visual.transform.rotation = yaw * flat;
        }

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

        public void PreserveVisual(float time)
        {
            m_preserveVisual = true;
            m_preserveVisualUntil = Mathf.Max(m_preserveVisualUntil, Time.time + time);
        }

    }
}