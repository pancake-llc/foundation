using System;
using Newtonsoft.Json;

namespace Pancake.BakingSheet.Unity
{
    // ReSharper disable once InconsistentNaming
    public class JsonSheetSOReferenceConverter : JsonConverter<IUnitySheetReference>
    {
        public override IUnitySheetReference ReadJson(
            JsonReader reader,
            Type objectType,
            IUnitySheetReference existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            existingValue ??= (IUnitySheetReference) Activator.CreateInstance(objectType);
            existingValue.Asset = serializer.Deserialize<SheetRowScriptableObject>(reader);
            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, IUnitySheetReference value, JsonSerializer serializer) { serializer.Serialize(writer, value.Asset); }
    }
}