using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Pools
{
    public static class SharedGameObjectPool
    {
        private static readonly Dictionary<GameObject, Stack<GameObject>> Pools = new();
        private static readonly Dictionary<GameObject, Stack<GameObject>> CloneReferences = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            Pools.Clear();
            CloneReferences.Clear();
        }

        public static GameObject Request(this GameObject original)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));

            var pool = GetOrCreatePool(original);

            GameObject obj;
            while (true)
            {
                if (!pool.TryPop(out obj))
                {
                    obj = UnityEngine.Object.Instantiate(original);
                    break;
                }

                if (obj != null)
                {
                    obj.SetActive(true);
                    break;
                }
            }

            CloneReferences.Add(obj, pool);

            PoolCallbackHelper.InvokeOnRequest(obj);

            return obj;
        }

        public static GameObject Request(this GameObject original, Transform parent)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));

            var pool = GetOrCreatePool(original);

            GameObject obj;
            while (true)
            {
                if (!pool.TryPop(out obj))
                {
                    obj = UnityEngine.Object.Instantiate(original, parent);
                    break;
                }

                if (obj != null)
                {
                    obj.transform.SetParent(parent);
                    obj.SetActive(true);
                    break;
                }
            }

            CloneReferences.Add(obj, pool);

            PoolCallbackHelper.InvokeOnRequest(obj);

            return obj;
        }

        public static GameObject Request(this GameObject original, Vector3 position, Quaternion rotation)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));

            var pool = GetOrCreatePool(original);

            GameObject obj;
            while (true)
            {
                if (!pool.TryPop(out obj))
                {
                    obj = UnityEngine.Object.Instantiate(original, position, rotation);
                    break;
                }

                if (obj != null)
                {
                    obj.transform.SetPositionAndRotation(position, rotation);
                    obj.SetActive(true);
                    break;
                }
            }

            CloneReferences.Add(obj, pool);

            PoolCallbackHelper.InvokeOnRequest(obj);

            return obj;
        }

        public static GameObject Request(this GameObject original, Vector3 position, Quaternion rotation, Transform parent)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));

            var pool = GetOrCreatePool(original);

            GameObject obj;
            while (true)
            {
                if (!pool.TryPop(out obj))
                {
                    obj = UnityEngine.Object.Instantiate(original, position, rotation, parent);
                    break;
                }

                if (obj != null)
                {
                    obj.transform.SetParent(parent);
                    obj.transform.SetPositionAndRotation(position, rotation);
                    obj.SetActive(true);
                    break;
                }
            }

            CloneReferences.Add(obj, pool);

            PoolCallbackHelper.InvokeOnRequest(obj);

            return obj;
        }

        public static TComponent Request<TComponent>(TComponent original) where TComponent : Component { return Request(original.gameObject).GetComponent<TComponent>(); }

        public static TComponent Request<TComponent>(TComponent original, Vector3 position, Quaternion rotation, Transform parent) where TComponent : Component
        {
            return Request(original.gameObject, position, rotation, parent).GetComponent<TComponent>();
        }

        public static TComponent Request<TComponent>(TComponent original, Vector3 position, Quaternion rotation) where TComponent : Component
        {
            return Request(original.gameObject, position, rotation).GetComponent<TComponent>();
        }

        public static TComponent Request<TComponent>(TComponent original, Transform parent) where TComponent : Component
        {
            return Request(original.gameObject, parent).GetComponent<TComponent>();
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

        private static Stack<GameObject> GetOrCreatePool(GameObject original)
        {
            if (!Pools.TryGetValue(original, out var pool))
            {
                pool = new Stack<GameObject>();
                Pools.Add(original, pool);
            }

            return pool;
        }
    }
}