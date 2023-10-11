using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.ComponentHeader
{
    internal static class TextureManager
    {
        private static readonly Dictionary<ButtonType, string> GuidDictionary = new()
        {
            {ButtonType.Remove, "81ba6b20930dc4a4ead95f969e255970"},
            {ButtonType.MoveUp, "76ede05262f8bc643aae0c54085298b1"},
            {ButtonType.MoveDown, "661ee6cb51d809c4abb4f09727ae9c26"},
            {ButtonType.CopyComponent, "0eac7ff23df01a741b7f715e6e2871b3"},
            {ButtonType.PasteComponentValue, "c722d036a010ab94b8a0188f6339063b"},
        };

        private static readonly Dictionary<ButtonType, Texture2D> TextureDictionary = new();

        public static Texture2D Get(ButtonType buttonType)
        {
            if (TextureDictionary.TryGetValue(buttonType, out var result)) return result;

            var guid = GuidDictionary[buttonType];
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);

            TextureDictionary[buttonType] = texture;

            return texture;
        }
    }
}