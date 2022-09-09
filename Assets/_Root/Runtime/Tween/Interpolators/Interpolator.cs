using System;
using UnityEngine;

namespace Pancake.Tween
{
    /// <summary>
    /// Interpolator
    /// </summary>
    [Serializable]
    public partial struct Interpolator
    {
        public Ease ease;
        [Range(0, 1)] public float strength;
        public AnimationCurve customCurve;

        internal static readonly Func<float, float, float>[] Interpolators =
        {
            (t, s) => t, (t, s) => InSine(t), (t, s) => OutSine(t), (t, s) => InOutSine(t), (t, s) => InQuad(t), // Accelerate
            (t, s) => OutQuad(t), (t, s) => InOutQuad(t), (t, s) => InCubic(t), (t, s) => OutCubic(t), (t, s) => InOutCubic(t), (t, s) => InQuart(t),
            (t, s) => OutQuart(t), (t, s) => InOutQuart(t), (t, s) => InQuint(t), (t, s) => OutQuint(t), (t, s) => InOutQuint(t), (t, s) => InExpo(t),
            (t, s) => OutExpo(t), (t, s) => InOutExpo(t), (t, s) => InCirc(t), (t, s) => OutCirc(t), (t, s) => InOutCirc(t), (t, s) => InBack(t), (t, s) => OutBack(t),
            (t, s) => InOutBack(t), (t, s) => InElastic(t), (t, s) => OutElastic(t), (t, s) => InOutElastic(t), (t, s) => InBounce(t), (t, s) => OutBounce(t),
            (t, s) => InOutBounce(t), Accelerate, Decelerate, AccelerateDecelerate, Anticipate, Overshoot, AnticipateOvershoot, Bounce, (t, s) => Parabolic(t),
            (t, s) => Sine(t)
        };


        /// <summary>
        /// Calculate interpolation value
        /// </summary>
        /// <param name="t"> normalized time </param>
        /// <returns> result </returns>
        public float this[float t] => ease == Ease.CustomCurve ? customCurve.Evaluate(t) : Interpolators[(int) ease](t, strength);


        public Interpolator(Ease ease, float strength = 0.5f, AnimationCurve customCurve = null)
        {
            this.ease = ease;
            this.strength = strength;
            this.customCurve = customCurve;
        }
    } // struct Interpolator
} // namespace Pancake.Core