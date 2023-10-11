using UnityEngine;

namespace Newtonsoft.Json.UnityConverters
{
    /// <summary>
    /// Custom Newtonsoft.Json converter <see cref="JsonConverter"/> for the Unity Rect type <see cref="Rect"/>.
    /// </summary>
    public class RectConverter : PartialConverter<Rect>
    {
        protected override void ReadValue(ref Rect value, string name, JsonReader reader, JsonSerializer serializer)
        {
            switch (name)
            {
                case nameof(value.x):
                    value.x = reader.ReadAsFloat() ?? 0;
                    break;
                case nameof(value.y):
                    value.y = reader.ReadAsFloat() ?? 0;
                    break;
                case nameof(value.width):
                    value.width = reader.ReadAsFloat() ?? 0;
                    break;
                case nameof(value.height):
                    value.height = reader.ReadAsFloat() ?? 0;
                    break;
            }
        }

        protected override void WriteJsonProperties(JsonWriter writer, Rect value, JsonSerializer serializer)
        {
            writer.WritePropertyName(nameof(value.x));
            writer.WriteValue(value.x);
            writer.WritePropertyName(nameof(value.y));
            writer.WriteValue(value.y);
            writer.WritePropertyName(nameof(value.width));
            writer.WriteValue(value.width);
            writer.WritePropertyName(nameof(value.height));
            writer.WriteValue(value.height);
        }
    }
}