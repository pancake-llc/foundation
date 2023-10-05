using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Pancake.SceneFlow
{
    public static class Static
    {
        public static Dictionary<string, AsyncOperationHandle<SceneInstance>> sceneHolder = new Dictionary<string, AsyncOperationHandle<SceneInstance>>();
    }
}