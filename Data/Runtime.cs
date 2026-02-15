using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Data
{
    internal static partial class Runtime
    {
        // note: this class is for faster data lookup
        // instead of `Data.DataBase<...>.Get(prefabName).componentName.fieldName`
        // the data can be accessed directly using cached values
        // the values are added to the list when prefabs getting registered (check: Data/Handling/...Handler.cs)

        public static void Reset()
        {
            Character.Reset();
            Tameable.Reset();
            MonsterAI.Reset();
            ItemData.Reset();
            EggGrow.Reset();
            Procreation.Reset();
            Growup.Reset();
        }

    }
}
