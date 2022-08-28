using System;
using UnityEngine;

namespace Pancake.Core.Tween
{
    public class QuaternionInterpolator : IInterpolator<Quaternion>
    {
        public static readonly QuaternionInterpolator Instance = new QuaternionInterpolator();

        private QuaternionInterpolator() { }

        public Quaternion Evaluate(Quaternion initialValue, Quaternion finalValue, float time, EaseDelegate interpolator)
        {
            if (interpolator == null)
            {
                throw new ArgumentNullException($"Tried to Evaluate with a " + $"null {nameof(EaseDelegate)} on {nameof(ColorInterpolator)}");
            }

            float curveTime = interpolator(0f, 1f, time);

            return Quaternion.Slerp(initialValue, finalValue, curveTime);
        }

        public Quaternion Subtract(Quaternion initialValue, Quaternion finalValue) { return Quaternion.Inverse(initialValue) * finalValue; }

        public Quaternion Add(Quaternion initialValue, Quaternion finalValue) { return initialValue * finalValue; }
    }
}