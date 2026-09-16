using System;

namespace OfTamingAndBreeding.Data.Files.SubData
{
    // todo: put me somewhere else
    [Serializable]
    public enum IsEnemyCondition
    {
        Default = 0,
        Never = 1,
        Force = 2,
        WhenFed = 3,
        WhenHungry = 4,
    }
}
