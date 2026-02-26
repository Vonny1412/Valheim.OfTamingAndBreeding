using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Data
{
    internal abstract class DataBase<T> : SerializeableData where T : DataBase<T>
    {

        public static T Deserialize(string yaml)
            => deserializer.Deserialize<T>(yaml);

        public static string Serialize(T model)
            => serializer.Serialize(model);

        public string Serialize()
            => serializer.Serialize(this);

        private static readonly Dictionary<string, T> dataHolder
            = new Dictionary<string, T>();

        public static void Store(string prefabName, T data)
            => dataHolder[prefabName] = data;
        
        public static Dictionary<string, T> GetAll()
            => dataHolder;

        public static T Drop(string prefabName)
        {
            prefabName = global::Utils.GetPrefabName(prefabName);
            if (dataHolder.TryGetValue(prefabName, out T data))
            {
                dataHolder.Remove(prefabName);
                return data;
            }
            return null;
        }

        public static void DropAll()
        {
            dataHolder.Clear();
        }

    }
}
