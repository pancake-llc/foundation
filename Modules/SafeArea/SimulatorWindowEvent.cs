#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Pancake.SafeAreEditor
{
    public static class SimulatorWindowEvent
    {
        public static event Action OnOpen;
        public static event Action OnClose;
        public static event Action OnFocus;
        public static event Action OnLostFocus;
        public static event Action<ScreenOrientation> OnOrientationChanged;

        private static bool isOpen;
        private static bool hasFocus;
        private static ScreenOrientation orientation;

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            isOpen = SimulatorWindowProxy.isOpen;
            hasFocus = SimulatorWindowProxy.hasFocus;
            orientation = Screen.orientation;

            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
        }

        private static void OnUpdate()
        {
            if (isOpen == false && SimulatorWindowProxy.isOpen)
            {
                OnOpen?.Invoke();
                isOpen = true;
            }

            if (isOpen && SimulatorWindowProxy.isOpen == false)
            {
                OnClose?.Invoke();
                isOpen = false;
            }

            if (hasFocus == false && SimulatorWindowProxy.hasFocus)
            {
                OnFocus?.Invoke();
                hasFocus = true;
            }

            if (hasFocus && SimulatorWindowProxy.hasFocus == false)
            {
                OnLostFocus?.Invoke();
                hasFocus = false;
            }

            if (orientation != Screen.orientation)
            {
                OnOrientationChanged?.Invoke(Screen.orientation);
                orientation = Screen.orientation;
            }
        }
    }
}
#endif