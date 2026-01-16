using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OfTamingAndBreeding.Helpers
{
    internal static class ZNetHelper
    {

        // ZNetHelper
        // Centralized helper for safely accessing ZNetView/ZDO.
        // Always checks:
        //   - object existence
        //   - ZNetView validity
        //   - ZDO availability
        //
        // Use this instead of GetComponent<ZNetView>() + GetZDO()
        // to avoid rare null / invalid state crashes.

        public static bool TryGetZDO(GameObject obj, out ZDO zdo, out ZNetView nview)
        {
            zdo = null;
            nview = null;

            if (!obj) return false;

            var _nview = obj.GetComponent<ZNetView>();
            if (!_nview || !_nview.IsValid()) return false;
            nview = _nview;

            var _zdo = nview.GetZDO();
            if (_zdo == null) return false;
            zdo = _zdo;

            return true;
        }

        public static bool TryGetZDO(GameObject obj, out ZDO zdo)
            => TryGetZDO(obj, out zdo, out ZNetView _);

        public static bool TryGetZDO(Component c, out ZDO zdo, out ZNetView nview)
            => TryGetZDO(c ? c.gameObject : null, out zdo, out nview);

        public static bool TryGetZDO(Component c, out ZDO zdo)
            => TryGetZDO(c ? c.gameObject : null, out zdo, out ZNetView _);


    }
}
