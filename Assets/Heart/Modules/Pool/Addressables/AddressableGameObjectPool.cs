#if PANCAKE_ADDRESSABLE
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Pancake.Pools
{
    public sealed class AddressableGameObjectPool : IObjectPool<GameObject>
    {
        public AddressableGameObjectPool(object key) { _key = key ?? throw new ArgumentNullException(nameof(key)); }

        public AddressableGameObjectPool(AssetReferenceGameObject reference)
        {
            if (reference == null) throw new ArgumentNullException(nameof(reference));
            _key = reference.RuntimeKey;
        }

        private readonly object _key;
        private readonly Stack<GameObject> _stack = new(32);
        private bool _isDisposed;

        public int Count => _stack.Count;
        public bool IsDisposed => _isDisposed;

        public GameObject Request()
        {
            ThrowIfDisposed();

            if (!_stack.TryPop(out var obj))
            {
                obj = Addressables.InstantiateAsync(_key).WaitForCompletion();
            }
            else
            {
                obj.SetActive(true);
            }

            PoolCallbackHelper.InvokeOnRequest(obj);
            return obj;
        }

        public GameObject Request(Transform parent, bool worldPositionStays = false)
        {
            ThrowIfDisposed();

            if (!_stack.TryPop(out var obj))
            {
                obj = Addressables.InstantiateAsync(_key, parent, worldPositionStays).WaitForCompletion();
            }
            else
            {
                obj.transform.SetParent(parent, worldPositionStays);
                obj.SetActive(true);
            }

            PoolCallbackHelper.InvokeOnRequest(obj);
            return obj;
        }

        public GameObject Request(Vector3 position, Quaternion rotation)
        {
            ThrowIfDisposed();

            if (!_stack.TryPop(out var obj))
            {
                obj = Addressables.InstantiateAsync(_key, position, rotation).WaitForCompletion();
            }
            else
            {
                obj.transform.SetPositionAndRotation(position, rotation);
                obj.SetActive(true);
            }

            PoolCallbackHelper.InvokeOnRequest(obj);
            return obj;
        }

        public GameObject Request(Vector3 position, Quaternion rotation, Transform parent)
        {
            ThrowIfDisposed();

            if (!_stack.TryPop(out var obj))
            {
                obj = Addressables.InstantiateAsync(_key, position, rotation, parent).WaitForCompletion();
            }
            else
            {
                obj.transform.SetParent(parent);
                obj.transform.SetPositionAndRotation(position, rotation);
                obj.SetActive(true);
            }

            PoolCallbackHelper.InvokeOnRequest(obj);
            return obj;
        }

        public void Return(GameObject obj)
        {
            ThrowIfDisposed();

            _stack.Push(obj);
            obj.SetActive(false);

            PoolCallbackHelper.InvokeOnReturn(obj);
        }

        private void Clear()
        {
            ThrowIfDisposed();

            while (_stack.TryPop(out var obj))
            {
                Addressables.ReleaseInstance(obj);
            }
        }

        public void Prewarm(int count)
        {
            ThrowIfDisposed();

            for (int i = 0; i < count; i++)
            {
                var obj = Addressables.InstantiateAsync(_key).WaitForCompletion();

                _stack.Push(obj);
                obj.SetActive(false);

                PoolCallbackHelper.InvokeOnReturn(obj);
            }
        }

        public void Dispose()
        {
            ThrowIfDisposed();
            Clear();
            _isDisposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed) throw new ObjectDisposedException(GetType().Name);
        }
    }
}
#endif