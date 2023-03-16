using System;
using UnityEngine;

namespace Pancake
{
    
    /// <summary>
    /// Attached in each object marked as object in the pool
    /// </summary>
    public class Poolable : Mono, IPoolable
    {
        public Pool Pool { get; private set; }
        public GameObject Prefab { get; private set; }
        public bool IsActive { get; private set; }

        private bool _isInitialized;

        public void Init(Pool pool, GameObject prefab, bool active)
        {
            if (_isInitialized) return;
            if (prefab == null) throw new NullReferenceException(nameof(prefab), null);
           
            gameObject.SetActive(active);
            IsActive = active;
            Prefab = prefab;
            Pool = pool;
            
            _isInitialized = true;
        }

        void IPoolable.OnSpawn()
        {
            IsActive = true;
        }

        void IPoolable.OnDespawn()
        {
            IsActive = false;
        }

        private void OnDestroy()
        {
            if (Pool != null) Pool.Remove(this);
        }
    }
}