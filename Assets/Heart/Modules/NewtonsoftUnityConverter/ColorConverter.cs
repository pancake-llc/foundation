using UnityEngine;

namespace Newtonsoft.Json.UnityConverters
{
    /// <summary>
    /// Custom Newtonsoft.Json converter <see cref="JsonConverter"/> for the Unity Color type <see cref="Color"/>.
    /// </summary>
    public class ColorConverter : PartialConverter<Color>
    {
        protected override void ReadValue(ref Color value, string name, JsonReader reader, JsonSerializer serializer)
        {
            switch (name)
            {
                case nameof(value.r):
                    value.r = reader.ReadAsFloat() ?? 0f;
                    break;
                case nameof(value.g):
                    value.g = reader.ReadAsFloat() ?? 0f;
                    break;
                case nameof(value.b):
                    value.b = reader.ReadAsFloat() ?? 0f;
                    break;
                case nameof(value.a):
                    value.a = reader.ReadAsFloat() ?? 0f;
                    break;
            }
        }

        protected override void WriteJsonProperties(JsonWriter writer, Color value, JsonSerializer serializer)
        {
            writer.WritePropertyName(nameof(value.r));
            writer.WriteValue(value.r);
            writer.WritePropertyName(nameof(value.g));
            writer.WriteValue(value.g);
            writer.WritePropertyName(nameof(value.b));
            writer.WriteValue(value.b);
            writer.WritePropertyName(nameof(value.a));
            writer.WriteValue(value.a);
        }
    }
}