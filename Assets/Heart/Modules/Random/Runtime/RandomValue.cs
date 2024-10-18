using System;
using UnityEngine;

namespace Pancake
{
    [Serializable]
    public abstract class RandomValue<T>
    {
        [SerializeField] protected bool useConstant;
        [SerializeField] protected T min;
        [SerializeField] protected T max;

        public abstract T Value();
    }
}