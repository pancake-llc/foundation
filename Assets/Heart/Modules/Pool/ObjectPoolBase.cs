using System;
using System.Collections.Generic;

namespace Pancake.Pools
{
    public abstract class ObjectPoolBase<T> : IObjectPool<T> where T : class
    {
        protected readonly Stack<T> stack = new(32);
        private bool _isDisposed;
        
        public int Count => stack.Count;
        public bool IsDisposed => _isDisposed;

        protected abstract T CreateInstance();
        protected virtual void OnDestroy(T instance) { }
        protected virtual void OnRequest(T instance) { }
        protected virtual void OnReturn(T instance) { }

        public T Request()
        {
            ThrowIfDisposed();
            if (stack.TryPop(out var obj))
            {
                OnRequest(obj);
                if (obj is IPoolCallbackReceiver receiver) receiver.OnRequest();
                return obj;
            }

            return CreateInstance();
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

        public void Prewarm(int count)
        {
            ThrowIfDisposed();
            for (int i = 0; i < count; i++)
            {
                var instance = CreateInstance();
                Return(instance);
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