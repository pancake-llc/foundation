using System;
using UnityEngine;

namespace Pancake.Core
{
    public class RectInterpolator : IInterpolator<Rect>
    {
        public static readonly RectInterpolator Instance = new RectInterpolator();

        private RectInterpolator() { }

        public Rect Evaluate(Rect initialValue, Rect finalValue, float time, EaseDelegate interpolator)
        {
            if (interpolator == null)
            {
                throw new ArgumentNullException($"Tried to Evaluate with a " + $"null {nameof(EaseDelegate)} on {nameof(Rect)}");
            }

            return new Rect(interpolator(initialValue.x, finalValue.x, time),
                interpolator(initialValue.y, finalValue.y, time),
                interpolator(initialValue.width, finalValue.width, time),
                interpolator(initialValue.height, finalValue.height, time));
        }

        public Rect Subtract(Rect initialValue, Rect finalValue)
        {
            return new Rect(finalValue.x - initialValue.x, finalValue.y - initialValue.y, finalValue.width - initialValue.width, finalValue.height - initialValue.height);
        }

        public Rect Add(Rect initialValue, Rect finalValue)
        {
            return new Rect(finalValue.x + initialValue.x, finalValue.y + initialValue.y, finalValue.width + initialValue.width, finalValue.height + initialValue.height);
        }
    }
}