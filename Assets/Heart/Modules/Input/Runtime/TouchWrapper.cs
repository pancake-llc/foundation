using UnityEngine;
using System.Collections.Generic;

namespace Pancake.MobileInput
{
#if ENABLE_INPUT_SYSTEM && PANCAKE_INPUTSYSTEM
    public static class TouchWrapper
    {
        private static readonly UnityEngine.InputSystem.InputAction mousePosition;
        private static readonly UnityEngine.InputSystem.InputAction mouseButton;

        static TouchWrapper()
        {
            UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Enable();
            
            mousePosition = new UnityEngine.InputSystem.InputAction(type: UnityEngine.InputSystem.InputActionType.Value, binding: "<Mouse>/position");
            mouseButton = new UnityEngine.InputSystem.InputAction(type: UnityEngine.InputSystem.InputActionType.Button, binding: "<Mouse>/leftButton");
            
            mousePosition.Enable();
            mouseButton.Enable();
        }

        public static int TouchCount
        {
            get
            {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
                if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count > 0) 
                    return UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count;
                return mouseButton.IsPressed() ? 1 : 0;
#else
                return UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count;
#endif
            }
        }

        public static ITouchData Touch0
        {
            get
            {
                if (TouchCount > 0)
                {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
                    if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count > 0)
                        return InputSystemTouchData.From(UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[0]);
                    return InputSystemTouchData.FromMouse();
#else
                    return InputSystemTouchData.From(UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[0]);
#endif
                }
                return null;
            }
        }

        public static bool IsFingerDown => TouchCount > 0;

        public static List<ITouchData> Touches
        {
            get
            {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
                return UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count > 0 ? 
                    GetTouchesFromInput() : 
                    new List<ITouchData> { Touch0 };
#else
                return GetTouchesFromInput();
#endif
            }
        }

        private static List<ITouchData> GetTouchesFromInput()
        {
            var touches = new List<ITouchData>();
            foreach (var touch in UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches)
            {
                touches.Add(InputSystemTouchData.From(touch));
            }
            return touches;
        }

        public static Vector2 AverageTouchPos
        {
            get
            {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
                if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count > 0)
                    return GetAverageTouchPositionFromInput();
                return mousePosition.ReadValue<Vector2>();
#else
                return GetAverageTouchPositionFromInput();
#endif
            }
        }

        private static Vector2 GetAverageTouchPositionFromInput()
        {
            var position = Vector2.zero;
            if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count > 0)
            {
                foreach (var touch in UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches)
                {
                    position += touch.screenPosition;
                }
                position /= UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count;
            }
            return position;
        }
    }
#else
    public static class TouchWrapper
    {
        public static int TouchCount
        {
            get
            {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
                if (Input.touchCount > 0) return Input.touchCount;
                return Input.GetMouseButton(0) ? 1 : 0;
#else
                return Input.touchCount;
#endif
            }
        }

        public static ITouchData Touch0
        {
            get
            {
                if (TouchCount > 0)
                {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
                    return Input.touchCount > 0 ? 
                        LegacyTouchData.From(Input.touches[0]) : 
                        LegacyTouchData.FromMouse();
#else
                    return LegacyTouchData.From(Input.touches[0]);
#endif
                }
                return null;
            }
        }

        public static bool IsFingerDown => TouchCount > 0;

        public static List<ITouchData> Touches
        {
            get
            {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
                return Input.touchCount > 0 ? 
                    GetTouchesFromInput() : 
                    new List<ITouchData> { Touch0 };
#else
                return GetTouchesFromInput();
#endif
            }
        }

        private static List<ITouchData> GetTouchesFromInput()
        {
            var touches = new List<ITouchData>();
            foreach (var touch in Input.touches)
            {
                touches.Add(LegacyTouchData.From(touch));
            }
            return touches;
        }

        public static Vector2 AverageTouchPos
        {
            get
            {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
                if (Input.touchCount > 0)
                    return GetAverageTouchPositionFromInput();
                return Input.mousePosition;
#else
                return GetAverageTouchPositionFromInput();
#endif
            }
        }

        private static Vector2 GetAverageTouchPositionFromInput()
        {
            var position = Vector2.zero;
            if (Input.touches != null && Input.touches.Length > 0)
            {
                foreach (var touch in Input.touches)
                {
                    position += touch.position;
                }
                position /= Input.touches.Length;
            }
            return position;
        }
    }
#endif
}
