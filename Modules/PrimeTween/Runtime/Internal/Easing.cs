// Credits: http://robertpenner.com/easing/
// Copyright © 2001 Robert Penner
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

// ReSharper disable PossibleNullReferenceException
// ReSharper disable CompareOfFloatsByEqualityOperator
using JetBrains.Annotations;
using UnityEngine;

namespace PrimeTween {
    [PublicAPI]
    internal static class Easing {
        const float halfPi = Mathf.PI / 2f;
        const float twoPi = Mathf.PI * 2f;

        public static float InSine(float t) {
            return 1 - Mathf.Cos(t * halfPi);
        }

        public static float OutSine(float t) {
            return Mathf.Sin(t * halfPi);
        }

        public static float InOutSine(float t) {
            return -0.5f * (Mathf.Cos(Mathf.PI * t) - 1);
        }

        public static float InQuad(float t) {
            return t * t;
        }

        public static float OutQuad(float t) {
            return -t * (t - 2);
        }

        public static float InOutQuad(float t) {
            t *= 2f;
            if (t < 1) {
                return 0.5f * t * t;
            }
            return -0.5f * (--t * (t - 2) - 1);
        }

        public static float InCubic(float t) {
            return t * t * t;
        }

        public static float OutCubic(float t) {
            return (t -= 1) * t * t + 1;
        }

        public static float InOutCubic(float t) {
            t *= 2f;
            if (t < 1) {
                return 0.5f * t * t * t;
            }
            return 0.5f * ((t -= 2) * t * t + 2);
        }

        public static float InQuart(float t) {
            return t * t * t * t;
        }

        public static float OutQuart(float t) {
            return -((t -= 1) * t * t * t - 1);
        }

        public static float InOutQuart(float t) {
            t *= 2f;
            if (t < 1) {
                return 0.5f * t * t * t * t;
            }
            return -0.5f * ((t -= 2) * t * t * t - 2);
        }

        public static float InQuint(float t) {
            return t * t * t * t * t;
        }

        public static float OutQuint(float t) {
            return (t -= 1) * t * t * t * t + 1;
        }

        public static float InOutQuint(float t) {
            t *= 2f;
            if (t < 1) {
                return 0.5f * t * t * t * t * t;
            }
            return 0.5f * ((t -= 2) * t * t * t * t + 2);
        }

        public static float InExpo(float x) {
            return x == 0 ? 0 : Mathf.Pow(2, 10 * (x - 1));
        }

        public static float OutExpo(float t) {
            if (t == 1) {
                return 1;
            }
            return -Mathf.Pow(2, -10 * t) + 1;
        }

        public static float InOutExpo(float t) {
            if (t == 0) {
                return 0;
            }
            if (t == 1) {
                return 1;
            }
            t *= 2f;
            if (t < 1) {
                return 0.5f * Mathf.Pow(2, 10 * (t - 1));
            }
            return 0.5f * (-Mathf.Pow(2, -10 * --t) + 2);
        }

        public static float InCirc(float t) {
            return -(Mathf.Sqrt(1 - t * t) - 1);
        }

        public static float OutCirc(float t) {
            return Mathf.Sqrt(1 - (t -= 1) * t);
        }

        public static float InOutCirc(float t) {
            t *= 2f;
            if (t < 1) {
                return -0.5f * (Mathf.Sqrt(1 - t * t) - 1);
            }
            return 0.5f * (Mathf.Sqrt(1 - (t -= 2) * t) + 1);
        }
        
        const float backEaseConst = 1.70158f;

        public static float InBack(float t) {
            return t * t * ((backEaseConst + 1) * t - backEaseConst);
        }

        public static float OutBack(float t) {
            return (t -= 1) * t * ((backEaseConst + 1) * t + backEaseConst) + 1;
        }

        public static float InOutBack(float t) {
            t *= 2f;
            const float c1 = backEaseConst * 1.525f;
            if (t < 1) {
                return 0.5f * (t * t * ((c1 + 1) * t - c1));
            }
            return 0.5f * ((t -= 2) * t * ((c1 + 1) * t + c1) + 2);
        }
        
        const float elasticEasePeriod = 0.3f;
        const float elasticEaseConst = 0.02999433f; // elasticEasePeriod / twoPi * Mathf.Asin(1 / c);

