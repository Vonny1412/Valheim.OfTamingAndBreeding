using System.Collections.Generic;

namespace OfTamingAndBreeding.Common
{
    internal class IndexedDataStore<T>
    {
        private readonly List<T> m_items = new List<T>();

        public IndexedDataStore()
        {
            Network.NetworkSessionManager.OnSessionClosed += () => {
                m_items.Clear();
            };
        }

        public int Add(T item)
        {
            var index = m_items.Count;
            m_items.Add(item);
            return index;
        }

        public bool TryGet(int index, out T item)
        {
            if (index >= 0 && index < m_items.Count)
            {
                item = m_items[index];
                return true;
            }
            item = default;
            return false;
        }

    }
}
