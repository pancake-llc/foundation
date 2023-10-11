using System;
using System.Diagnostics.CodeAnalysis;

namespace Newtonsoft.Json.UnityConverters
{
    /// <summary>
    /// Custom base <c>Newtonsoft.Json.JsonConverter</c> to filter serialized properties.
    /// </summary>
    public abstract class PartialConverter<T> : JsonConverter where T : new()
    {
        protected abstract void ReadValue(ref T value, string name, JsonReader reader, JsonSerializer serializer);

        protected abstract void WriteJsonProperties(JsonWriter writer, T value, JsonSerializer serializer);

        /// <summary>
        /// Determine if the object type is <typeparamref name="T"/>
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns><c>true</c> if this can convert the specified type; otherwise, <c>false</c>.</returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(T) || (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                                               objectType.GenericTypeArguments[0] == typeof(T));
        }

        /// <summary>
        /// Read the specified properties to the object.
        /// </summary>
        /// <returns>The object value.</returns>
        /// <param name="reader">The <c>Newtonsoft.Json.JsonReader</c> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        [return: MaybeNull]
        public override object ReadJson(JsonReader reader, Type objectType, [AllowNull] object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                bool isNullableStruct = objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Nullable<>);

                return isNullableStruct ? null : (object) default(T);
            }

            return InternalReadJson(reader, serializer, existingValue);
        }

        [return: MaybeNull]
        private T InternalReadJson(JsonReader reader, JsonSerializer serializer, [AllowNull] object existingValue)
        {
            if (reader.TokenType != JsonToken.StartObject)
            {
                throw reader.CreateSerializationException($"Failed to read type '{typeof(T).Name}'. Expected object start, got '{reader.TokenType}' <{reader.Value}>");
            }

            reader.Read();

            if (!(existingValue is T value))
            {
                value = new T();
            }

            string previousName = null;

            while (reader.TokenType == JsonToken.PropertyName)
            {
                if (reader.Value is string name)
                {
                    if (name == previousName)
                    {
                        throw reader.CreateSerializationException($"Failed to read type '{typeof(T).Name}'. Possible loop when reading property '{name}'");
                    }

                    previousName = name;
                    ReadValue(ref value, name, reader, serializer);
                }
                else
                {
                    reader.Skip();
                }

                reader.Read();
            }

            return value;
        }

        /// <summary>
        /// Write the specified properties of the object.
        /// </summary>
        /// <param name="writer">The <c>Newtonsoft.Json.JsonWriter</c> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, [AllowNull] object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartObject();

            var typed = (T) value;
            WriteJsonProperties(writer, typed, serializer);

            writer.WriteEndObject();
        }
    }
}