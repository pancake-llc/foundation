using MessagePack;
using MessagePack.Formatters;

namespace Pancake
{
    public class AssetFormatter<T> : IMessagePackFormatter<T> where T : UnityEngine.Object
    {
        public void Serialize(ref MessagePackWriter writer, T value, MessagePackSerializerOptions options) { throw new System.NotImplementedException(); }

        public T Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) { throw new System.NotImplementedException(); }
    }
}
