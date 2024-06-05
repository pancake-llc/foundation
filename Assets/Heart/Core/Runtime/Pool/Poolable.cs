using UnityEngine;
using System;
using Pancake.Common;

namespace Pancake
{
    [DisallowMultipleComponent]
    [EditorIcon("icon_default")]
    public sealed class Poolable : MonoBehaviour
    {
        private IPoolable[] _poolables = Array.Empty<IPoolable>();

        private IPoolable[] Poolables
        {
            get
            {
                if (_poolables.IsNullOrEmpty()) _poolables = GetComponentsInChildren<IPoolable>(true);
                return _poolables;
            }
        }

        private void OnDestroy() { Pool.Remove(this); }

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