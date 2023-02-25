namespace Pancake.Tween
{
    /// <summary>
    /// Predefined Interpolators
    /// </summary>
    public partial struct Interpolator
    {
        private static float s = 1.70158f;
        private static float s2 = 2.5949095f;

        /// <summary>
        /// Linear interpolation
        /// </summary>
        /// <param name="t"> Unitized time, i.e. a value in the range [0, 1] </param>
        /// <returns> Interpolation result </returns>
        public static float Linear(float t) { return t; }

        public static float InSine(float t) { return 1f - (((1f - t) * Math.PI) / 2f).Sin(); }

        public static float OutSine(float t) { return ((t * Math.PI) / 2f).Sin(); }

        public static float InOutSine(float t) { return 0.5f * (1f - (Math.PI * (0.5f - t)).Sin()); }

        /// <summary>
        /// Speed up interpolation (a.k.a Accelerate)
        /// </summary>
        /// <param name="t"> Unitized time, i.e. a value in the range [0, 1] </param>
        /// <returns> Interpolation result </returns>
        public static float InQuad(float t) { return t * t; }

        public static float OutQuad(float t) { return t * (2f - t); }

        public static float InOutQuad(float t)
        {
            if ((t *= 2f) < 1f)
            {
                return 0.5f * t * t;
            }

            return -0.5f * (--t * (t - 2f) - 1f);
        }

        public static float InCubic(float t) { return t * t * t; }

        public static float OutCubic(float t) { return --t * t * t + 1f; }

        public static float InOutCubic(float t)
        {
            if ((t *= 2f) < 1f)
            {
                return 0.5f * t * t * t;
            }

            return 0.5f * ((t -= 2) * t * t + 2f);
        }

        public static float InQuart(float t) { return t * t * t * t; }

        public static float OutQuart(float t) { return 1f - --t * t * t * t; }

        public static float InOutQuart(float t)
        {
            if ((t *= 2f) < 1f)
            {
                return 0.5f * t * t * t * t;
            }

            return -0.5f * ((t -= 2f) * t * t * t - 2f);
        }

        public static float InQuint(float t) { return t * t * t * t * t; }

        public static float OutQuint(float t) { return --t * t * t * t * t + 1f; }

        public static float InOutQuint(float t)
        {
            if ((t *= 2f) < 1f)
            {
                return 0.5f * t * t * t * t * t;
            }

            return 0.5f * ((t -= 2f) * t * t * t * t + 2f);
        }

        public static float InExpo(float t) { return t.Approximately(0f) ? 0f : 1024f.Pow(t - 1f); }

        public static float OutExpo(float t) { return t.Approximately(1f) ? 1f : 1f - 2f.Pow(-10f * t); }

        public static float InOutExpo(float t)
        {
            if (t.Approximately(0f) || t.Approximately(1f)) return t;

            if ((t *= 2) < 1)
            {
                return 0.5f * Math.Pow(1024, t - 1);
            }

            return 0.5f * (-Math.Pow(2, -10 * (t - 1)) + 2);
        }

        public static float InCirc(float t) { return 1f - (1f - t * t).Sqrt(); }
        public static float OutCirc(float t) { return (1f - --t * t).Sqrt(); }

        public static float InOutCirc(float t)
        {
            if ((t *= 2f) < 1f)
            {
                return -0.5f * ((1f - t * t).Sqrt() - 1f);
            }

            return 0.5f * ((1f - (t -= 2f) * t).Sqrt() + 1f);
        }

        public static float InBack(float t) { return t.Approximately(1f) ? 1f : t * t * ((s + 1f) * t - s); }

        public static float OutBack(float t) { return t.Approximately(0f) ? 0f : --t * t * ((s + 1f) * t + s) + 1f; }

        public static float InOutBack(float t)
        {
            if ((t *= 2f) < 1f)
            {
                return 0.5f * (t * t * ((s2 + 1f) * t - s2));
            }

            return 0.5f * ((t -= 2f) * t * ((s2 + 1f) * t + s2) + 2f);
        }

        public static float InElastic(float t)
        {
            if (t.Approximately(0f) || t.Approximately(1f)) return t;
            return -2f.Pow(10f * (t - 1)) * ((t - 1.1f) * 5f * Math.PI).Sin();
        }

        public static float OutElastic(float t)
        {
            if (t.Approximately(0f) || t.Approximately(1f)) return t;
            return 2f.Pow(-10f * t) * ((t - 0.1f) * 5f * Math.PI).Sin() + 1f;
        }

        public static float InOutElastic(float t)
        {
            if (t.Approximately(0f) || t.Approximately(1f)) return t;

            t *= 2;

            if (t < 1)
            {
                return -0.5f * 2f.Pow(10f * (t - 1f)) * ((t - 1.1f) * 5f * Math.PI).Sin();
            }

            return 0.5f * 2f.Pow(-10f * (t - 1f)) * ((t - 1.1f) * 5f * Math.PI).Sin() + 1f;
        }

        public static float InBounce(float t) { return 1f - OutBounce(1f - t); }

        public static float OutBounce(float t)
        {
            if (t < 1f / 2.75f)
            {
                return 7.5625f * t * t;
            }

            if (t < 2f / 2.75f)
            {
                return 7.5625f * (t -= 1.5f / 2.75f) * t + 0.75f;
            }

            if (t < 2.5f / 2.75f)
            {
                return 7.5625f * (t -= 2.25f / 2.75f) * t + 0.9375f;
            }

            return 7.5625f * (t -= 2.625f / 2.75f) * t + 0.984375f;
        }

        public static float InOutBounce(float t)
        {
            if (t < 0.5f)
            {
                return InBounce(t * 2f) * 0.5f;
            }

            return OutBounce(t * 2f - 1f) * 0.5f + 0.5f;
        }


        /// <summary>
        /// Speed up interpolation Weakly
        /// </summary>
        /// <param name="t">  Unitized time, which is a value in the range [0, 1] </param>
        /// <returns> Interpolation result </returns>
        public static float AccelerateWeakly(float t) { return t * t * (2f - t); }


        /// <summary>
        /// Accelerated Interpolation Strongly
        /// </summary>
        /// <param name="t"> Unitized time, i.e. a value in the range [0, 1] </param>
        /// <returns> Interpolation result </returns>
        public static float AccelerateStrongly(float t) { return t * t * t; }


        /// <summary>
        /// Speed up interpolation
        /// </summary>
        /// <param name="t"> Unitized time, i.e. a value in the range [0, 1] </param>
        /// <param name="strength"> [0, 1] range strength </param>
        /// <returns> Interpolation result </returns>
        public static float Accelerate(float t, float strength) { return t * t * ((2f - t) * (1f - strength) + t * strength); }


        /// <summary>
        /// Deceleration interpolation
        /// </summary>
        /// <param name="t"> Unitized time, i.e. a value in the range [0, 1] </param>
        /// <returns> Interpolation result </returns>
        public static float Decelerate(float t) { return (2f - t) * t; }


        /// <summary>
        /// Deceleration Interpolation Weakly
        /// </summary>
        /// <param name="t"> Unitized time, i.e. a value in the range [0, 1] </param>
        /// <returns> Interpolation result </returns>
        public static float DecelerateWeakly(float t)
        {
            t = 1f - t;
            return 1f - t * t * (2f - t);
        }


        /// <summary>
        /// Deceleration Interpolation Strongly
        /// </summary>
        /// <param name="t"> Unitized time, i.e. a value in the range [0, 1] </param>
        /// <returns> Interpolation result </returns>
        public static float DecelerateStrongly(float t)
        {
            t = 1f - t;
            return 1f - t * t * t;
        }


        /// <summary>
        /// Deceleration interpolation
        /// </summary>
        /// <param name="t"> Unitized time, i.e. a value in the range [0, 1] </param>
        /// <param name="strength"> [0, 1] range strength </param>
        /// <returns> Interpolation result </returns>
        public static float Decelerate(float t, float strength)
        {
            t = 1f - t;
            return 1f - t * t * ((2f - t) * (1f - strength) + t * strength);
        }


        /// <summary>
        /// Accelerate then decelerate interpolation
        /// </summary>
        /// <param name="t"> Unitized time, i.e. a value in the range [0, 1] </param>
        /// <returns> Interpolation result </returns>
        public static float AccelerateDecelerate(float t) { return (3f - t - t) * t * t; }


        /// <summary>
        /// Accelerate then decelerate interpolation Weakly
        /// </summary>
        /// <param name="t"> Unitized time, i.e. a value in the range [0, 1] </param>
        /// <returns> Interpolation result </returns>
        public static float AccelerateDecelerateWeakly(float t)
        {
            float tt = t * t;
            return ((-6f * t + 15f) * tt - 14f * t + 6f) * tt;
        }


        /// <summary>
        /// Accelerate then decelerate interpolation Strongly
        /// </summary>
        /// <param name="t"> Unitized time, i.e. a value in the range [0, 1] </param>
        /// <returns> Interpolation result </returns>
        public static float AccelerateDecelerateStrongly(float t) { return ((6f * t - 15f) * t + 10f) * t * t * t; }


        /// <summary>
        /// Accelerate then decelerate interpolation
        /// </summary>
        /// <param name="t"> Unitized time, i.e. a value in the range [0, 1] </param>
        /// <param name="strength"> [0, 1] range strength </param>
        /// <returns> Interpolation result </returns>
        public static float AccelerateDecelerate(float t, float strength)
        {
            float tt = t * t;
            float k = (6f * t - 15f) * tt;
            return ((6f - k - 14f * t) * (1f - strength) + (k + 10f * t) * strength) * tt;
        }


        /// <summary>
        /// Bounce acceleration interpolation
        /// </summary>
        /// <param name="t"> Unitized time, i.e. a value in the range [0, 1] </param>
        /// <param name="strength"> [0, 1] range strength </param>
        /// <returns> Interpolation result </returns>
        public static float Anticipate(float t, float strength = 0.5f)
        {
            float a = 2f + strength * 2f;
            return (a * t - a + 1f) * t * t;
        }


        /// <summary>
        /// Deceleration bounce interpolation
        /// </summary>
        /// <param name="t"> Unitized time, i.e. a value in the range [0, 1] </param>
        /// <param name="strength"> [0, 1] range strength </param>
        /// <returns> Interpolation result </returns>
        public static float Overshoot(float t, float strength = 0.5f)
        {
            t = 1f - t;
            float a = 2f + strength * 2f;
            return 1f - (a * t - a + 1f) * t * t;
        }


        /// <summary>
        /// First rebound acceleration and then deceleration rebound interpolation
        /// </summary>
        /// <param name="t"> Unitized time, i.e. a value in the range [0, 1] </param>
        /// <param name="strength"> [0, 1] range strength </param>
        /// <returns> Interpolation result </returns>
        public static float AnticipateOvershoot(float t, float strength = 0.5f)
        {
            float d = -6f - 12f * strength;
            return ((((6f - d - d) * t + (5f * d - 15f)) * t + (10f - 4f * d)) * t + d) * t * t;
        }


        /// <summary>
        /// Bounce Interpolation
        /// </summary>
        /// <param name="t"> Unitized time, i.e. a value in the range [0, 1] </param>
        /// <param name="strength"> [0, 1] range strength </param>
        /// <returns> Interpolation result </returns>
        public static float Bounce(float t, float strength = 0.5f)
        {
            float k = 0.3f + 0.4f * strength;
            float kk = k * k;
            float a = 1f + (k + k) * (1f + k + kk);

            float tmp;

            if (t < 1f / a)
            {
                tmp = a * t;
                return tmp * tmp;
            }

            if (t < (1f + k + k) / a)
            {
                tmp = a * t - 1f - k;
                return 1f - kk + tmp * tmp;
            }

            if (t < (1f + (k + kk) * 2f) / a)
            {
                tmp = a * t - 1f - k - k - kk;
                return 1f - kk * kk + tmp * tmp;
            }

            tmp = a * t - 1f - 2 * (k + kk) - kk * k;
            return 1f - kk * kk * kk + tmp * tmp;
        }


        /// <summary>
        /// Parabolic interpolation
        /// </summary>
        /// <param name="t"> Unitized time, i.e. a value in the range [0, 1] </param>
        /// <returns> Interpolation result </returns>
        public static float Parabolic(float t) { return 4f * t * (1f - t); }


        /// <summary>
        /// Sine interpolation
        /// </summary>
        /// <param name="t"> Unitized time, i.e. a value in the range [0, 1] </param>
        /// <returns> Interpolation result </returns>
        public static float Sine(float t) { return ((t + t + 1.5f) * Math.PI).Sin() * 0.5f + 0.5f; }
    } // struct Interpolator
} // namespace Pancake.Core