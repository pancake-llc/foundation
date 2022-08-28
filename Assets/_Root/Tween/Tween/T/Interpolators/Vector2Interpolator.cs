using System;
using UnityEngine;

namespace Pancake.Core.Tween
{
    public class Vector2Interpolator : IInterpolator<Vector2>
    {
        public static readonly Vector2Interpolator Instance = new Vector2Interpolator();

        private Vector2Interpolator() { }

        public Vector2 Evaluate(Vector2 initialValue, Vector2 finalValue, float time, EaseDelegate interpolator)
        {
            if (interpolator == null)
            {
                throw new ArgumentNullException($"Tried to Evaluate with a " + $"null {nameof(EaseDelegate)} on {nameof(Vector2Interpolator)}");
            }

            return new Vector2(interpolator(initialValue.x, finalValue.x, time), interpolator(initialValue.y, finalValue.y, time));
        }

        public Vector2 Subtract(Vector2 initialValue, Vector2 finalValue) { return finalValue - initialValue; }

        public Vector2 Add(Vector2 initialValue, Vector2 finalValue) { return finalValue + initialValue; }
    }
}