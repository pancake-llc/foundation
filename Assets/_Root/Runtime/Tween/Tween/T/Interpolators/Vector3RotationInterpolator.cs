using System;
using UnityEngine;

namespace Pancake.Tween
{
    public class Vector3RotationInterpolator : IInterpolator<Vector3>
    {
        private readonly RotationMode rotationMode;

        public Vector3RotationInterpolator(RotationMode rotationMode) { this.rotationMode = rotationMode; }

        public Vector3 Evaluate(Vector3 initialValue, Vector3 finalValue, float time, EaseDelegate interpolator)
        {
            if (interpolator == null)
            {
                throw new ArgumentNullException($"Tried to Evaluate with a " + $"null {nameof(EaseDelegate)} on {nameof(Vector3Interpolator)}");
            }

            if (rotationMode == RotationMode.Fast)
            {
                Vector3 deltaAngle = AngleUtils.DeltaAngle(initialValue, finalValue);

                finalValue = initialValue + deltaAngle;
            }

            return new Vector3(interpolator(initialValue.x, finalValue.x, time),
                interpolator(initialValue.y, finalValue.y, time),
                interpolator(initialValue.z, finalValue.z, time));
        }

        public Vector3 Subtract(Vector3 initialValue, Vector3 finalValue) { return finalValue - initialValue; }

        public Vector3 Add(Vector3 firstValue, Vector3 secondValue) { return secondValue + firstValue; }
    }
}