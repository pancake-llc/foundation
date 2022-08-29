using System;
using UnityEngine;

namespace Pancake.Tween
{
    public class ColorInterpolator : IInterpolator<Color>
    {
        public static readonly ColorInterpolator Instance = new ColorInterpolator();

        private ColorInterpolator() { }

        public Color Evaluate(Color initialValue, Color finalValue, float time, EaseDelegate interpolator)
        {
            if (interpolator == null)
            {
                throw new ArgumentNullException($"Tried to Evaluate with a " + $"null {nameof(EaseDelegate)} on {nameof(ColorInterpolator)}");
            }

            return new Color(interpolator(initialValue.r, finalValue.r, time),
                interpolator(initialValue.g, finalValue.g, time),
                interpolator(initialValue.b, finalValue.b, time),
                interpolator(initialValue.a, finalValue.a, time));
        }

        public Color Subtract(Color initialValue, Color finalValue) { return finalValue - initialValue; }

        public Color Add(Color initialValue, Color finalValue) { return finalValue + initialValue; }
    }
}