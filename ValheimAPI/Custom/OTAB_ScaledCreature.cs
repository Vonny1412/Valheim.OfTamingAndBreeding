using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using UnityEngine;

namespace OfTamingAndBreeding.ValheimAPI.Custom
{
    public sealed class OTAB_ScaledCreature : MonoBehaviour
    {
        [SerializeField] internal float m_customEffectScale = 1f;
        [SerializeField] internal float m_customAnimationScale = 1f;
    }
}
