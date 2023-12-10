using UnityEngine;

namespace Pancake.MobileInput
{
    /// <summary>
    /// Provide pinch data use form touch
    /// </summary>
    public class PinchData
    {
        public Vector3 pinchCenter;
        public float pinchDistance;
        public float pinchStartDistance;
        public float pinchAngleDelta;
        public float pinchAngleDeltaNormalized;
        public float pinchTiltDelta;
        public float pinchTotalFingerMovement;
    }
}