using MessagePack;
using MessagePack.Formatters;
using UnityEngine;

namespace Pancake
{
    public class AssetFormatter<T> : IMessagePackFormatter<T> where T : UnityEngine.Object
    {
        public void Serialize(ref MessagePackWriter writer, T value, MessagePackSerializerOptions options)
        {
            Archive.Container.TryResolveId(value, out string id);
            Debug.Log(id);
            writer.Write(id);
        }

        public T Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            Archive.Container.TryResolveReference(reader.ReadString(), out object value);
            return (T)value;
        }
    }
}
