using OfTamingAndBreeding.Components.Core;
using UnityEngine;

namespace OfTamingAndBreeding.Components.Traits
{
    public partial class BaseAITrait : OTABComponent<BaseAITrait>
    {

        private bool m_frustrated = false;
        private float m_frustratedLastCheck = 0f;
        private GameObject m_frustratedHud;
        private GameObject m_awareHud;

        internal void SetFrustratedHud(GameObject frustratedHud, GameObject awareHud)
        {
            m_frustratedHud = frustratedHud;
            m_awareHud = awareHud;
        }

        private void UpdateFrustratedHud()
        {
            if (!m_frustratedHud)
            {
                return;
            }

            bool flag2 = m_baseAI.IsAlerted();
            if (IsFrustrated() && !flag2)
            {
                m_awareHud.SetActive(false);
                m_frustratedHud.SetActive(true);
            }
            else
            {
                m_awareHud.SetActive(true);
                m_frustratedHud.SetActive(false);
            }
        }

        public bool IsFrustrated()
        {
            var t = Time.realtimeSinceStartup;
            if (t != m_frustratedLastCheck)
            {
                m_frustratedLastCheck = t;
                m_frustrated = m_nview.GetZDO().GetInt(Plugin.ZDOVars.z_frustrated, 0) == 1;
            }
            return m_frustrated;
        }
        
        internal void SetFrustrated(bool frustrated)
        {
            // warning: check for valid nview and owner yourself!
            m_nview.GetZDO().Set(Plugin.ZDOVars.z_frustrated, frustrated ? 1 : 0);
        }

    }
}
