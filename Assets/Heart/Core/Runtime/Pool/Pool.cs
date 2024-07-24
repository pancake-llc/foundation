using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pancake
{
    internal sealed class Pool
    {
        private GameObject _source;
        private Poolable _prototype;
        private Stack<Poolable> _insidePoolableInstances;
        private HashSet<Poolable> _outsidePoolableInstances;
        private readonly Quaternion _rotation;
        private readonly Vector3 _scale;
        private readonly bool _prototypeIsNotSource;
        private readonly bool _persistent;

        internal static readonly Dictionary<GameObject, Pool> PrefabLookup = new(64);
        private static readonly Dictionary<GameObject, Pool> InstanceLookup = new(512);

        private const int INITIAL_SIZE = 128;

        public Pool(GameObject prefab, bool persistent)
        {
            _source = prefab;
            _prototype = prefab.GetComponent<Poolable>();
            _persistent = persistent;

            if (_prototype == null)
            {
                _prototype = Object.Instantiate(prefab).AddComponent<Poolable>();
#if UNITY_EDITOR
                _prototype.name = prefab.name;
#endif
                Object.DontDestroyOnLoad(_prototype);
                _prototype.gameObject.SetActive(false);
                _prototypeIsNotSource = true;
            }

            _insidePoolableInstances = new Stack<Poolable>(INITIAL_SIZE);
            _outsidePoolableInstances = new HashSet<Poolable>(INITIAL_SIZE);
            PrefabLookup.Add(_source, this);

            var transform = prefab.transform;
            _rotation = transform.rotation;
            _scale = transform.localScale;
        }

        public static Pool GetPoolByPrefab(GameObject prefab, bool create = true, bool persistent = false)
        {
            bool hasPool = PrefabLookup.TryGetValue(prefab, out var pool);
            if (!hasPool && create) pool = new Pool(prefab, persistent);
            return pool;
        }

        public static bool GetPoolByInstance(GameObject instance, out Pool pool) { return InstanceLookup.TryGetValue(instance, out pool); }

        public static void Remove(GameObject instance)
        {
            var poolable = instance.GetComponent<Poolable>();
            Remove(poolable);
        }

        public static void Remove(Poolable poolable)
        {
            bool isPooled = GetPoolByInstance(poolable.gameObject, out var pool);
            if (isPooled) pool._outsidePoolableInstances.Remove(poolable);
            InstanceLookup.Remove(poolable.gameObject);
        }

        public void Populate(int count)
        {
            for (var i = 0; i < count; i++)
            {
                var instance = CreateInstance();
                instance.gameObject.SetActive(false);
                _insidePoolableInstances.Push(instance);
            }
        }

        public void Clear(bool isDestroy = true)
        {
            if (_persistent) return;

            PrefabLookup.Remove(_source);

            foreach (var instance in _insidePoolableInstances)
            {
                if (instance == null) continue;

                InstanceLookup.Remove(instance.gameObject);

                if (!isDestroy) continue;

                Object.Destroy(instance);
            }

            if (_prototypeIsNotSource) Object.Destroy(_prototype.gameObject);

            _source = null;
            _prototype = null;
            _insidePoolableInstances = null;
        }

        public void ReturnAll()
        {
            if (_persistent) return;

            foreach (var poolable in _outsidePoolableInstances.ToList())
            {
                Return(poolable);
            }

            _outsidePoolableInstances.Clear();
        }

        public GameObject Request()
        {
            var instance = GetInstance();
            _outsidePoolableInstances.Add(instance);
            return instance.gameObject;
        }

        public GameObject Request(Transform parent)
        {
            var instance = GetInstance();
            _outsidePoolableInstances.Add(instance);
            instance.transform.SetParent(parent);
            return instance.gameObject;
        }

        public GameObject Request(Transform parent, bool worldPositionStays)
        {
            var instance = GetInstance();
            _outsidePoolableInstances.Add(instance);
            instance.transform.SetParent(parent, worldPositionStays);
            return instance.gameObject;
        }

        public GameObject Request(Vector3 position, Quaternion rotation)
        {
            var instance = GetInstance();
            _outsidePoolableInstances.Add(instance);
            instance.transform.SetPositionAndRotation(position, rotation);
            return instance.gameObject;
        }

        public GameObject Request(Vector3 position, Quaternion rotation, Transform parent)
        {
            var instance = GetInstance();
            _outsidePoolableInstances.Add(instance);
            var instanceTransform = instance.transform;
            instanceTransform.SetPositionAndRotation(position, rotation);
            instanceTransform.SetParent(parent);
            return instance.gameObject;
        }

        public void Return(GameObject instance) { Return(instance.GetComponent<Poolable>()); }

        private void Return(Poolable poolable)
        {
            if(_insidePoolableInstances.Contains(poolable)) return;
            poolable.OnReturn();
            poolable.gameObject.SetActive(false);
            var instanceTransform = poolable.transform;
            instanceTransform.SetParent(null);
            instanceTransform.rotation = _rotation;
            instanceTransform.localScale = _scale;

            _insidePoolableInstances.Push(poolable);
            _outsidePoolableInstances.Remove(poolable);
        }

        private Poolable GetInstance()
        {
            var count = _insidePoolableInstances.Count;

            if (count != 0)
            {
                var instance = _insidePoolableInstances.Pop();

                if (instance == null)
                {
                    count--;

                    while (count != 0)
                    {
                        instance = _insidePoolableInstances.Pop();

                        if (instance != null)
                        {
                            instance.OnRequest();
                            instance.gameObject.SetActive(true);

                            return instance;
                        }

                        count--;
                    }

                    instance = CreateInstance();
                    instance.OnRequest();
                    instance.gameObject.SetActive(true);

                    return instance;
                }

                instance.OnRequest();
                instance.gameObject.SetActive(true);

                return instance;
            }
            else
            {
                var instance = CreateInstance();
                instance.OnRequest();
                instance.gameObject.SetActive(true);

                return instance;
            }
        }

        private Poolable CreateInstance()
        {
            var instance = Object.Instantiate(_prototype);
            var instanceGameObject = instance.gameObject;

            InstanceLookup.Add(instanceGameObject, this);

            return instance;
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetDomain()
        {
            PrefabLookup.Clear();
            InstanceLookup.Clear();
        }
#endif
    }
}