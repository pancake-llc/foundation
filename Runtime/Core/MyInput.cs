#if PANCAKE_INPUTSYSTEM
using System;
using UnityEngine.InputSystem.EnhancedTouch;
using ETouch = UnityEngine.InputSystem.EnhancedTouch;

namespace Pancake
{
    public sealed class MyInput : BaseBehaviour
    {
        public static event Action<Finger> FingerDownEvent;
        public static event Action<Finger> FingerUpEvent;
        public static event Action<Finger> FingerMoveEvent;

        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
            Touch.onFingerDown += OnFingerDown;
            Touch.onFingerUp += OnFingerUp;
            Touch.onFingerMove += OnFingerMove;
        }

        private void OnFingerMove(Finger finger) { FingerMoveEvent?.Invoke(finger); }

        private void OnFingerUp(Finger finger) { FingerUpEvent?.Invoke(finger); }

        private void OnFingerDown(Finger finger) { FingerDownEvent?.Invoke(finger); }

        private void OnDisable()
        {
            Touch.onFingerDown -= OnFingerDown;
            Touch.onFingerUp -= OnFingerUp;
            Touch.onFingerMove -= OnFingerMove;
            EnhancedTouchSupport.Disable();
        }
    }
}
#endif