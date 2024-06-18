using System;

namespace Pancake.Pools
{
    public sealed class ObjectPool<T> : ObjectPoolBase<T> where T : class
    {
        private readonly Func<T> _createFunc;
        private readonly Action<T> _onRequest;
        private readonly Action<T> _onReturn;
        private readonly Action<T> _onDestroy;

        public ObjectPool(Func<T> createFunc, Action<T> onRequest = null, Action<T> onReturn = null, Action<T> onDestroy = null)
        {
            _createFunc = createFunc ?? throw new ArgumentException(nameof(createFunc));
            _onRequest = onRequest;
            _onReturn = onReturn;
            _onDestroy = onDestroy;
        }

        protected override T CreateInstance() { return _createFunc(); }

        protected override void OnDestroy(T instance) { _onDestroy?.Invoke(instance); }

        protected override void OnRequest(T instance) { _onRequest?.Invoke(instance); }

        protected override void OnReturn(T instance) { _onReturn?.Invoke(instance); }
    }
}