using System;
using UnityEngine;
using UnityEngine.Events;

namespace Pancake.Sensor
{
    public interface IRayCastingSensor
    {
        RayHit GetDetectionRayHit(GameObject detectedGameObject);

        bool IsObstructed { get; }
        RayHit GetObstructionRayHit();

        // Event fired at the time the sensor is obstructed when before it was unobstructed
        ObstructionEvent OnObstruction { get; }

        // Event fired at the time the sensor is unobstructed when before it was obstructed
        ObstructionEvent OnClear { get; }
    }

    [System.Serializable]
    public class ObstructionEvent : UnityEvent<IRayCastingSensor>
    {
    }

    /**
     * A common representation for ray hits that combines both RaycastHit and
     * RaycastHit2D. This exists to provide some consistency between the RaySensor
     * and RaySensor2D interfaces, and also the Arc Sensors.
     */
    public struct RayHit : IEquatable<RayHit>
    {
        public static RayHit None => new RayHit() {Distance = -1};
        public bool IsObstructing;
        public Vector3 Point;
        public Vector3 Normal;
        public float Distance;
        public float DistanceFraction;

        public Collider Collider;
        public Collider2D Collider2D;

        public GameObject GameObject => Collider != null ? Collider.gameObject : Collider2D != null ? Collider2D.gameObject : null;

        public bool Equals(RayHit other)
        {
            return IsObstructing == other.IsObstructing && Point.Equals(other.Point) && Normal.Equals(other.Normal) && Distance == other.Distance &&
                   DistanceFraction == other.DistanceFraction && Collider == other.Collider && Collider2D == other.Collider2D;
        }
    }
}