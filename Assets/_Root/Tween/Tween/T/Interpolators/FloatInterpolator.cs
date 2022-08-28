using System;

namespace Pancake.Core.Tween
{
    public class FloatInterpolator : IInterpolator<float>
    {
        public static readonly FloatInterpolator Instance = new FloatInterpolator();

        private FloatInterpolator() { }

        public float Evaluate(float initialValue, float finalValue, float time, EaseDelegate interpolator)
        {
            if (interpolator == null)
            {
                throw new ArgumentNullException($"Tried to Evaluate with a " + $"null {nameof(EaseDelegate)} on {nameof(FloatInterpolator)}");
            }

            return interpolator(initialValue, finalValue, time);
        }

        public float Subtract(float initialValue, float finalValue) { return finalValue - initialValue; }

        public float Add(float initialValue, float finalValue) { return finalValue + initialValue; }
    }
}