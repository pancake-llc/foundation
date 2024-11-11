#if PANCAKE_ADDRESSABLE
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Pancake.Pools
{
    public static class SharedAssetReferencePool
    {
        private static readonly Dictionary<AssetReferenceGameObject, AddressableGameObjectPool> Pools = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init() { DisposeAll(); }

        public static GameObject Request(AssetReferenceGameObject original)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));
            return GetOrCreatePool(original).Request();
        }

        public static GameObject Request(AssetReferenceGameObject original, Transform parent, bool worldPositionStays = false)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));
            return GetOrCreatePool(original).Request(parent, worldPositionStays);
        }

        public static GameObject Request(AssetReferenceGameObject original, Vector3 position, Quaternion rotation)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));
            return GetOrCreatePool(original).Request(position, rotation);
        }

        public static GameObject Request(AssetReferenceGameObject original, Vector3 position, Quaternion rotation, Transform parent)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));
            return GetOrCreatePool(original).Request(position, rotation, parent);
        }

        public static TComponent Request<TComponent>(AssetReferenceGameObject original) where TComponent : Component
        {
            return Request(original).GetComponent<TComponent>();
        }

        public static TComponent Request<TComponent>(AssetReferenceGameObject original, Transform parent, bool worldPositionStays = false)
            where TComponent : Component
        {
            return Request(original, parent, worldPositionStays).GetComponent<TComponent>();
        }

        public static TComponent Request<TComponent>(AssetReferenceGameObject original, Vector3 position, Quaternion rotation) where TComponent : Component
        {
            return Request(original, position, rotation).GetComponent<TComponent>();
        }

        public static TComponent Request<TComponent>(AssetReferenceGameObject original, Vector3 position, Quaternion rotation, Transform parent)
            where TComponent : Component
        {
            return Request(original, position, rotation, parent).GetComponent<TComponent>();
        }

        public static void Return(AssetReferenceGameObject original, GameObject gameObject)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));
            if (gameObject == null) throw new ArgumentNullException(nameof(gameObject));
            GetOrCreatePool(original).Return(gameObject);
        }

        public static void Prewarm(AssetReferenceGameObject original, int count)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));
            var pool = GetOrCreatePool(original);
            pool.Prewarm(count);
        }

        public static void Dispose(AssetReferenceGameObject original)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));
            var pool = GetOrCreatePool(original);
            pool.Dispose();
        }

        public static void DisposeAll()
        {
            foreach (var pool in Pools.Values)
            {
                pool.Dispose();
            }

            Pools.Clear();
        }

        private static AddressableGameObjectPool GetOrCreatePool(AssetReferenceGameObject original)
        {
            if (Pools.TryGetValue(original, out var pool)) return pool;
            pool = new AddressableGameObjectPool(original);
            Pools.Add(original, pool);

            return pool;
        }
    }
}
#endif