using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace OfTamingAndBreeding.Data
{
    internal abstract class SerializeableData
    {
        protected static IDeserializer deserializer;
        protected static ISerializer serializer;

        static SerializeableData()
        {
            deserializer = new DeserializerBuilder().WithCaseInsensitivePropertyMatching().Build();
            serializer = new SerializerBuilder().Build();
        }

        internal static string Serialize<T>(T data)
            => serializer.Serialize(data);

        internal static T Deserialize<T>(string yaml)
            => deserializer.Deserialize<T>(yaml);

    }
}
