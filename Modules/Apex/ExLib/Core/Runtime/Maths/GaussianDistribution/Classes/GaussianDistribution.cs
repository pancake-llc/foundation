using UnityEngine;

namespace Pancake.ExLib.Maths
{
    public sealed class GaussianDistribution
    {
        // Stored required properties.
        private float result;
        private bool nextResultReady;

        public float Next()
        {
            float result;

            if (nextResultReady)
            {
                result = this.result;
                nextResultReady = false;
            }
            else
            {
                float s = -1f, x, y;

                do
                {
                    x = 2f * Random.value - 1f;
                    y = 2f * Random.value - 1f;
                    s = x * x + y * y;
                } while (s < 0f || s >= 1f);

                s = Mathf.Sqrt((-2f * Mathf.Log(s)) / s);

                this.result = y * s;
                nextResultReady = true;

                result = x * s;
            }

            return result;
        }

        public float Next(float mean, float sigma = 1f) { return mean + sigma * Next(); }

        public float Next(float mean, float sigma, float min, float max)
        {
            float x = min - 1f;
            while (x < min || x > max)
            {
                x = Next(mean, sigma);
            }

            return x;
        }
    }
}