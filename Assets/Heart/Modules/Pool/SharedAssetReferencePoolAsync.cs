#if PANCAKE_ADDRESSABLE && PANCAKE_UNITASK
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Pancake.Pools
{
    public static class SharedAssetReferencePoolAsync
    {
        private static readonly Dictionary<AssetReferenceGameObject, AsyncAddressableGameObjectPool> Pools = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init() { DisposeAll(); }

        public static async UniTask<GameObject> RequestAsync(this AssetReferenceGameObject original)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));
            return await GetOrCreatePool(original).RequestAsync();
        }

        public static async UniTask<GameObject> RequestAsync(this AssetReferenceGameObject original, Transform parent, bool worldPositionStays = false)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));
            return await GetOrCreatePool(original).RequestAsync(parent, worldPositionStays);
        }

        public static async UniTask<GameObject> RequestAsync(this AssetReferenceGameObject original, Vector3 position, Quaternion rotation)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));
            return await GetOrCreatePool(original).RequestAsync(position, rotation);
        }

        public static async UniTask<GameObject> RequestAsync(this AssetReferenceGameObject original, Vector3 position, Quaternion rotation, Transform parent)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));
            return await GetOrCreatePool(original).RequestAsync(position, rotation, parent);
        }

        public static async UniTask<TComponent> RequestAsync<TComponent>(this AssetReferenceGameObject original) where TComponent : Component
        {
            var temp = await RequestAsync(original);
            return temp.GetComponent<TComponent>();
        }

        public static async UniTask<TComponent> RequestAsync<TComponent>(this AssetReferenceGameObject original, Transform parent, bool worldPositionStays = false)
            where TComponent : Component
        {
            var temp = await RequestAsync(original, parent, worldPositionStays);
            return temp.GetComponent<TComponent>();
        }

        public static async UniTask<TComponent> RequestAsync<TComponent>(this AssetReferenceGameObject original, Vector3 position, Quaternion rotation)
            where TComponent : Component
        {
            var temp = await RequestAsync(original, position, rotation);
            return temp.GetComponent<TComponent>();
        }

        public static async UniTask<TComponent> RequestAsync<TComponent>(this AssetReferenceGameObject original, Vector3 position, Quaternion rotation, Transform parent)
            where TComponent : Component
        {
            var temp = await RequestAsync(original, position, rotation, parent);
            return temp.GetComponent<TComponent>();
        }

        public static void Return(this AssetReferenceGameObject original, GameObject gameObject)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));
            if (gameObject == null) throw new ArgumentNullException(nameof(gameObject));
            GetOrCreatePool(original).Return(gameObject);
        }

        public static async UniTask PrewarmAsync(this AssetReferenceGameObject original, int count, CancellationToken cancellationToken = default)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));
            var pool = GetOrCreatePool(original);
            await pool.PrewarmAsync(count, cancellationToken);
        }

        public static void Dispose(this AssetReferenceGameObject original)
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

        private static AsyncAddressableGameObjectPool GetOrCreatePool(AssetReferenceGameObject original)
        {
            if (Pools.TryGetValue(original, out var pool)) return pool;
            pool = new AsyncAddressableGameObjectPool(original);
            Pools.Add(original, pool);

            return pool;
        }
    }
}
#endif