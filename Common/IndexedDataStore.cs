using System.Collections.Generic;

// todo: cleanup
// this is only component/trait related

namespace OfTamingAndBreeding.Common
{
    internal class IndexedDataStore<T>
    {
        private readonly List<T> m_items = new List<T>();

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

        public void Clear()
        {
            m_items.Clear();
        }

    }
}
