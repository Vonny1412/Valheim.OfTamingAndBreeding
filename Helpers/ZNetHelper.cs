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
        //
        // IMPORTANT: no need to check for m_nview.isValid()
        // because if nview is not valid this method will also return false since zdo relies on nview
        public static bool TryGetZDO(GameObject obj, out ZDO zdo, out ZNetView nview)
        {
            zdo = null;
            nview = null;

            if (!obj) return false;

            var _nview = obj.GetComponent<ZNetView>();
            if (!_nview) return false;
            nview = _nview;

            var _zdo = nview.GetZDO();
            if (_zdo == null) return false;
            if (!_zdo.IsValid())
            {
                return false;
            }
            zdo = _zdo;
            return true;
        }

        public static bool TryGetZDO(GameObject obj, out ZDO zdo)
            => TryGetZDO(obj, out zdo, out ZNetView _);

        public static bool TryGetZDO(Component c, out ZDO zdo, out ZNetView nview)
            => TryGetZDO(c ? c.gameObject : null, out zdo, out nview);

        public static bool TryGetZDO(Component c, out ZDO zdo)
            => TryGetZDO(c ? c.gameObject : null, out zdo, out ZNetView _);



        // about unsafe overloads:
        // Use only if 'cur' is a cached snapshot of the same ZDO key from earlier in this tick.
        // Avoids an extra zdo.GetX() call.

        public static int SetInt(ZDO zdo, int key, int value)
        {
            if (zdo.GetInt(key, int.MinValue) != value) zdo.Set(key, value);
            return value;
        }

        public static int SetInt(ZDO zdo, int key, int value, int cur) // unsafe! use with care
        {
            if (cur != value) zdo.Set(key, value);
            return value;
        }

        public static long SetLong(ZDO zdo, int key, long value)
        {
            if (zdo.GetLong(key, long.MinValue) != value) zdo.Set(key, value);
            return value;
        }

        public static long SetLong(ZDO zdo, int key, long value, long cur) // unsafe! use with care
        {
            if (cur != value) zdo.Set(key, value);
            return value;
        }

        public static string SetString(ZDO zdo, int key, string value)
        {
            if (zdo.GetString(key, null) != value) zdo.Set(key, value);
            return value;
        }

        public static string SetString(ZDO zdo, int key, string value, string cur) // unsafe! use with care
        {
            if (cur != value) zdo.Set(key, value);
            return value;
        }

        public static float SetFloat(ZDO zdo, int key, float value)
        {
            if (zdo.GetFloat(key, 0f) != value) zdo.Set(key, value);
            return value;
        }

        public static float SetFloat(ZDO zdo, int key, float value, float cur) // unsafe! use with care
        {
            if (cur != value) zdo.Set(key, value);
            return value;
        }

    }
}
