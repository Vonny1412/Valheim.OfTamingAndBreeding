using System;

namespace OfTamingAndBreeding.Data.Models.SubData
{
    [Serializable]
    internal enum IsEnemyCondition
    {
        Never = 0,
        Always = 1,
        WhenStarving = 2,
    }
}
