using UnityEngine;
using System;

namespace Pancake
{
    [DisallowMultipleComponent]
    [EditorIcon("cs")]
    internal sealed class Poolable : MonoBehaviour
    {
        private IPoolable[] _poolables = Array.Empty<IPoolable>();
        
        public Transform Transform => transform;

        private IPoolable[] Poolables
        {
            get
            {
                if (_poolables.IsNullOrEmpty()) _poolables = GetComponentsInChildren<IPoolable>(true);
                return _poolables;
            }
        }

        private void OnDestroy() { Pool.Remove(gameObject); }

        public void OnRequest()
        {
            float length = Poolables.Length;
            for (var i = 0; i < length; i++) Poolables[i].OnRequest();
        }

        public void OnReturn()
        {
            float length = Poolables.Length;
            for (var i = 0; i < length; i++) Poolables[i].OnReturn();
        }
    }
}