using UnityEngine;

namespace Pancake.Core.Tween
{
    internal static class AngleUtils
    {
        public static float Clamp360(float eulerAngles)
        {
            float result = eulerAngles - Mathf.CeilToInt(eulerAngles / 360f) * 360f;

            if (result < 0)
            {
                result += 360f;
            }

            return result;
        }

        public static Vector3 Clamp360(Vector3 eulerAngles) { return new Vector3(Clamp360(eulerAngles.x), Clamp360(eulerAngles.y), Clamp360(eulerAngles.z)); }

        public static Vector3 DeltaAngle(Vector3 current, Vector3 target)
        {
            return new Vector3(Mathf.DeltaAngle(current.x, target.x), Mathf.DeltaAngle(current.y, target.y), Mathf.DeltaAngle(current.z, target.z));
        }

        public static float GetDestinationAngle(float origin, float destination, RotationMode mode)
        {
            switch (mode)
            {
                case RotationMode.Fast:
                {
                    float clampedOrigin = Clamp360(origin);
                    float clampedDestination = Clamp360(destination);

                    float deltaAngle = Mathf.DeltaAngle(clampedOrigin, clampedDestination);

                    return origin + deltaAngle;
                }

                default:
                case RotationMode.Beyond360:
                {
                    return destination;
                }
            }
        }

        public static Vector3 GetDestinationAngle(Vector3 origin, Vector3 destination, RotationMode mode)
        {
            switch (mode)
            {
                case RotationMode.Fast:
                {
                    Vector3 clampedOrigin = Clamp360(origin);
                    Vector3 clampedDestination = Clamp360(destination);

                    Vector3 deltaAngle = DeltaAngle(clampedOrigin, clampedDestination);

                    return origin + deltaAngle;
                }

                default:
                case RotationMode.Beyond360:
                {
                    return destination;
                }
            }
        }
    }
}