using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Pancake
{
    [Serializable]
    public class BagItem<T>
    {
        public T item;
        [Min(0f)] public float weight;
    }

    [Serializable]
    public class Bag<T>
    {
        [SerializeField] private List<BagItem<T>> data;
        private float[] _cumulativeWeights;
        private float _totalWeight;

        public Bag(params BagItem<T>[] items)
        {
            data = new List<BagItem<T>>(items);
            Initialize();
        }

        public void Initialize()
        {
            _cumulativeWeights = new float[data.Count];
            UpdateCumulativeWeights();
        }

        /// <summary>
        /// <para>Indicates the random value in the <see cref="data"/>. If <see cref="data"/> is empty return default vaule of T</para>
        /// </summary>
        /// <returns></returns>
        public T Pick()
        {
            if (data == null || data.Count == 0) return default;

            float point = Random.Range(0f, _totalWeight);
            var lo = 0;
            int hi = _cumulativeWeights.Length - 1;

            while (lo < hi)
            {
                var mid = (int) (((uint) hi + (uint) lo) >> 1);
                float value = _cumulativeWeights[mid];

                if (point > value)
                {
                    lo = mid + 1;
                }
                else
                {
                    float prevPoint = mid > 0 ? _cumulativeWeights[mid - 1] : 0;

                    if (point >= prevPoint) return data[mid].item;

                    hi = mid;
                }
            }

            return data[hi].item;
        }


        /// <summary>
        /// <para>Indicates the random value in the <see cref="data"/> and also remove that element. If <see cref="data"/> is empty return default vaule of T</para>
        /// </summary>
        /// <returns></returns>
        public T Pop()
        {
            if (data == null || data.Count == 0) return default;

            float point = Random.Range(0f, _totalWeight);
            var lo = 0;
            int hi = _cumulativeWeights.Length - 1;

            while (lo < hi)
            {
                var mid = (int) (((uint) hi + (uint) lo) >> 1);
                float value = _cumulativeWeights[mid];

                if (point > value)
                {
                    lo = mid + 1;
                }
                else
                {
                    float prevPoint = mid > 0 ? _cumulativeWeights[mid - 1] : 0;

                    if (point >= prevPoint)
                    {
                        var item = data[mid];
                        data.RemoveAt(mid);
                        _totalWeight -= item.weight;
                        UpdateCumulativeWeights();
                        return item.item;
                    }

                    hi = mid;
                }
            }

            int l = _cumulativeWeights.Length - 1;
            var result = data[l];
            data.RemoveAt(l);
            _totalWeight -= result.weight;
            UpdateCumulativeWeights();
            return result.item;
        }

        private void UpdateCumulativeWeights()
        {
            float totalWeight = 0;
            for (var i = 0; i < data.Count; i++)
            {
                totalWeight += data[i].weight;
                _cumulativeWeights[i] = totalWeight;
            }

            _totalWeight = totalWeight;
        }
    }
}