        public static float InElastic(float t) {
            switch (t) {
                case 0:
                    return 0;
                case 1:
                    return 1;
                default:
                    return -(backEaseConst * Mathf.Pow(2, 10 * (t -= 1)) * Mathf.Sin((t - elasticEaseConst) * twoPi / elasticEasePeriod));
            }
        }

        public static float OutElastic(float t) {
            switch (t) {
                case 0:
                    return 0;
                case 1:
                    return 1;
                default:
                    return backEaseConst * Mathf.Pow(2, -10 * t) * Mathf.Sin((t - elasticEaseConst) * twoPi / elasticEasePeriod) + 1;
            }
        }

        public static float InOutElastic(float t) {
            if (t == 0) {
                return 0;
            }
            if (t == 1) {
                return 1;
            }
            t *= 2;
            const float p = 0.3f * 1.5f;
            const float s = p / 4;
            if (t < 1) {
                return -0.5f * (Mathf.Pow(2, 10 * (t -= 1)) * Mathf.Sin((t - s) * twoPi / p));
            }
            return Mathf.Pow(2, -10 * (t -= 1)) * Mathf.Sin((t - s) * twoPi / p) * 0.5f + 1;
        }

        public static float InBounce(float x) {
            return 1 - OutBounce(1 - x);
        }

        public static float OutBounce(float x) {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;
            if (x < 1 / d1) {
                return n1 * x * x;
            }
            if (x < 2 / d1) {
                return n1 * (x -= 1.5f / d1) * x + 0.75f;
            }
            if (x < 2.5 / d1) {
                return n1 * (x -= 2.25f / d1) * x + 0.9375f;
            }
            return n1 * (x -= 2.625f / d1) * x + 0.984375f;
        }

        public static float InOutBounce(float x) {
            return x < 0.5
                ? (1 - OutBounce(1 - 2 * x)) / 2
                : (1 + OutBounce(2 * x - 1)) / 2;
        }

        public static float Evaluate(float t, Ease ease, [CanBeNull] AnimationCurve customEase = null) {
            switch (ease) {
                case Ease.Custom:
                    Assert.IsNotNull(customEase);
                    return customEase.Evaluate(t);
                case Ease.Linear:
                    return t;
                case Ease.InSine:
                    return InSine(t);
                case Ease.OutSine:
                    return OutSine(t);
                case Ease.InOutSine:
                    return InOutSine(t);
                case Ease.InQuad:
                    return InQuad(t);
                case Ease.OutQuad:
                    return OutQuad(t);
                case Ease.InOutQuad:
                    return InOutQuad(t);
                case Ease.InCubic:
                    return InCubic(t);
                case Ease.OutCubic:
                    return OutCubic(t);
                case Ease.InOutCubic:
                    return InOutCubic(t);
                case Ease.InQuart:
                    return InQuart(t);
                case Ease.OutQuart:
                    return OutQuart(t);
                case Ease.InOutQuart:
                    return InOutQuart(t);
                case Ease.InQuint:
                    return InQuint(t);
                case Ease.OutQuint:
                    return OutQuint(t);
                case Ease.InOutQuint:
                    return InOutQuint(t);
                case Ease.InExpo:
                    return InExpo(t);
                case Ease.OutExpo:
                    return OutExpo(t);
                case Ease.InOutExpo:
                    return InOutExpo(t);
                case Ease.InCirc:
                    return InCirc(t);
                case Ease.OutCirc:
                    return OutCirc(t);
                case Ease.InOutCirc:
                    return InOutCirc(t);
                case Ease.InBack:
                    return InBack(t);
                case Ease.OutBack:
                    return OutBack(t);
                case Ease.InOutBack:
                    return InOutBack(t);
                case Ease.InElastic:
                    return InElastic(t);
                case Ease.OutElastic:
                    return OutElastic(t);
                case Ease.InOutElastic:
                    return InOutElastic(t);
                case Ease.InBounce:
                    return InBounce(t);
                case Ease.OutBounce:
                    return OutBounce(t);
                case Ease.InOutBounce:
                    return InOutBounce(t);
                case Ease.Default:
                default:
                    throw new System.Exception();
            }
        }
    }
}