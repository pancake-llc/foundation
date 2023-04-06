using System;
using System.Diagnostics;

namespace Pancake.Attribute
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Struct)]
    [Conditional("UNITY_EDITOR")]
    public class PropertySpaceAttribute : System.Attribute
    {
        public float SpaceBefore { get; set; }
        public float SpaceAfter { get; set; }

        public PropertySpaceAttribute() { SpaceBefore = 7; }

        public PropertySpaceAttribute(float spaceBefore = 0, float spaceAfter = 0)
        {
            SpaceBefore = spaceBefore;
            SpaceAfter = spaceAfter;
        }
    }
}