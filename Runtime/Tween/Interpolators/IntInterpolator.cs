using System;

namespace Pancake.Tween
{
    public class IntInterpolator : IInterpolator<int>
    {
        public static readonly IntInterpolator Instance = new IntInterpolator();

        private IntInterpolator() { }

        public int Evaluate(int initialValue, int finalValue, float time, EaseDelegate interpolator)
        {
            if (interpolator == null)
            {
                throw new ArgumentNullException($"Tried to Evaluate with a " + $"null {nameof(EaseDelegate)} on {nameof(IntInterpolator)}");
            }

            return (int) interpolator(initialValue, finalValue, time);
        }

        public int Subtract(int initialValue, int finalValue) { return finalValue - initialValue; }

        public int Add(int initialValue, int finalValue) { return finalValue + initialValue; }
    }
}