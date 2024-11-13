#if PANCAKE_UNITASK
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Pancake.Pools
{
    public abstract class AsyncObjectPoolBase<T> : IAsyncObjectPool<T>
        where T : class
    {
        protected readonly Stack<T> stack = new(32);
        private bool _isDisposed;
        
        public int Count => stack.Count;
        public bool IsDisposed => _isDisposed;

        protected abstract UniTask<T> CreateInstanceAsync(CancellationToken cancellationToken);
        protected virtual void OnDestroy(T instance) { }
        protected virtual void OnRequest(T instance) { }
        protected virtual void OnReturn(T instance) { }

        public UniTask<T> RequestAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            if (stack.TryPop(out var obj))
            {
                OnRequest(obj);
                if (obj is IPoolCallbackReceiver receiver) receiver.OnRequest();
                return new UniTask<T>(obj);
            }

            return CreateInstanceAsync(cancellationToken);
        }

        public void Return(T obj)
        {
            ThrowIfDisposed();
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            OnReturn(obj);
            if (obj is IPoolCallbackReceiver receiver) receiver.OnReturn();
            stack.Push(obj);
        }

        private void Clear()
        {
            ThrowIfDisposed();
            while (stack.TryPop(out var obj))
            {
                OnDestroy(obj);
            }
        }

        public async UniTask PrewarmAsync(int count, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            for (int i = 0; i < count; i++)
            {
                var instance = await CreateInstanceAsync(cancellationToken);
                Return(instance);
            }
        }

        public virtual void Dispose()
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