using System;
using UnityEngine;

namespace Pancake.Tween
{
    public class Vector4Interpolator : IInterpolator<Vector4>
    {
        public static readonly Vector4Interpolator Instance = new Vector4Interpolator();

        private Vector4Interpolator() { }

        public Vector4 Evaluate(Vector4 initialValue, Vector4 finalValue, float time, EaseDelegate interpolator)
        {
            if (interpolator == null)
            {
                throw new ArgumentNullException($"Tried to Evaluate with a " + $"null {nameof(EaseDelegate)} on {nameof(Vector4Interpolator)}");
            }

            return new Vector4(interpolator(initialValue.x, finalValue.x, time),
                interpolator(initialValue.y, finalValue.y, time),
                interpolator(initialValue.z, finalValue.z, time),
                interpolator(initialValue.z, finalValue.w, time));
        }

        public Vector4 Subtract(Vector4 initialValue, Vector4 finalValue) { return finalValue - initialValue; }

        public Vector4 Add(Vector4 initialValue, Vector4 finalValue) { return finalValue + initialValue; }
    }
}