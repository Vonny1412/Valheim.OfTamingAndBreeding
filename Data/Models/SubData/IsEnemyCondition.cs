using System;

namespace OfTamingAndBreeding.Data.Models.SubData
{
    [Serializable]
    public enum IsEnemyCondition
    {
        NeverEver = -1,
        Never = 0,
        WhenFed = 1,
        WhenHungry = 2,
        WhenStarving = 4,
        Always = 7,
    }
}
