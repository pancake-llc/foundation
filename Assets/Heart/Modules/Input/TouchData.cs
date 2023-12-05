using UnityEngine;

namespace Pancake.MobileInput
{
    /// <summary>
    /// wrapper touch data class
    /// </summary>
    public class TouchData
    {
        public int FingerId { get; set; } = -1;
        public Vector3 Position { get; set; }

        public static TouchData From(Touch touch) { return new TouchData {Position = touch.position, FingerId = touch.fingerId}; }
    }
}