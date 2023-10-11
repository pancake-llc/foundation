using UnityEngine;

namespace Newtonsoft.Json.UnityConverters
{
    /// <summary>
    /// Custom Newtonsoft.Json converter <see cref="JsonConverter"/> for the Unity integer Bounds type <see cref="BoundsInt"/>.
    /// </summary>
    public class BoundsIntConverter : PartialConverter<BoundsInt>
    {
        protected override void ReadValue(ref BoundsInt value, string name, JsonReader reader, JsonSerializer serializer)
        {
            switch (name)
            {
                case nameof(value.position):
                    value.position = reader.ReadViaSerializer<Vector3Int>(serializer);
                    break;
                case nameof(value.size):
                    value.size = reader.ReadViaSerializer<Vector3Int>(serializer);
                    break;
            }
        }

        protected override void WriteJsonProperties(JsonWriter writer, BoundsInt value, JsonSerializer serializer)
        {
            writer.WritePropertyName(nameof(value.position));
            serializer.Serialize(writer, value.position, typeof(Vector3Int));
            writer.WritePropertyName(nameof(value.size));
            serializer.Serialize(writer, value.size, typeof(Vector3Int));
        }
    }
}