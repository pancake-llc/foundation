using UnityEngine;

namespace Newtonsoft.Json.UnityConverters
{
    /// <summary>
    /// Custom Newtonsoft.Json converter <see cref="JsonConverter"/> for the Unity Bounds type <see cref="Bounds"/>.
    /// </summary>
    public class BoundsConverter : PartialConverter<Bounds>
    {
        protected override void ReadValue(ref Bounds value, string name, JsonReader reader, JsonSerializer serializer)
        {
            switch (name)
            {
                case nameof(value.center):
                    value.center = reader.ReadViaSerializer<Vector3>(serializer);
                    break;
                case nameof(value.size):
                    value.size = reader.ReadViaSerializer<Vector3>(serializer);
                    break;
            }
        }

        protected override void WriteJsonProperties(JsonWriter writer, Bounds value, JsonSerializer serializer)
        {
            writer.WritePropertyName(nameof(value.center));
            serializer.Serialize(writer, value.center, typeof(Vector3));
            writer.WritePropertyName(nameof(value.size));
            serializer.Serialize(writer, value.size, typeof(Vector3));
        }
    }
}