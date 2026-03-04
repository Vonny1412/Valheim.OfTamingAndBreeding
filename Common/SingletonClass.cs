using System;

namespace OfTamingAndBreeding.Common
{
    internal abstract class SingletonClass<T> where T : SingletonClass<T>, new()
    {

        private static T _instance;
        public static T Instance => _instance;

        public static void CreateInstance()
        {
            DestroyInstance();
            _instance = new T();
            _instance.OnCreate();
        }

        public static void DestroyInstance()
        {
            _instance?.OnDestroy();
            _instance = null;
        }

        protected abstract void OnCreate();

        protected abstract void OnDestroy();

    }
}
