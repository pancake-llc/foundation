using UnityEngine;
using System;

namespace Pancake
{
    [DisallowMultipleComponent]
    [EditorIcon("cs")]
    internal sealed class Poolable : MonoBehaviour
    {
        private IPoolable[] _poolables = Array.Empty<IPoolable>();
        private bool _isInitialized;

        private void Awake()
        {
            _poolables = GetComponentsInChildren<IPoolable>(true);
            _isInitialized = true;
        }

        private void OnDestroy() { Pool.Remove(gameObject); }

        public void OnRequest()
        {
            if (!_isInitialized) return;

            for (var i = 0; i < _poolables.Length; i++) _poolables[i].OnRequest();
        }

        public void OnReturn()
        {
            for (var i = 0; i < _poolables.Length; i++) _poolables[i].OnReturn();
        }
    }
}