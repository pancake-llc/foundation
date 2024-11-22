using System;
#if PANCAKE_UNITASK
using Cysharp.Threading.Tasks;

#else
using System.Threading.Tasks;
#endif

namespace Pancake.AssetLoader
{
    public interface IAssetLoadHandleSetter<T>
    {
        void SetStatus(AssetLoadStatus status);

        void SetResult(T result);

        void SetPercentCompleteFunc(Func<float> percentComplete);

#if PANCAKE_UNITASK
        void SetTask(UniTask<T> task);
#else
        void SetTask(Task<T> task);
#endif

        void SetOperationException(Exception ex);
    }
}