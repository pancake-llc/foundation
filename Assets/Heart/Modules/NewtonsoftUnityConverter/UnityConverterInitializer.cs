using Newtonsoft.Json.Converters;
using UnityEngine;

namespace Newtonsoft.Json.UnityConverters
{
    public static class UnityConverterInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#endif
        internal static void Init()
        {
            if (JsonConvert.DefaultSettings == null) JsonConvert.DefaultSettings = CreateJsonSettingsWithFreslyLoadedConfig;
        }

        private static JsonSerializerSettings CreateJsonSettingsWithFreslyLoadedConfig()
        {
            var settings = new JsonSerializerSettings
            {
                Converters = new JsonConverter[]
                {
                    new BoundsConverter(), new BoundsIntConverter(), new RectConverter(), new RectIntConverter(), new RectOffsetConverter(), new Color32Converter(),
                    new ColorConverter(), new Vector2Converter(), new Vector2IntConverter(), new Vector3Converter(), new Vector3IntConverter(), new Vector4Converter(),
                    new QuaternionConverter(), new NativeArrayConverter(), new LayerMaskConverter(), new RangeIntConverter(),
#if PANCAKE_ADDRESSABLE
                    new AssetReferenceConverter(),
#endif
                    new StringEnumConverter(), new VersionConverter()
                },
                ContractResolver = new UnityTypeContractResolver()
            };

            return settings;
        }
    }
}