using OfTamingAndBreeding.ValheimAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace OfTamingAndBreeding.API
{
    /*
    OTAB returns no wrapper instances. Call API methods with the Component each time. Internally OTAB uses weak references; holding wrapper instances would be unsafe.
    */

    public class TameableAPI
    {

        public static bool IsStarving(Tameable tameable)
        {
            if (tameable == null)
            {
                return false;
            }
            return tameable.IsStarving();
        }

    }
}
