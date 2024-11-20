using System;
using UnityEngine;
using Object = UnityEngine.Object;

#if PANCAKE_UNITASK
using Cysharp.Threading.Tasks;

#else
using System.Threading.Tasks;
#endif

namespace Pancake.AssetLoader
{
    public sealed class ResourcesAssetLoader : IAssetLoader
    {
        private int _nextControlId;

        public AssetLoadHandle<T> Load<T>(string key) where T : Object
        {
            var controlId = _nextControlId++;

            var handle = new AssetLoadHandle<T>(controlId);
            var setter = (IAssetLoadHandleSetter<T>) handle;
            var result = Resources.Load<T>(key);

            setter.SetResult(result);
            var status = result != null ? AssetLoadStatus.Success : AssetLoadStatus.Failed;
            setter.SetStatus(status);
            if (result == null)
            {
                var exception = new InvalidOperationException($"Requested asset（Key: {key}）was not found.");
                setter.SetOperationException(exception);
            }

            setter.SetPercentCompleteFunc(() => 1.0f);
#if PANCAKE_UNITASK
            setter.SetTask(UniTask.FromResult(result));

#else
            setter.SetTask(Task.FromResult(result));
#endif
            return handle;
        }

        public AssetLoadHandle<T> LoadAsync<T>(string key) where T : Object
        {
            var controlId = _nextControlId++;

            var handle = new AssetLoadHandle<T>(controlId);
            var setter = (IAssetLoadHandleSetter<T>) handle;
#if PANCAKE_UNITASK
            var tcs = new UniTaskCompletionSource<T>();
#else
            var tcs = new TaskCompletionSource<T>();
#endif

            var req = Resources.LoadAsync<T>(key);

            req.completed += _ =>
            {
                var result = req.asset as T;
                setter.SetResult(result);
                var status = result != null ? AssetLoadStatus.Success : AssetLoadStatus.Failed;
                setter.SetStatus(status);
                if (result == null)
                {
                    var exception = new InvalidOperationException($"Requested asset（Key: {key}）was not found.");
                    setter.SetOperationException(exception);
                }

#if PANCAKE_UNITASK
                tcs.TrySetResult(result);
#else
                tcs.SetResult(result);
#endif
            };

            setter.SetPercentCompleteFunc(() => req.progress);
            setter.SetTask(tcs.Task);
            return handle;
        }

        public void Release(AssetLoadHandle handle)
        {
            // Resources.UnloadUnusedAssets() is responsible for releasing assets loaded by Resources.Load(), so nothing is done here.
        }
    }
}