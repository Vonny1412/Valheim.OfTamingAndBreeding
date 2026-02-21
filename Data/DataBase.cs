using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace OfTamingAndBreeding.Data
{


    internal abstract class DataBase
    {
        protected static IDeserializer deserializer;
        protected static ISerializer serializer;
        static DataBase()
        {
            deserializer = new DeserializerBuilder().WithCaseInsensitivePropertyMatching().Build();
            serializer = new SerializerBuilder().Build();
        }
        public static string Serialize<T>(T data)
            => serializer.Serialize(data);
        public static T Deserialize<T>(string yaml)
            => deserializer.Deserialize<T>(yaml);

    }

    internal abstract class DataBase<T> : DataBase
        where T : DataBase<T>
    {

        public static T Deserialize(string yaml)
            => deserializer.Deserialize<T>(yaml);

        public static string Serialize(T model)
            => serializer.Serialize(model);

        public string Serialize()
            => serializer.Serialize(this);

        private static readonly Dictionary<string, T> prefabsList
            = new Dictionary<string, T>();

        public static void Store(string prefabName, T data)
            => prefabsList[prefabName] = data;
        
        public static bool Exists(string prefabName)
        {
            prefabName = Utils.GetPrefabName(prefabName);
            return prefabsList.ContainsKey(prefabName);
        }

        public static bool TryGet(string prefabName, out T data)
        {
            prefabName = Utils.GetPrefabName(prefabName);
            return prefabsList.TryGetValue(prefabName, out data);
        }

        public static Dictionary<string, T> GetAll()
            => prefabsList;

        public static T Drop(string prefabName)
        {
            prefabName = global::Utils.GetPrefabName(prefabName);
            if (prefabsList.TryGetValue(prefabName, out T data))
            {
                prefabsList.Remove(prefabName);
                return data;
            }
            return null;
        }

        public static void DropAll()
        {
            prefabsList.Clear();
        }

    }
}
