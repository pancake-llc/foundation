using System;
using System.Collections.Generic;
using UnityEngine;
#if PANCAKE_UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace Pancake.Pools
{
    public static class SharedGameObjectPool
    {
        private static readonly Dictionary<GameObject, Stack<GameObject>> Pools = new();
        private static readonly Dictionary<GameObject, Stack<GameObject>> CloneReferences = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init() { DisposeAll(); }

        public static GameObject Request(this GameObject original)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));

            var pool = GetOrCreatePool(original);

            if (!pool.TryPop(out var obj)) obj = UnityEngine.Object.Instantiate(original);

            if (obj != null) obj.SetActive(true);

            CloneReferences.Add(obj, pool);

            PoolCallbackHelper.InvokeOnRequest(obj);

            return obj;
        }

        public static GameObject Request(this GameObject original, Transform parent, bool worldPositionStays = false)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));

            var pool = GetOrCreatePool(original);

            if (!pool.TryPop(out var obj)) obj = UnityEngine.Object.Instantiate(original, parent, worldPositionStays);

            if (obj != null)
            {
                obj.transform.SetParent(parent, worldPositionStays);
                obj.SetActive(true);
            }

            CloneReferences.Add(obj, pool);

            PoolCallbackHelper.InvokeOnRequest(obj);

            return obj;
        }

        public static GameObject Request(this GameObject original, Vector3 position, Quaternion rotation)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));

            var pool = GetOrCreatePool(original);

            if (!pool.TryPop(out var obj)) obj = UnityEngine.Object.Instantiate(original, position, rotation);

            if (obj != null)
            {
                obj.transform.SetPositionAndRotation(position, rotation);
                obj.SetActive(true);
            }

            CloneReferences.Add(obj, pool);

            PoolCallbackHelper.InvokeOnRequest(obj);

            return obj;
        }

        public static GameObject Request(this GameObject original, Vector3 position, Quaternion rotation, Transform parent)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));

            var pool = GetOrCreatePool(original);

            if (!pool.TryPop(out var obj)) obj = UnityEngine.Object.Instantiate(original, position, rotation, parent);

            if (obj != null)
            {
                obj.transform.SetParent(parent);
                obj.transform.SetPositionAndRotation(position, rotation);
                obj.SetActive(true);
            }

            CloneReferences.Add(obj, pool);

            PoolCallbackHelper.InvokeOnRequest(obj);

            return obj;
        }

        public static TComponent Request<TComponent>(this GameObject original) where TComponent : Component { return Request(original).GetComponent<TComponent>(); }

        public static TComponent Request<TComponent>(this GameObject original, Transform parent, bool worldPositionStays = false) where TComponent : Component
        {
            return Request(original, parent, worldPositionStays).GetComponent<TComponent>();
        }

        public static TComponent Request<TComponent>(this GameObject original, Vector3 position, Quaternion rotation) where TComponent : Component
        {
            return Request(original, position, rotation).GetComponent<TComponent>();
        }

        public static TComponent Request<TComponent>(this GameObject original, Vector3 position, Quaternion rotation, Transform parent) where TComponent : Component
        {
            return Request(original, position, rotation, parent).GetComponent<TComponent>();
        }

        public static void Return(this GameObject instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            var pool = CloneReferences[instance];
            instance.SetActive(false);
            pool.Push(instance);
            CloneReferences.Remove(instance);

            PoolCallbackHelper.InvokeOnReturn(instance);
        }

        public static void Prewarm(this GameObject original, int count)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));

            var pool = GetOrCreatePool(original);

            for (int i = 0; i < count; i++)
            {
                var obj = UnityEngine.Object.Instantiate(original);

                obj.SetActive(false);
                pool.Push(obj);

                PoolCallbackHelper.InvokeOnReturn(obj);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="original"></param>
        /// <param name="batchCount"></param>
        /// <param name="batchSize"></param>
        /// <param name="timeSlice">Sets the target duration allowed per frame to integrate instantiated object operations, in milliseconds.</param>
        /// <param name="onPrewarmCompleted"></param>
        /// <exception cref="ArgumentNullException"></exception>
#if PANCAKE_UNITASK
        public static async UniTaskVoid PrewarmAsync(
#else
        public static async void PrewarmAsync(
#endif
            this GameObject original,
            int batchCount,
            int batchSize,
            float timeSlice = 2f,
            Action onPrewarmCompleted = null)

        {
            if (original == null) throw new ArgumentNullException(nameof(original));

            var pool = GetOrCreatePool(original);
            AsyncInstantiateOperation.SetIntegrationTimeMS(timeSlice);
            var operations = new AsyncInstantiateOperation<GameObject>[batchCount];
            for (var i = 0; i < batchCount; i++)
            {
                operations[i] = UnityEngine.Object.InstantiateAsync(original, batchSize);
            }

            for (var i = 0; i < batchCount; i++)
            {
                while (!operations[i].isDone)
                {
#if PANCAKE_UNITASK
                    await UniTask.NextFrame();
#else
                    await Awaitable.NextFrameAsync();
#endif
                }
            }

            for (var i = 0; i < batchCount; i++)
            {
                foreach (var obj in operations[i].Result)
                {
                    obj.SetActive(false);
                    pool.Push(obj);

                    PoolCallbackHelper.InvokeOnReturn(obj);
                }
            }

            onPrewarmCompleted?.Invoke();
        }

        public static void DisposeAll()
        {
            Pools.Clear();
            CloneReferences.Clear();
        }

        public static void Dispose(GameObject original)
        {
            if (Pools.TryGetValue(original, out var pool)) pool.Clear();
            if (CloneReferences.TryGetValue(original, out var poolRef)) poolRef.Clear();
        }

        private static Stack<GameObject> GetOrCreatePool(GameObject original)
        {
            if (Pools.TryGetValue(original, out var pool)) return pool;
            pool = new Stack<GameObject>();
            Pools.Add(original, pool);

            return pool;
        }
    }
}