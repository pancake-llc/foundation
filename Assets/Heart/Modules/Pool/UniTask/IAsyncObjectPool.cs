#if PANCAKE_UNITASK
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Pancake.Pools
{
    public interface IAsyncObjectPool<T>
    {
        UniTask<T> RequestAsync(CancellationToken cancellationToken);
        void Return(T instance);
    }
}
#endif