using JetBrains.Annotations;
using UnityEngine;

namespace PrimeTween {
    [PublicAPI]
    public readonly struct Easing {
        internal readonly Ease ease;
        internal readonly AnimationCurve curve;
        internal readonly ParametricEaseType parametricEaseType;
        internal readonly float parametricEaseStrength;
        internal readonly float parametricEasePeriod;

        Easing(ParametricEaseType type, float strength, float period = float.NaN) {
            ease = Ease.Custom;
            curve = null;
            parametricEaseType = type;
            parametricEaseStrength = strength;
            parametricEasePeriod = period;
        }

        Easing(Ease ease, AnimationCurve curve) {
            this.ease = ease;
            this.curve = curve;
            parametricEaseType = ParametricEaseType.None;
            parametricEaseStrength = float.NaN;
            parametricEasePeriod = float.NaN;
        }
        
        public static implicit operator Easing(Ease ease) => Standard(ease);
        /// <summary>Standard Robert Penner's easing methods. Or simply use Ease enum instead of this method.</summary>
        public static Easing Standard(Ease ease) => new Easing(ease, null);
        public static implicit operator Easing([NotNull] AnimationCurve curve) => Curve(curve);
        /// <summary>AnimationCurve to use an easing function. Or simply use AnimationCurve instead of this method.</summary>
        public static Easing Curve([NotNull] AnimationCurve curve) => new Easing(Ease.Custom, curve);
        
        #if PRIME_TWEEN_EXPERIMENTAL
        public static Easing Overshoot(float strength) => new(ParametricEaseType.Overshoot, strength * StandardEasing.backEaseConst);
        public static Easing Elastic(float strength, float normalizedPeriod = 0.3f) {
            if (strength < 1) {
                strength = Mathf.Lerp(0.2f, 1f, strength); // remap strength to limit decayFactor
            }
            return new Easing(ParametricEaseType.Elastic, strength, Mathf.Max(0.01f, normalizedPeriod));
        }
        #endif

        internal static float Evaluate(float t, ParametricEaseType type, float strength, float period) {
            switch (type) {
                case ParametricEaseType.Overshoot:
                    t -= 1.0f;
                    return t * t * ((strength + 1) * t + strength) + 1.0f;
                case ParametricEaseType.Elastic:
                    const float twoPi = 2 * Mathf.PI;
                    float decayFactor;
                    if (strength >= 1) {
                        decayFactor = 1f;
                    } else {
                        decayFactor = 1 / strength;
                        strength = 1;
                    }
                    float decay = Mathf.Pow(2, -10f * t * decayFactor);
                    float phase = period / twoPi * Mathf.Asin(1f / strength);
                    return t > 0.9999f ? 1 : strength * decay * Mathf.Sin((t - phase) * twoPi / period) + 1;
                case ParametricEaseType.None:
                default:
                    throw new System.Exception();
            }
        }
    }

    internal enum ParametricEaseType {
        None = 0,
        Overshoot = 5,
        Elastic = 11,
    }
}