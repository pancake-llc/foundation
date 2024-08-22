using System;
using System.Collections.Generic;
using UnityEngine;
using Math = Pancake.Common.Math;
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
        private readonly ENumberRoll _numberRoll = ENumberRoll.One;
        private readonly ERollResultType _rollResultType = ERollResultType.None;

        public float TotalWeight => _totalWeight;

        public Bag(params BagItem<T>[] items)
        {
            data = new List<BagItem<T>>(items);
            Initialize();
        }

        public Bag(ENumberRoll numberRoll, ERollResultType rollResultType, params BagItem<T>[] items)
        {
            _numberRoll = numberRoll;
            _rollResultType = rollResultType;
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

            float point = 0;
            if (_rollResultType == ERollResultType.None) point = Random.Range(0f, _totalWeight);
            else
            {
                switch (_numberRoll)
                {
                    case ENumberRoll.One:
                        point = Random.Range(0f, _totalWeight);
                        break;
                    case ENumberRoll.Two:
                        switch (_rollResultType)
                        {
                            case ERollResultType.Min:
                                float a1 = Random.Range(0f, _totalWeight);
                                float b1 = Random.Range(0f, _totalWeight);
                                point = Math.Min(a1, b1);
                                break;
                            case ERollResultType.Max:
                                float a2 = Random.Range(0f, _totalWeight);
                                float b2 = Random.Range(0f, _totalWeight);
                                point = Math.Max(a2, b2);
                                break;
                            case ERollResultType.Sum:
                                float newCap = _totalWeight / 2f;
                                float a3 = Random.Range(0f, newCap);
                                float b3 = Random.Range(0f, newCap);
                                point = a3 + b3;
                                break;
                        }

                        break;
                    case ENumberRoll.Three:
                        switch (_rollResultType)
                        {
                            case ERollResultType.Min:
                                float a1 = Random.Range(0f, _totalWeight);
                                float b1 = Random.Range(0f, _totalWeight);
                                float c1 = Random.Range(0f, _totalWeight);
                                point = Math.Min(a1, b1, c1);
                                break;
                            case ERollResultType.Max:
                                float a2 = Random.Range(0f, _totalWeight);
                                float b2 = Random.Range(0f, _totalWeight);
                                float c2 = Random.Range(0f, _totalWeight);
                                point = Math.Max(a2, b2, c2);
                                break;
                            case ERollResultType.Sum:
                                float newCap = _totalWeight / 3f;
                                float a3 = Random.Range(0f, newCap);
                                float b3 = Random.Range(0f, newCap);
                                float c3 = Random.Range(0f, newCap);
                                point = a3 + b3 + c3;
                                break;
                        }
                        break;
                }
            }

            var lo = 0;
            int hi = _cumulativeWeights.Length - 1;

            while (lo < hi)
            {
                int mid = (int) (((uint) hi + (uint) lo) >> 1);
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
        /// <para>Indicates the random value in <see cref="data"/> and also remove that element. If <see cref="data"/> is empty return default vaule of T</para>
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