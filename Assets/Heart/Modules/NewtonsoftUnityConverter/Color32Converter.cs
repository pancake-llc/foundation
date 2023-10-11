using UnityEngine;

namespace Newtonsoft.Json.UnityConverters
{
    /// <summary>
    /// Custom Newtonsoft.Json converter <see cref="JsonConverter"/> for the Unity byte based Color type <see cref="Color32"/>.
    /// </summary>
    public class Color32Converter : PartialConverter<Color32>
    {
        protected override void ReadValue(ref Color32 value, string name, JsonReader reader, JsonSerializer serializer)
        {
            switch (name)
            {
                case nameof(value.r):
                    value.r = reader.ReadAsInt8() ?? 0;
                    break;
                case nameof(value.g):
                    value.g = reader.ReadAsInt8() ?? 0;
                    break;
                case nameof(value.b):
                    value.b = reader.ReadAsInt8() ?? 0;
                    break;
                case nameof(value.a):
                    value.a = reader.ReadAsInt8() ?? 0;
                    break;
            }
        }

        protected override void WriteJsonProperties(JsonWriter writer, Color32 value, JsonSerializer serializer)
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