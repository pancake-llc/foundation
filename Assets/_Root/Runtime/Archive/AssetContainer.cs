using System;
using System.Collections.Generic;
using Pancake.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using System.Linq;
#endif

namespace Pancake
{
    internal sealed class AssetContainer : ScriptableObject
    {
        [SerializeField] private string[] assetGuids = System.Array.Empty<string>();

        internal string[] AssetGuids { get => assetGuids; set => assetGuids = value; }

        public bool TryResolveId(object value, out string id)
        {
            id = null;

            //if (value is not Object obj || !TryGetValue(obj, out var entry)) return false;

            //id = entry.Guid;
            return true;
        }

        public bool TryResolveReference(string id, out object value)
        {
            value = null;

            if (id == null) return false;

            //var contains = TryGetValue(id, out var entry);
            // value = entry.Asset;
            //
            // return contains;
            return false;
        }

// #if UNITY_EDITOR
//         public void LoadAssets()
//         {
//             if (paths == null) return;
//
//             paths = paths.Where(x => !string.IsNullOrEmpty(x) && UnityEditor.AssetDatabase.IsValidFolder(x)).ToArray();
//
//             if (paths.Length == 0) return;
//
//             var assets = UnityEditor.AssetDatabase.FindAssets("t:Object", paths)
//                 .Select(UnityEditor.AssetDatabase.GUIDToAssetPath)
//                 .Select(UnityEditor.AssetDatabase.LoadAssetAtPath<Object>)
//                 .Where(x =>
//                 {
//                     var fileNamespace = x.GetType().Namespace;
//
//                     return x != null && (fileNamespace == null || !fileNamespace.Contains("UnityEditor"));
//                 })
//                 .ToList();
//
//             var newEntries = new List<AssetEntry>();
//
//             foreach (var asset in assets)
//             {
//                 var path = UnityEditor.AssetDatabase.GetAssetPath(asset);
//                 var guid = UnityEditor.AssetDatabase.AssetPathToGUID(path);
//
//                 if (!TryGetValue(asset, out _)) newEntries.Add(new AssetEntry(guid));
//
//                 var childAssets = UnityEditor.AssetDatabase.LoadAllAssetRepresentationsAtPath(UnityEditor.AssetDatabase.GetAssetPath(asset));
//
//                 foreach (var child in childAssets)
//                 {
//                     if (TryGetValue(child, out _))
//                         continue;
//
//                     var childGuid = System.Guid.NewGuid().ToString();
//                     newEntries.Add(new AssetEntry(childGuid));
//                 }
//             }
//
//             UnityEditor.ArrayUtility.AddRange(ref savedAssets, newEntries.ToArray());
//             UnityEditor.EditorUtility.SetDirty(this);
//         }
//
//         public void Clear()
//         {
//             savedAssets = System.Array.Empty<AssetEntry>();
//             UnityEditor.EditorUtility.SetDirty(this);
//         }
// #endif

        private async UniTask<(bool, Object)> TryGetValue(string guid)
        {
            try
            {
                var obj = await Addressables.LoadAssetAsync<Object>(guid);
                if (obj != null) return (true, obj);

                return (false, null);
            }
            catch (Exception)
            {
                return (false, null);
            }
        }

        private bool TryGetValue(Object obj)
        {
            // foreach (var asset in savedAssets)
            // {
            //     if (asset.Asset != obj) continue;
            //
            //     entry = asset;
            //     return true;
            // }
            //
            // entry = null;
            return false;
        }


        public async void LocationMap()
        {
            Addressables.InitializeAsync().WaitForCompletion(); // populate ResourceLocators
            var locToKeys = new Dictionary<string, List<object>>();
            foreach (IResourceLocator locator in Addressables.ResourceLocators)
            {
                ResourceLocationMap map = locator as ResourceLocationMap;
                if (map == null)
                    continue;
                foreach (KeyValuePair<object, IList<IResourceLocation>> keyToLocs in map.Locations)
                {
                    foreach (IResourceLocation loc in keyToLocs.Value)
                    {
                        if (!locToKeys.ContainsKey(loc.InternalId)) locToKeys.Add(loc.InternalId, new List<object>(){ keyToLocs.Key });
                        else locToKeys[loc.InternalId].Add(keyToLocs.Key);
                    }
                }
            }
        }
        
    }
}