#if PANCAKE_SPINE
using System;
using UnityEngine;

namespace Pancake.Spine
{
    /// <summary>
    /// example: [SpineAnimPickup("Name file skeletonDataAsset without extension")] public string anim;
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class SpineAnimPickupAttribute : PropertyAttribute
    {
        internal string Name { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Name file skeletonDataAsset without extension</param>
        public SpineAnimPickupAttribute(string name) { Name = name; }
    }
}
#endif