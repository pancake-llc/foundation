using System.Collections.Generic;
using UnityEngine;

namespace Pancake.MobileInput
{
    public class TouchWrapper
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
                    if (Input.touchCount > 0) return TouchData.From(Input.touches[0]);

                    return new TouchData {Position = Input.mousePosition};
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
                if (Input.touchCount > 0) return GetTouches();

                return new List<TouchData> {Touch0};
#else
                return GetTouches();
#endif
            }
        }

        public static Vector2 AverageTouchPosition
        {
            get
            {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
                if (Input.touchCount > 0) return GetAverageTouchPosition();

                return Input.mousePosition;
#else
                return GetAverageTouchPosition();
#endif
            }
        }

        /// <summary>
        /// Get touches from Unity Input to list <see cref="TouchData"/>
        /// </summary>
        /// <returns></returns>
        private static List<TouchData> GetTouches()
        {
            var touches = new List<TouchData>();
            foreach (var touch in Input.touches) touches.Add(TouchData.From(touch));
            return touches;
        }

        /// <summary>
        /// Get average from list touches of unity input
        /// </summary>
        /// <returns></returns>
        private static Vector2 GetAverageTouchPosition()
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