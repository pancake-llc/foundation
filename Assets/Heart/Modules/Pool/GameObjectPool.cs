using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Pools
{
    public sealed class GameObjectPool : IObjectPool<GameObject>
    {
        public GameObjectPool(GameObject original)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));
            _original = original;
        }

        private readonly GameObject _original;
        private readonly Stack<GameObject> _stack = new(32);
        private bool _isDisposed;

        public int Count => _stack.Count;
        public bool IsDisposed => _isDisposed;

        public GameObject Request()
        {
            ThrowIfDisposed();

            if (!_stack.TryPop(out var obj))
            {
                obj = UnityEngine.Object.Instantiate(_original);
            }
            else
            {
                obj.SetActive(true);
            }

            PoolCallbackHelper.InvokeOnRequest(obj);
            return obj;
        }

        public GameObject Request(Transform parent)
        {
            ThrowIfDisposed();

            if (!_stack.TryPop(out var obj))
            {
                obj = UnityEngine.Object.Instantiate(_original, parent);
            }
            else
            {
                obj.transform.SetParent(parent);
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
                obj = UnityEngine.Object.Instantiate(_original, position, rotation);
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
                obj = UnityEngine.Object.Instantiate(_original, position, rotation, parent);
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

        public void Clear()
        {
            ThrowIfDisposed();

            while (_stack.TryPop(out var obj))
            {
                UnityEngine.Object.Destroy(obj);
            }
        }

        public void Prewarm(int count)
        {
            ThrowIfDisposed();

            for (int i = 0; i < count; i++)
            {
                var obj = UnityEngine.Object.Instantiate(_original);

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