#if PANCAKE_SPINE
using System;
using UnityEngine;

namespace Pancake.Spine
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class SpineSkinPickupAttribute : PropertyAttribute
    {
        public string Name { get; set; }

        public SpineSkinPickupAttribute(string name) { Name = name; }
    }
}
#endif