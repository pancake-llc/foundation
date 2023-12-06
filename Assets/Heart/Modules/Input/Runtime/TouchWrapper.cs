using UnityEngine;
using System.Collections.Generic;

namespace Pancake.MobileInput
{
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

        public static TouchData Touch0
        {
            get
            {
                if (TouchCount > 0)
                {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
                    return Input.touchCount > 0 ? TouchData.From(Input.touches[0]) : new TouchData {Position = Input.mousePosition};
#else
                    return TouchData.From(Input.touches[0]);
#endif
                }

                return null;
            }
        }

        public static bool IsFingerDown => TouchCount > 0;

        public static List<TouchData> Touches
        {
            get
            {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL

                return Input.touchCount > 0 ? GetTouchesFromInput() : new List<TouchData> {Touch0};
#else
                return GetTouchesFromInput();
#endif
            }
        }

        private static List<TouchData> GetTouchesFromInput()
        {
            var touches = new List<TouchData>();
            foreach (var touch in Input.touches) touches.Add(TouchData.From(touch));
            return touches;
        }

        public static Vector2 AverageTouchPos
        {
            get
            {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
                if (Input.touchCount > 0) return GetAverageTouchPositionFromInput();
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
                foreach (var touch in Input.touches) position += touch.position;
                position /= Input.touches.Length;
            }

            return position;
        }
    }
}