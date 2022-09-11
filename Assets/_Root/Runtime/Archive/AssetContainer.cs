using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using System.Linq;
#endif

namespace Pancake
{
    internal sealed class AssetContainer : ScriptableObject
    {
        [SerializeField] private AssetEntry[] savedAssets = System.Array.Empty<AssetEntry>();
        [SerializeField] private string[] paths;

        public bool TryResolveId(object value, out string id)
        {
            id = null;

            if (value is not Object obj || !TryGetValue(obj, out var entry)) return false;

            id = entry.Guid;
            return true;
        }

        public bool TryResolveReference(string id, out object value)
        {
            value = null;

            if (id == null) return false;

            var contains = TryGetValue(id, out var entry);
            value = entry.Asset;

            return contains;
        }

#if UNITY_EDITOR
        public void LoadAssets()
        {
            if (paths == null) return;

            paths = paths.Where(x => !string.IsNullOrEmpty(x) && UnityEditor.AssetDatabase.IsValidFolder(x)).ToArray();

            if (paths.Length == 0) return;

            var assets = UnityEditor.AssetDatabase.FindAssets("t:Object", paths)
                .Select(UnityEditor.AssetDatabase.GUIDToAssetPath)
                .Select(UnityEditor.AssetDatabase.LoadAssetAtPath<Object>)
                .Where(x =>
                {
                    var fileNamespace = x.GetType().Namespace;

                    return x != null && (fileNamespace == null || !fileNamespace.Contains("UnityEditor"));
                })
                .ToList();

            var newEntries = new List<AssetEntry>();

            foreach (var asset in assets)
            {
                var path = UnityEditor.AssetDatabase.GetAssetPath(asset);
                var guid = UnityEditor.AssetDatabase.AssetPathToGUID(path);

                if (!TryGetValue(asset, out _))
                    newEntries.Add(new AssetEntry(guid, asset));

                var childAssets = UnityEditor.AssetDatabase.LoadAllAssetRepresentationsAtPath(UnityEditor.AssetDatabase.GetAssetPath(asset));

                foreach (var child in childAssets)
                {
                    if (TryGetValue(child, out _))
                        continue;

                    var childGuid = System.Guid.NewGuid().ToString();
                    newEntries.Add(new AssetEntry(childGuid, child));
                }
            }

            UnityEditor.ArrayUtility.AddRange(ref savedAssets, newEntries.ToArray());
            UnityEditor.EditorUtility.SetDirty(this);
        }

        public void Clear()
        {
            savedAssets = System.Array.Empty<AssetEntry>();
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif

        private bool TryGetValue(string guid, out AssetEntry entry)
        {
            foreach (var asset in savedAssets)
            {
                if (asset.Guid != guid) continue;

                entry = asset;
                return true;
            }

            entry = null;
            return false;
        }

        private bool TryGetValue(Object obj, out AssetEntry entry)
        {
            foreach (var asset in savedAssets)
            {
                if (asset.Asset != obj) continue;

                entry = asset;
                return true;
            }

            entry = null;
            return false;
        }
    }
}