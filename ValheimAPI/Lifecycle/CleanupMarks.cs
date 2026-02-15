using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.ValheimAPI.Lifecycle
{
    static class CleanupMarks
    {
        private static readonly ConditionalWeakTable<ZNetView, object> touched = new ConditionalWeakTable<ZNetView, object>();
        private static readonly object marker = new object();

        public static void Mark(ZNetView nview)
        {
            if (nview != null)
                touched.GetValue(nview, _ => marker);
        }

        public static bool IsMarked(ZNetView nview) =>
            nview != null && touched.TryGetValue(nview, out _);

        public static void Unmark(ZNetView nview)
        {
            if (nview != null)
                touched.Remove(nview);
        }
    }
}
