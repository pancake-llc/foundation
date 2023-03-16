using System.Collections.Generic;
using UnityEngine;

namespace Pancake
{
    public class Pool : Mono
    {
        public GameObject Prefab { get; private set; }
        public Transform Parent { get; private set; }
        public IReadOnlyList<Poolable> Poolables => _pooledGameObjects;
        private readonly List<Poolable> _pooledGameObjects = new List<Poolable>(64);


        private bool _isInitialized;

        public void Init(GameObject prefab, Transform parent)
        {
            if (_isInitialized) return;

            Prefab = prefab;
            Parent = parent;

            _isInitialized = true;
        }

        public Poolable Get()
        {
            if (_pooledGameObjects.Count > 0)
            {
                var poolable = _pooledGameObjects[0];
                _pooledGameObjects.RemoveAt(0);
                return poolable;
            }

            return Create();
        }

        public void Populate(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Add(Create());
            }
        }

        public void Add(Poolable poolable)
        {
            if (Prefab != poolable.Prefab)
            {
#if UNITY_EDITOR
                Debug.LogError(
                    $"You tries to add object from other pool.\nObject target : {poolable.Prefab.name.TextColor(Color.red)} and Pool : {Prefab.name.TextColor(Color.blue)}");
#endif
                return;
            }

            _pooledGameObjects.Add(poolable);
        }

        public void Remove(Poolable poolable)
        {
            if (Prefab != poolable.Prefab)
            {
#if UNITY_EDITOR
                Debug.LogError(
                    $"You tries to remove object from other pool.\nObject target : {poolable.Prefab.name.TextColor(Color.red)} and Pool : {Prefab.name.TextColor(Color.blue)}");
#endif
                return;
            }

            _pooledGameObjects.Remove(poolable);
        }

        private Poolable Create()
        {
            var poolable = Instantiate(Prefab, Parent).AddComponent<Poolable>();
            poolable.Init(this, Prefab, false);
            return poolable;
        }
    }
}