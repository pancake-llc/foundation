using System;
using Object = UnityEngine.Object;
#if PANCAKE_UNITASK
using Cysharp.Threading.Tasks;

#else
using System.Threading.Tasks;
#endif

namespace Pancake.AssetLoader
{
    public abstract class AssetLoadHandle
    {
        protected Func<float> percentCompleteFunc;

        protected AssetLoadHandle(int controlId) { ControlId = controlId; }

        public int ControlId { get; }

        public bool IsDone => Status != AssetLoadStatus.None;

        public AssetLoadStatus Status { get; protected set; }

        public float PercentComplete => percentCompleteFunc.Invoke();

        public Exception OperationException { get; protected set; }
    }

    public sealed class AssetLoadHandle<T> : AssetLoadHandle, IAssetLoadHandleSetter<T> where T : Object
    {
        public AssetLoadHandle(int controlId)
            : base(controlId)
        {
        }

        public T Result { get; private set; }

#if PANCAKE_UNITASK
        public UniTask<T> Task { get; private set; }
#else
        public Task<T> Task { get; private set; }
#endif

        void IAssetLoadHandleSetter<T>.SetStatus(AssetLoadStatus status) { Status = status; }

        void IAssetLoadHandleSetter<T>.SetResult(T result) { Result = result; }

        void IAssetLoadHandleSetter<T>.SetPercentCompleteFunc(Func<float> percentComplete) { percentCompleteFunc = percentComplete; }

#if PANCAKE_UNITASK
        void IAssetLoadHandleSetter<T>.SetTask(UniTask<T> task) { Task = task; }
#else
        void IAssetLoadHandleSetter<T>.SetTask(Task<T> task) { Task = task; }
#endif

        void IAssetLoadHandleSetter<T>.SetOperationException(Exception ex) { OperationException = ex; }
    }
}