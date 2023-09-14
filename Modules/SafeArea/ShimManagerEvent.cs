#if UNITY_EDITOR
using System;
using UnityEditor;

namespace Pancake.SafeAreEditor
{
    public static class ShimManagerEvent
    {
        public static event Action OnActiveShimChanged;

        private static object activeScreenShim;

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            activeScreenShim = ShimManagerProxy.GetActiveScreenShim();
            
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
        }

        private static void OnUpdate()
        {
            var currentActiveScreenShim = ShimManagerProxy.GetActiveScreenShim();

            if (activeScreenShim != currentActiveScreenShim)
            {
                activeScreenShim = currentActiveScreenShim;
                OnActiveShimChanged?.Invoke();
            }
        }
    }
}
#endif