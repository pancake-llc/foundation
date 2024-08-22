using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;


namespace Pancake.BakingSheet.Unity
{
    // ReSharper disable once InconsistentNaming
    public class JsonSheetSOContractResolver : DefaultContractResolver
    {
        public static readonly JsonSheetSOContractResolver Instance = new();

        protected override JsonContract CreateContract(Type objectType)
        {
            if (typeof(ISheetRow).IsAssignableFrom(objectType) || typeof(ISheetRowElem).IsAssignableFrom(objectType))
            {
                return CreateObjectContract(objectType);
            }

            if (typeof(IUnitySheetReference).IsAssignableFrom(objectType))
            {
                var contract = base.CreateContract(objectType);
                contract.Converter = new JsonSheetSOReferenceConverter();
                return contract;
            }

            if (typeof(IUnitySheetAssetPath).IsAssignableFrom(objectType))
            {
                var contract = base.CreateContract(objectType);
                contract.Converter = new JsonSheetSOAssetPathConverter();
                return contract;
            }

            if (objectType.IsEnum || Nullable.GetUnderlyingType(objectType)?.IsEnum == true)
            {
                var contract = base.CreateContract(objectType);
                contract.Converter = new StringEnumConverter();
                return contract;
            }

            return base.CreateContract(objectType);
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (member is PropertyInfo pi)
            {
                var nonSerialize = pi.IsDefined(typeof(NonSerializedAttribute));
                var hasSetMethod = pi.SetMethod != null;

                property.Writable = !nonSerialize && hasSetMethod;
                property.ShouldSerialize = property.ShouldDeserialize = _ => !nonSerialize && hasSetMethod;
            }

            return property;
        }

        public static void ErrorHandler(object sender, ErrorEventArgs err)
        {
            if (err.ErrorContext.Member?.ToString() == nameof(ISheetRow.Id) && err.ErrorContext.OriginalObject is ISheetRow && !(err.CurrentObject is ISheet))
            {
                // if id has error, the error must be handled on the sheet level
                return;
            }

            UnityEngine.Debug.LogError(err.ErrorContext.Error.Message);

            err.ErrorContext.Handled = true;
        }
    }
}