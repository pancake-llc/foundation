using System;
using Pancake.Apex;
using UnityEngine;

namespace Pancake
{
    [Serializable]
    [UseDefaultEditor]
    public abstract class RandomValue<T>
    {
        [SerializeField] protected bool useConstant;
        [SerializeField] protected T min;
        [SerializeField] protected T max;

        public abstract T Value();
    }
}