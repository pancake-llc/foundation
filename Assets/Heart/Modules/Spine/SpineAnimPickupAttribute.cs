#if PANCAKE_SPINE
using System;

namespace Pancake.Spine
{
    using UnityEngine;

    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class SpineAnimPickupAttribute : PropertyAttribute
    {
        public string Name { get; set; }

        public SpineAnimPickupAttribute(string name) { Name = name; }
    }
}
#endif