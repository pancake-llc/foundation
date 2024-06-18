using System.IO;
using UnityEngine;

#if BAKINGSHEET_ADDRESSABLES
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
#endif

namespace Pancake.BakingSheet.Unity
{
    /// <summary>
    /// AssetPath representing path to Unity's Addressable Assets.
    /// User can specify sub asset name with square bracket "My/Asset/Path.png[SubAssetName]".
    /// </summary>
    public sealed class AddressablePath : IUnitySheetAssetPath
    {
#if BAKINGSHEET_ADDRESSABLES
        private AsyncOperationHandle _handle;
#endif

        public string RawValue { get; }
        public string FullPath { get; }
        public string SubAssetName { get; }

        public string MetaType => SheetMetaType.AddressablePath;

        public AddressablePath(string rawValue, string basePath = "", string extension = "")
        {
            RawValue = rawValue;

            if (string.IsNullOrEmpty(RawValue)) return;

            var match = DirectAssetPath.PathRegex.Match(RawValue);

            if (!match.Success) return;

            string filePath = match.Groups[1].Value;
            string subAssetName = match.Groups[2].Value;

            if (!string.IsNullOrEmpty(extension)) filePath = $"{filePath}.{extension}";

            FullPath = AssetPath.CombinePath(basePath, filePath, "/");
            SubAssetName = subAssetName;

            if (!string.IsNullOrEmpty(SubAssetName)) FullPath += $"[{SubAssetName}]";
        }

#if BAKINGSHEET_ADDRESSABLES
        public AsyncOperationHandle<T> LoadAsync<T>() where T : Object
        {
            if (!this.IsValid()) return default;

            if (!_handle.IsValid())
            {
                var handle = Addressables.LoadAssetAsync<T>(FullPath);
                _handle = handle;
                return handle;
            }

            return _handle.Convert<T>();
        }

        public T Get<T>() where T : Object
        {
            if (!_handle.IsValid()) return null;

            return _handle.Result as T;
        }

        public void Release()
        {
            if (!_handle.IsValid()) return;

            Addressables.Release(_handle);
            _handle = default;
        }
#endif
    }
}