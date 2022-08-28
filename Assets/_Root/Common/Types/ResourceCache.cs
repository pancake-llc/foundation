using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Core
{
    /// <summary>
    /// Helper for cache Resources.Load calls with 2x performance boost.
    /// </summary>
    public sealed class ResourceCache : MonoBehaviour
    {
        private readonly Dictionary<string, Object> _cache = new Dictionary<string, Object>(512);

        /// <summary>
        /// Return loaded resource from cache or load it.
        /// Important:
        /// if you request resource with one type,
        /// you cant get it for same path and different type.
        /// </summary>
        /// <param name="path">Path to loadable resource relative to "Resources" folder.</param>
        public T Load<T>(string path) where T : Object
        {
            if (!_cache.TryGetValue(path, out var asset))
            {
                asset = Resources.Load<T>(path);
                if (asset != null)
                {
                    _cache[path] = asset;
                }
            }

            return asset as T;
        }

        /// <summary>
        /// Force unload resource. Use carefully.
        /// </summary>
        /// <param name="path">Path to loadable resource relative to "Resources" folder.</param>
        public void Unload(string path)
        {
            if (_cache.TryGetValue(path, out var asset))
            {
                _cache.Remove(path);
                Resources.UnloadAsset(asset);
            }
        }
        
        private void ClearCache() { _cache.Clear(); }
    }
}