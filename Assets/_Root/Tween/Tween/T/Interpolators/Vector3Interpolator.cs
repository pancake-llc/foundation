using System;
using UnityEngine;

namespace Pancake.Tween
{
    public class Vector3Interpolator : IInterpolator<Vector3>
    {
        public static readonly Vector3Interpolator Instance = new Vector3Interpolator();

        private Vector3Interpolator() { }

        public Vector3 Evaluate(Vector3 initialValue, Vector3 finalValue, float time, EaseDelegate interpolator)
        {
            if (interpolator == null)
            {
                throw new ArgumentNullException($"Tried to Evaluate with a " + $"null {nameof(EaseDelegate)} on {nameof(Vector3Interpolator)}");
            }

            return new Vector3(interpolator(initialValue.x, finalValue.x, time),
                interpolator(initialValue.y, finalValue.y, time),
                interpolator(initialValue.z, finalValue.z, time));
        }

        public Vector3 Subtract(Vector3 initialValue, Vector3 finalValue) { return finalValue - initialValue; }

        public Vector3 Add(Vector3 initialValue, Vector3 finalValue) { return finalValue + initialValue; }
    }
}