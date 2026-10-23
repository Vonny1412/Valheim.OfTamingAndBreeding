using JetBrains.Annotations;
using System;

namespace OfTamingAndBreeding.Data.Models
{
    [Serializable]
    internal class MiscFile : DataBase<MiscFile>
    {

        public const string DirectoryName = "Misc";

        public PickableData Pickable = null;

        [Serializable]
        [CanBeNull]
        public class PickableData
        {
            public string Item = null;
        }







    }
}
