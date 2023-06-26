using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Sensor
{
    public class MovingAverageFilter
    {
        int _size;

        public int Size
        {
            get => _size;
            set
            {
                _size = Mathf.Max(value, 1);
                FitSize();
            }
        }

        // Prefer to underestimate then overestimate
        public float Value => total / Mathf.Max(_size, samples.Count);

        Queue<float> samples = new Queue<float>();
        float total;

        MovingAverageFilter() { }
        public MovingAverageFilter(int initialSize) { Size = initialSize; }

        public void AddSample(float x)
        {
            samples.Enqueue(x);
            total += x;
            FitSize();
        }

        public void Clear()
        {
            samples.Clear();
            total = 0;
        }

        void FitSize()
        {
            while (samples.Count > _size)
            {
                total -= samples.Dequeue();
            }
        }
    }
}