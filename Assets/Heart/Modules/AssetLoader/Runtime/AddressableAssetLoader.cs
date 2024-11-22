#if PANCAKE_ADDRESSABLE
using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;
#if PANCAKE_UNITASK
using Cysharp.Threading.Tasks;

#else
using System.Threading.Tasks;
#endif

namespace Pancake.AssetLoader
{
    public sealed class AddressableAssetLoader : IAssetLoader
    {
        private readonly Dictionary<int, AsyncOperationHandle> _controlIdToHandles = new();

        private int _nextControlId;

        public AssetLoadHandle<T> Load<T>(string key) where T : Object
        {
            var addressableHandle = Addressables.LoadAssetAsync<T>(key);
            addressableHandle.WaitForCompletion();
            var controlId = _nextControlId++;
            _controlIdToHandles.Add(controlId, addressableHandle);
            var handle = new AssetLoadHandle<T>(controlId);
            var setter = (IAssetLoadHandleSetter<T>) handle;
            setter.SetPercentCompleteFunc(() => addressableHandle.PercentComplete);
#if PANCAKE_UNITASK
            setter.SetTask(UniTask.FromResult(addressableHandle.Result));
#else
            setter.SetTask(Task.FromResult(addressableHandle.Result));
#endif

            setter.SetResult(addressableHandle.Result);
            var status = addressableHandle.Status == AsyncOperationStatus.Succeeded ? AssetLoadStatus.Success : AssetLoadStatus.Failed;
            setter.SetStatus(status);
            setter.SetOperationException(addressableHandle.OperationException);
            return handle;
        }

        public AssetLoadHandle<T> LoadAsync<T>(string key) where T : Object
        {
            var addressableHandle = Addressables.LoadAssetAsync<T>(key);
            var controlId = _nextControlId++;
            _controlIdToHandles.Add(controlId, addressableHandle);
            var handle = new AssetLoadHandle<T>(controlId);
            var setter = (IAssetLoadHandleSetter<T>) handle;
#if PANCAKE_UNITASK
            var tcs = new UniTaskCompletionSource<T>();
#else
            var tcs = new TaskCompletionSource<T>();
#endif
            addressableHandle.Completed += x =>
            {
                setter.SetResult(x.Result);
                var status = x.Status == AsyncOperationStatus.Succeeded ? AssetLoadStatus.Success : AssetLoadStatus.Failed;
                setter.SetStatus(status);
                setter.SetOperationException(addressableHandle.OperationException);
#if PANCAKE_UNITASK
                tcs.TrySetResult(x.Result);
#else
                tcs.SetResult(x.Result);
#endif
            };

            setter.SetPercentCompleteFunc(() => addressableHandle.PercentComplete);
            setter.SetTask(tcs.Task);
            return handle;
        }

        public void Release(AssetLoadHandle handle)
        {
            if (!_controlIdToHandles.ContainsKey(handle.ControlId))
            {
                throw new InvalidOperationException($"There is no asset that has been requested for release (ControlId: {handle.ControlId}).");
            }

            var addressableHandle = _controlIdToHandles[handle.ControlId];
            _controlIdToHandles.Remove(handle.ControlId);
            Addressables.Release(addressableHandle);
        }
    }
}
#endif