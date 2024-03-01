using System.Collections.Generic;
using UnityEngine;

namespace Pancake
{
    internal sealed class ScriptablePool<T> where T : ScriptableObject, IPoolable
    {
        private T _source;
        private Stack<T> _instances;
        private static readonly Dictionary<T, ScriptablePool<T>> OriginalLookup = new(64);
        private static readonly Dictionary<T, ScriptablePool<T>> InstanceLookup = new(128);
        private const int INITIAL_SIZE = 128;

        public ScriptablePool(T source)
        {
            _source = source;
            _instances = new Stack<T>(INITIAL_SIZE);
            OriginalLookup.Add(_source, this);
        }

        public static ScriptablePool<T> GetPoolByOriginal(T source, bool create = true)
        {
            bool hasPool = OriginalLookup.TryGetValue(source, out var pool);
            if (!hasPool && create) pool = new ScriptablePool<T>(source);
            return pool;
        }

        public static bool GetPoolByInstance(T instance, out ScriptablePool<T> pool) { return InstanceLookup.TryGetValue(instance, out pool); }

        public static void Remove(T instance) { InstanceLookup.Remove(instance); }

        public void Populate(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var instance = Object.Instantiate(_source);
                _instances.Push(instance);
            }
        }

        public void Clear()
        {
            OriginalLookup.Clear();
            foreach (var instance in _instances)
            {
                if (instance == null) continue;
                InstanceLookup.Remove(instance);
                Object.Destroy(instance);
            }

            _source = null;
            _instances = null;
        }

        public T Request()
        {
            int count = _instances.Count;
            if (count != 0)
            {
                var instance = _instances.Pop();
                if (instance == null)
                {
                    count--;
                    while (count != 0)
                    {
                        instance = _instances.Pop();
                        if (instance != null)
                        {
                            instance.OnRequest();
                            return instance;
                        }

                        count--;
                    }

                    instance = CreateInstance();
                    return instance;
                }

                instance.OnRequest();
                return instance;
            }
            else
            {
                var instance = CreateInstance();
                return instance;
            }
        }

        public void Return(T instance)
        {
            instance.OnReturn();
            _instances.Push(instance);
        }

        private T CreateInstance()
        {
            var instance = Object.Instantiate(_source);
            InstanceLookup.Add(instance, this);
            _instances.Push(instance);
            return instance;
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetDomain()
        {
            OriginalLookup.Clear();
            InstanceLookup.Clear();
        }
#endif
    }
}