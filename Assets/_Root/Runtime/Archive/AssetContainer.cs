using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake
{
    internal sealed class AssetContainer : ScriptableObject
    {
        [SerializeField] internal AssetEntry[] assetEntries = System.Array.Empty<AssetEntry>();

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

        internal bool TryGetValue(string guid, out AssetEntry entry)
        {
            foreach (var asset in assetEntries)
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
            foreach (var asset in assetEntries)
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