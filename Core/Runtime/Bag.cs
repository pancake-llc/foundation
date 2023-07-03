using System;
using Random = UnityEngine.Random;

namespace Pancake
{
    public class Bag
    {
        private readonly float[] _weights;
        private readonly float[] _cumulativeWeights;
        private readonly float _totalWeight;

        public Bag(float[] weights)
        {
            _weights = weights;
            _cumulativeWeights = new float[weights.Length];
            for (int i = 0; i < weights.Length; i++)
            {
                _totalWeight += weights[i];
                _cumulativeWeights[i] = _totalWeight;
            }
        }

        /// <summary>
        /// Return index of element was choosed
        /// </summary>
        /// <returns></returns>
        public int Take()
        {
            var point = Random.Range(0f, _totalWeight);
            var lo = 0;
            var hi = _weights.Length - 1;

            while (lo < hi)
            {
                var mid = (int) (((uint) hi + (uint) lo) >> 1);
                var value = _cumulativeWeights[mid];

                if (point > value)
                {
                    lo = mid + 1;
                }
                else
                {
                    var prevPoint = mid > 0 ? _cumulativeWeights[mid - 1] : 0;

                    if (point >= prevPoint)
                    {
                        return mid;
                    }

                    hi = mid;
                }
            }

            return _weights.Length - 1;
        }
    }

    public class Bag<T>
    {
        private readonly Bag _bag;

        public Bag(T[] sources, Func<T, float> selector)
        {
            float[] weights = new float[sources.Length];
            for (int i = 0; i < sources.Length; i++)
            {
                weights[i] = selector.Invoke(sources[i]);
            }

            _bag = new Bag(weights);
        }

        /// <summary>
        /// Return index of element was choosed
        /// </summary>
        /// <returns></returns>
        public int Take() { return _bag.Take(); }
    }
}