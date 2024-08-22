using System.IO;
using System.Text.RegularExpressions;

namespace Pancake.BakingSheet.Unity
{
    public interface IUnitySheetDirectAssetPath : IUnitySheetAssetPath
    {
        UnityEngine.Object Asset { get; set; }
    }

    /// <summary>
    /// Direct asset reference to Unity's Assets folder.
    /// Note that this is only supported when you are using ScriptableObject exporter in Unity.
    /// User can specify sub asset name with square bracket "My/Asset/Path.png[SubAssetName]".
    /// </summary>
    public sealed class DirectAssetPath : IUnitySheetDirectAssetPath
    {
        internal static readonly Regex PathRegex = new Regex(@"^([^\[\]]+)(?:\[([^\[\]]+)\])?$");

        private UnityEngine.Object _asset;

        public string RawValue { get; }
        public string FullPath { get; }
        public string SubAssetName { get; }

        string IUnitySheetAssetPath.MetaType => SheetMetaType.DirectAssetPath;
        UnityEngine.Object IUnitySheetDirectAssetPath.Asset { get => _asset; set => _asset = value; }

        public DirectAssetPath(string rawValue, string basePath = "Assets", string extension = "")
        {
            RawValue = rawValue;

            if (string.IsNullOrEmpty(RawValue)) return;

            var match = PathRegex.Match(RawValue);

            if (!match.Success) return;

            string filePath = match.Groups[1].Value;
            string subAssetName = match.Groups[2].Value;

            if (!string.IsNullOrEmpty(extension)) filePath = $"{filePath}.{extension}";

            FullPath = AssetPath.CombinePath(basePath, filePath, "/");
            SubAssetName = subAssetName;
        }

        public T Get<T>() where T : UnityEngine.Object { return _asset as T; }
    }
}