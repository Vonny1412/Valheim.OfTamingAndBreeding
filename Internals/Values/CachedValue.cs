using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Internals.Values
{

    internal class CachedValue<T>
    {
        private bool captured = false;
        private T value;

        public delegate T GetterDelegate();
        public delegate void SetterDelegate(T value);
        public delegate void OnSaveDelegate(CachedValue<T> me);

        private readonly GetterDelegate getter;
        private readonly SetterDelegate setter;
        private readonly OnSaveDelegate onSave = null;

        public CachedValue(GetterDelegate getter, SetterDelegate setter)
        {
            this.getter = getter;
            this.setter = setter;
        }

        public CachedValue(GetterDelegate getter, SetterDelegate setter, OnSaveDelegate onSave)
        {
            this.getter = getter;
            this.setter = setter;
            this.onSave = onSave;
        }

        public T GetValue()
        {
            return captured ? value : getter();
        }

        public T GetCurrentValue() => getter();
        public T GetSavedValue() => value;
        public bool IsCaptured => captured;

        /*
        // method not used: it is unclear what value to set. this internal value? or the original? or both?
        // everythin is been handled by SaveValue()
        public void SetValue(T value)
        {
            setter(value);
        }
        */

        public void SaveValue(bool overwrite = false)
        {
            if (captured && !overwrite) return;
            value = getter();
            captured = true;
            onSave?.Invoke(this);
        }

        public void RestoreValue(bool keepCaptured = false)
        {
            if (!captured) return;
            setter(value);
            if (!keepCaptured)
            {
                captured = false;
                value = default;
            }
        }

    }
}
