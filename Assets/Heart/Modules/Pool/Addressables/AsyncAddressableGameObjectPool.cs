#if PANCAKE_ADDRESSABLE && PANCAKE_UNITASK
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Pancake.Pools
{
    public sealed class AsyncAddressableGameObjectPool : IAsyncObjectPool<GameObject>
    {
        public AsyncAddressableGameObjectPool(object key) { _key = key ?? throw new ArgumentNullException(nameof(key)); }

        public AsyncAddressableGameObjectPool(AssetReferenceGameObject reference)
        {
            if (reference == null) throw new ArgumentNullException(nameof(reference));
            _key = reference.RuntimeKey;
        }

        private readonly object _key;
        private readonly Stack<GameObject> _stack = new(32);
        private bool _isDisposed;

        public int Count => _stack.Count;
        public bool IsDisposed => _isDisposed;

        public async UniTask<GameObject> RequestAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            if (!_stack.TryPop(out var obj))
            {
                obj = await Addressables.InstantiateAsync(_key).ToUniTask(cancellationToken: cancellationToken);
            }
            else
            {
                obj.SetActive(true);
            }

            PoolCallbackHelper.InvokeOnRequest(obj);
            return obj;
        }

        public async UniTask<GameObject> RequestAsync(Transform parent, bool worldPositionStays = false, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            if (!_stack.TryPop(out var obj))
            {
                obj = await Addressables.InstantiateAsync(_key, parent, worldPositionStays).ToUniTask(cancellationToken: cancellationToken);
            }
            else
            {
                obj.transform.SetParent(parent, worldPositionStays);
                obj.SetActive(true);
            }

            PoolCallbackHelper.InvokeOnRequest(obj);
            return obj;
        }

        public async UniTask<GameObject> RequestAsync(Vector3 position, Quaternion rotation, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            if (!_stack.TryPop(out var obj))
            {
                obj = await Addressables.InstantiateAsync(_key, position, rotation).ToUniTask(cancellationToken: cancellationToken);
            }
            else
            {
                obj.transform.SetPositionAndRotation(position, rotation);
                obj.SetActive(true);
            }

            PoolCallbackHelper.InvokeOnRequest(obj);
            return obj;
        }

        public async UniTask<GameObject> RequestAsync(Vector3 position, Quaternion rotation, Transform parent)
        {
            ThrowIfDisposed();

            if (!_stack.TryPop(out var obj))
            {
                obj = await Addressables.InstantiateAsync(_key, position, rotation, parent);
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

        public async UniTask PrewarmAsync(int count, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            for (int i = 0; i < count; i++)
            {
                var obj = await Addressables.InstantiateAsync(_key).ToUniTask(cancellationToken: cancellationToken);

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