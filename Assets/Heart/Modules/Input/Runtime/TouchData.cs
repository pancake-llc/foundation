using UnityEngine;

namespace Pancake.MobileInput
{
    public interface ITouchData
    {
        Vector3 Position { get; }
        int FingerId { get; }
    }

#if ENABLE_INPUT_SYSTEM && PANCAKE_INPUTSYSTEM
    public class InputSystemTouchData : ITouchData
    {
        public Vector3 Position { get; private set; }
        public int FingerId { get; private set; }

        public static InputSystemTouchData From(UnityEngine.InputSystem.EnhancedTouch.Touch touch) =>
            new() { Position = touch.screenPosition, FingerId = touch.touchId };

        public static InputSystemTouchData FromMouse()
        {
            var mousePosition = UnityEngine.InputSystem.Mouse.current?.position.ReadValue() ?? Vector2.zero;
            return new() { Position = mousePosition, FingerId = 0 };
        }
    }
#else
    public class LegacyTouchData : ITouchData
    {
        public Vector3 Position { get; private set; }
        public int FingerId { get; private set; }

        public static LegacyTouchData From(UnityEngine.Touch touch) =>
            new() { Position = touch.position, FingerId = touch.fingerId };

        public static LegacyTouchData FromMouse() =>
            new() { Position = Input.mousePosition, FingerId = 0 };
    }

#endif
}