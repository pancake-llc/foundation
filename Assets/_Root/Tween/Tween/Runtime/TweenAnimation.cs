#pragma warning disable CS0414

using System;
using UnityEngine;

namespace Pancake.Core.Tween
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class TweenAnimationAttribute : Attribute
    {
        public readonly string menu;
        public readonly string name;

        public TweenAnimationAttribute(string menu, string name)
        {
            this.menu = menu;
            this.name = name;
        }
    }

    [Serializable]
    public abstract partial class TweenAnimation
    {
        public bool enabled = true;

        [SerializeField] private float minNormalizedTime = 0f;

        [SerializeField] private float maxNormalizedTime = 1f;

        [SerializeField] private bool holdBeforeStart = true;

        [SerializeField] private bool holdAfterEnd = true;

        [SerializeField] private Interpolator interpolator = default;

        [SerializeField] private bool foldout = true; // Editor Only

        [SerializeField] private string comment = null; // Editor Only

        public float MinNormalizedTime
        {
            get { return minNormalizedTime; }
            set
            {
                minNormalizedTime = M.Clamp01(value);
                maxNormalizedTime = M.Clamp(maxNormalizedTime, minNormalizedTime, 1f);
            }
        }


        public float MaxNormalizedTime
        {
            get { return maxNormalizedTime; }
            set
            {
                maxNormalizedTime = M.Clamp01(value);
                minNormalizedTime = M.Clamp(minNormalizedTime, 0f, maxNormalizedTime);
            }
        }


        public bool HoldBeforeStart { get => holdBeforeStart; set => holdBeforeStart = value; }


        public bool HoldAfterEnd { get => holdAfterEnd; set => holdAfterEnd = value; }


        public void Sample(float normalizedTime)
        {
            if (normalizedTime < minNormalizedTime)
            {
                if (holdBeforeStart) normalizedTime = 0f;
                else return;
            }
            else if (normalizedTime > maxNormalizedTime)
            {
                if (holdAfterEnd) normalizedTime = 1f;
                else return;
            }
            else
            {
                if (M.Approximately(maxNormalizedTime, minNormalizedTime)) normalizedTime = 1f;
                else normalizedTime = (normalizedTime - minNormalizedTime) / (maxNormalizedTime - minNormalizedTime);
            }

            Interpolate(interpolator[normalizedTime]);
        }


        public abstract void Interpolate(float factor);
    } // class TweenAnimation
} // UnityExtensions.Tween