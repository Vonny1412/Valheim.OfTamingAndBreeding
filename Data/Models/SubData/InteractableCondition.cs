using System;

namespace OfTamingAndBreeding.Data.Models.SubData
{
    [Serializable]
    public enum InteractableCondition
    {
        Never = 0,
        WhenFed = 1,
        WhenNotStarving = 2,
        Always = 3,
    }
}
