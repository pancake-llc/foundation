using UnityEngine;

namespace Pancake.Core
{
    public partial struct Interpolator
    {
        public static EaseDelegate Get(Ease ease)
        {
            EaseDelegate result;

            switch (ease)
            {
                case Ease.Linear:
                    result = Linear;
                    break;
                case Ease.InSine:
                    result = InSine;
                    break;
                case Ease.OutSine:
                    result = OutSine;
                    break;
                case Ease.InOutSine:
                    result = InOutSine;
                    break;
                case Ease.InQuad:
                    result = InQuad;
                    break;
                case Ease.OutQuad:
                    result = OutQuad;
                    break;
                case Ease.InOutQuad:
                    result = InOutQuad;
                    break;
                case Ease.InCubic:
                    result = InCubic;
                    break;
                case Ease.OutCubic:
                    result = OutCubic;
                    break;
                case Ease.InOutCubic:
                    result = InOutCubic;
                    break;
                case Ease.InQuart:
                    result = InQuart;
                    break;
                case Ease.OutQuart:
                    result = OutQuart;
                    break;
                case Ease.InOutQuart:
                    result = InOutQuart;
                    break;
                case Ease.InQuint:
                    result = InQuint;
                    break;
                case Ease.OutQuint:
                    result = OutQuint;
                    break;
                case Ease.InOutQuint:
                    result = InOutQuint;
                    break;
                case Ease.InExpo:
                    result = InExpo;
                    break;
                case Ease.OutExpo:
                    result = OutExpo;
                    break;
                case Ease.InOutExpo:
                    result = InOutExpo;
                    break;
                case Ease.InCirc:
                    result = InCirc;
                    break;
                case Ease.OutCirc:
                    result = OutCirc;
                    break;
                case Ease.InOutCirc:
                    result = InOutCirc;
                    break;
                case Ease.InBack:
                    result = InBack;
                    break;
                case Ease.OutBack:
                    result = OutBack;
                    break;
                case Ease.InOutBack:
                    result = InOutBack;
                    break;
                case Ease.InElastic:
                    result = InElastic;
                    break;
                case Ease.OutElastic:
                    result = OutElastic;
                    break;
                case Ease.InOutElastic:
                    result = InOutElastic;
                    break;
                case Ease.InBounce:
                    result = InBounce;
                    break;
                case Ease.OutBounce:
                    result = OutBounce;
                    break;
                case Ease.InOutBounce:
                    result = InOutBounce;
                    break;
                case Ease.Accelerate:
                    result = Accelerate;
                    break;
                case Ease.Decelerate:
                    result = Decelerate;
                    break;
                case Ease.AccelerateDecelerate:
                    result = AccelerateDecelerate;
                    break;
                case Ease.Anticipate:
                    result = Anticipate;
                    break;
                case Ease.Overshoot:
                    result = Overshoot;
                    break;
                case Ease.AnticipateOvershoot:
                    result = AnticipateOvershoot;
                    break;
                case Ease.Bounce:
                    result = Bounce;
                    break;
                case Ease.Parabolic:
                    result = Parabolic;
                    break;
                case Ease.Sine:
                    result = Sine;
                    break;
                case Ease.CustomCurve:
                default:
                    result = Linear;
                    break;
            }

            return result;
        }

        public static EaseDelegate Get(AnimationCurve animationCurve)
        {
            float Result(float a, float b, float t)
            {
                float factor = animationCurve.Evaluate(t);
                return Interpolate(a, b, factor);
            }

            return Result;
        }

        private static float Linear(float a, float b, float t) => Interpolate(a, b, Linear(t));
        private static float InSine(float a, float b, float t) => Interpolate(a, b, InSine(t));
        private static float OutSine(float a, float b, float t) => Interpolate(a, b, OutSine(t));
        private static float InOutSine(float a, float b, float t) => Interpolate(a, b, InOutSine(t));
        private static float InQuad(float a, float b, float t) => Interpolate(a, b, InQuad(t));
        private static float OutQuad(float a, float b, float t) => Interpolate(a, b, OutQuad(t));
        private static float InOutQuad(float a, float b, float t) => Interpolate(a, b, InOutQuad(t));
        private static float InCubic(float a, float b, float t) => Interpolate(a, b, InCubic(t));
        private static float OutCubic(float a, float b, float t) => Interpolate(a, b, OutCubic(t));
        private static float InOutCubic(float a, float b, float t) => Interpolate(a, b, InOutCubic(t));
        private static float InQuart(float a, float b, float t) => Interpolate(a, b, InQuart(t));
        private static float OutQuart(float a, float b, float t) => Interpolate(a, b, OutQuart(t));
        private static float InOutQuart(float a, float b, float t) => Interpolate(a, b, InOutQuart(t));
        private static float InQuint(float a, float b, float t) => Interpolate(a, b, InQuint(t));
        private static float OutQuint(float a, float b, float t) => Interpolate(a, b, OutQuint(t));
        private static float InOutQuint(float a, float b, float t) => Interpolate(a, b, InOutQuint(t));
        private static float InExpo(float a, float b, float t) => Interpolate(a, b, InExpo(t));
        private static float OutExpo(float a, float b, float t) => Interpolate(a, b, OutExpo(t));
        private static float InOutExpo(float a, float b, float t) => Interpolate(a, b, InOutExpo(t));
        private static float InCirc(float a, float b, float t) => Interpolate(a, b, InCirc(t));
        private static float OutCirc(float a, float b, float t) => Interpolate(a, b, OutCirc(t));
        private static float InOutCirc(float a, float b, float t) => Interpolate(a, b, InOutCirc(t));
        private static float InBack(float a, float b, float t) => Interpolate(a, b, InBack(t));
        private static float OutBack(float a, float b, float t) => Interpolate(a, b, OutBack(t));
        private static float InOutBack(float a, float b, float t) => Interpolate(a, b, InOutBack(t));
        private static float InElastic(float a, float b, float t) => Interpolate(a, b, InElastic(t));
        private static float OutElastic(float a, float b, float t) => Interpolate(a, b, OutElastic(t));
        private static float InOutElastic(float a, float b, float t) => Interpolate(a, b, InOutElastic(t));
        private static float InBounce(float a, float b, float t) => Interpolate(a, b, InBounce(t));
        private static float OutBounce(float a, float b, float t) => Interpolate(a, b, OutBounce(t));
        private static float InOutBounce(float a, float b, float t) => Interpolate(a, b, InOutBounce(t));
        private static float Accelerate(float a, float b, float t) => Interpolate(a, b, Linear(t));
        private static float Decelerate(float a, float b, float t) => Interpolate(a, b, Decelerate(t));
        private static float AccelerateDecelerate(float a, float b, float t) => Interpolate(a, b, AccelerateDecelerate(t));
        private static float Anticipate(float a, float b, float t) => Interpolate(a, b, Anticipate(t));
        private static float Overshoot(float a, float b, float t) => Interpolate(a, b, Overshoot(t));
        private static float AnticipateOvershoot(float a, float b, float t) => Interpolate(a, b, AnticipateOvershoot(t));
        private static float Bounce(float a, float b, float t) => Interpolate(a, b, Bounce(t));
        private static float Parabolic(float a, float b, float t) => Interpolate(a, b, Parabolic(t));
        private static float Sine(float a, float b, float t) => Interpolate(a, b, Sine(t));


        private static float Interpolate(float from, float to, float factor) { return (to - from) * factor + from; }
    }
}