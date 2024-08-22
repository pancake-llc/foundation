using UnityEngine;

namespace Pancake.MobileInput
{
    /// <summary>
    /// Wrapper touch data from <see cref="Touch"/>
    /// </summary>
    public class TouchData
    {
        public Vector3 Position { get; set; }
        public int FingerId { get; set; } = -1;

        public static TouchData From(Touch touch) => new() {Position = touch.position, FingerId = touch.fingerId};
    }
}