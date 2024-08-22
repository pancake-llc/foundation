using System.IO;
using UnityEngine;

namespace Pancake.BakingSheet.Unity
{
    /// <summary>
    /// AssetPath representing path to Unity's Resource folder.
    /// User can specify sub asset name with square bracket "My/Asset/Path[SubAssetName]".
    /// </summary>
    public sealed class ResourcePath : IUnitySheetAssetPath
    {
        private Object _asset;

        public string RawValue { get; }
        public string FullPath { get; }
        public string SubAssetName { get; }

        string IUnitySheetAssetPath.MetaType => SheetMetaType.ResourcePath;

        public ResourcePath(string rawValue, string basePath = "")
        {
            RawValue = rawValue;

            if (string.IsNullOrEmpty(RawValue)) return;

            var match = DirectAssetPath.PathRegex.Match(RawValue);

            if (!match.Success) return;

            string filePath = match.Groups[1].Value;
            string subAssetName = match.Groups[2].Value;

            FullPath = AssetPath.CombinePath(basePath, filePath, "/");
            SubAssetName = subAssetName;
        }

        public T Load<T>() where T : Object
        {
            if (!this.IsValid()) return null;

            if (_asset != null) return _asset as T;

            _asset = Load(FullPath, SubAssetName);
            return _asset as T;
        }

        internal static Object Load(string fullPath, string subAssetName)
        {
            if (string.IsNullOrEmpty(subAssetName)) return Resources.Load(fullPath);

            var candidates = Resources.LoadAll(fullPath);

            // skip the first asset (main asset)
            for (int i = 1; i < candidates.Length; ++i)
            {
                if (candidates[i].name == subAssetName) return candidates[i];
            }

            return null;
        }
    }
}