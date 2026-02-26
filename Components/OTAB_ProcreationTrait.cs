using UnityEngine;

namespace OfTamingAndBreeding.Components
{
    public sealed class OTAB_ProcreationTrait : MonoBehaviour
    {
        [SerializeField] internal float m_basePregnancyDuration = 60;
        [SerializeField] internal float m_realPregnancyDuration = 0;
    }
}
