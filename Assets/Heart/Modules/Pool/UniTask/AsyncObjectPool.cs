#if PANCAKE_UNITASK
using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Pancake.Pools
{
    public sealed class AsyncObjectPool<T> : AsyncObjectPoolBase<T> where T : class
    {
        public AsyncObjectPool(Func<UniTask<T>> createFunc, Action<T> onRequest = null, Action<T> onReturn = null, Action<T> onDestroy = null)
        {
            _createFunc = _ => createFunc();
            _onRequest = onRequest;
            _onReturn = onReturn;
            _onDestroy = onDestroy;
        }

        public AsyncObjectPool(Func<CancellationToken, UniTask<T>> createFunc, Action<T> onRequest = null, Action<T> onReturn = null, Action<T> onDestroy = null)
        {
            _createFunc = createFunc ?? throw new ArgumentException(nameof(createFunc));
            _onRequest = onRequest;
            _onReturn = onReturn;
            _onDestroy = onDestroy;
        }

        private readonly Func<CancellationToken, UniTask<T>> _createFunc;
        private readonly Action<T> _onRequest;
        private readonly Action<T> _onReturn;
        private readonly Action<T> _onDestroy;

        protected override UniTask<T> CreateInstanceAsync(CancellationToken cancellationToken) { return _createFunc(cancellationToken); }

        protected override void OnDestroy(T instance) { _onDestroy?.Invoke(instance); }

        protected override void OnRequest(T instance) { _onRequest?.Invoke(instance); }

        protected override void OnReturn(T instance) { _onReturn?.Invoke(instance); }
    }
}
#endif