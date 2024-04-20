using System.IO;
using Pancake.BakingSheet.Internal;

namespace Pancake.BakingSheet
{
    /// <summary>
    /// Generic ISheetAssetPath implementation.
    /// </summary>
    public class AssetPath : ISheetAssetPath
    {
        public string RawValue { get; }
        public string FullPath { get; }

        [Preserve]
        public AssetPath(string rawValue, string basePath = "", string extension = "")
        {
            RawValue = rawValue;

            if (string.IsNullOrEmpty(RawValue)) return;

            string filePath = RawValue;

            if (!string.IsNullOrEmpty(extension)) filePath = $"{filePath}.{extension}";

            FullPath = Path.Combine(basePath, filePath);
        }
    }

    public static class AssetPathExtensions
    {
        public static bool IsValid(this ISheetAssetPath assetPath) { return !string.IsNullOrEmpty(assetPath?.RawValue); }
    }
}