using System;
using System.Collections.Generic;
using UnityEngine;
#if PANCAKE_UNITASK
using Cysharp.Threading.Tasks;
#endif

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

        public GameObject Request(Transform parent, bool worldPositionStays = false)
        {
            ThrowIfDisposed();

            if (!_stack.TryPop(out var obj))
            {
                obj = UnityEngine.Object.Instantiate(_original, parent, worldPositionStays);
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

        private void Clear()
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

                obj.SetActive(false);
                _stack.Push(obj);

                PoolCallbackHelper.InvokeOnReturn(obj);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="batchCount"></param>
        /// <param name="batchSize"></param>
        /// <param name="timeSlice">Sets the target duration allowed per frame to integrate instantiated object operations, in milliseconds.</param>
        /// <param name="onPrewarmCompleted"></param>
        /// <exception cref="ArgumentNullException"></exception>
#if PANCAKE_UNITASK
        public async UniTaskVoid PrewarmAsync(
#else
        public async void PrewarmAsync(
#endif
            int batchCount,
            int batchSize,
            float timeSlice = 2f,
            Action onPrewarmCompleted = null)

        {
            ThrowIfDisposed();

            AsyncInstantiateOperation.SetIntegrationTimeMS(timeSlice);
            var operations = new AsyncInstantiateOperation<GameObject>[batchCount];
            for (var i = 0; i < batchCount; i++)
            {
                operations[i] = UnityEngine.Object.InstantiateAsync(_original, batchSize);
            }

            for (var i = 0; i < batchCount; i++)
            {
                while (!operations[i].isDone)
                {
#if PANCAKE_UNITASK
                    await UniTask.NextFrame();
#else
                    await Awaitable.NextFrameAsync();
#endif
                }
            }

            for (var i = 0; i < batchCount; i++)
            {
                foreach (var obj in operations[i].Result)
                {
                    obj.SetActive(false);
                    _stack.Push(obj);

                    PoolCallbackHelper.InvokeOnReturn(obj);
                }
            }

            onPrewarmCompleted?.Invoke();
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