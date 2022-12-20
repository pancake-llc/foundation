using System;

namespace Pancake
{
    public class MagicBag
    {
        private readonly float[] _weights;
        private readonly float _totalWeight;

        public MagicBag(float[] weights)
        {
            _weights = weights;
            for (int i = 0; i < weights.Length; i++)
            {
                _totalWeight += weights[i];
            }
        }

        /// <summary>
        /// Return index of element was choosed
        /// </summary>
        /// <returns></returns>
        public int Take()
        {
            var point = Random.Range(0f, _totalWeight);

            var currentWeight = 0f;
            for (int i = 0; i < _weights.Length; i++)
            {
                currentWeight += _weights[i];
                if (point < currentWeight)
                {
                    return i;
                }
            }

            return _weights.Length - 1;
        }
    }

    public class MagicBag<T>
    {
        private readonly MagicBag _bag;

        public MagicBag(T[] sources, Func<T, float> selector)
        {
            float[] weights = new float[sources.Length];
            for (int i = 0; i < sources.Length; i++)
            {
                weights[i] = selector.Invoke(sources[i]);
            }

            _bag = new MagicBag(weights);
        }

        /// <summary>
        /// Return index of element was choosed
        /// </summary>
        /// <returns></returns>
        public int Take() { return _bag.Take(); }
    }
}