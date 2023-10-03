using UnityEngine;

namespace Newtonsoft.Json.UnityConverters
{
    /// <summary>
    /// Custom Newtonsoft.Json converter <see cref="JsonConverter"/> for the Unity RectOffset type <see cref="RectOffset"/>.
    /// </summary>
    public class RectOffsetConverter : PartialConverter<RectOffset>
    {
        protected override void ReadValue(ref RectOffset value, string name, JsonReader reader, JsonSerializer serializer)
        {
            switch (name)
            {
                case nameof(value.left):
                    value.left = reader.ReadAsInt32() ?? 0;
                    break;
                case nameof(value.right):
                    value.right = reader.ReadAsInt32() ?? 0;
                    break;
                case nameof(value.top):
                    value.top = reader.ReadAsInt32() ?? 0;
                    break;
                case nameof(value.bottom):
                    value.bottom = reader.ReadAsInt32() ?? 0;
                    break;
            }
        }

        protected override void WriteJsonProperties(JsonWriter writer, RectOffset value, JsonSerializer serializer)
        {
            writer.WritePropertyName(nameof(value.left));
            writer.WriteValue(value.left);
            writer.WritePropertyName(nameof(value.right));
            writer.WriteValue(value.right);
            writer.WritePropertyName(nameof(value.top));
            writer.WriteValue(value.top);
            writer.WritePropertyName(nameof(value.bottom));
            writer.WriteValue(value.bottom);
        }
    }
}