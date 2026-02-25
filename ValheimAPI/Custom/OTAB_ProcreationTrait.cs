using UnityEngine;

namespace OfTamingAndBreeding.ValheimAPI.Custom
{
    public sealed class OTAB_ProcreationTrait : MonoBehaviour
    {
        [SerializeField] internal float m_basePregnancyDuration = 60;
        [SerializeField] internal float m_realPregnancyDuration = 0;
    }
}
