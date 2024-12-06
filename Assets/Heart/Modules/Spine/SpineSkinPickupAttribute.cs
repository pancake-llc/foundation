#if PANCAKE_SPINE
using System;
using UnityEngine;

namespace Pancake.Spine
{
    /// <summary>
    /// example: [SpineSkinPickup("Name file skeletonDataAsset without extension")] public string skin;
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class SpineSkinPickupAttribute : PropertyAttribute
    {
        internal string Name { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Name file skeletonDataAsset without extension</param>
        public SpineSkinPickupAttribute(string name) { Name = name; }
    }
}
#endif