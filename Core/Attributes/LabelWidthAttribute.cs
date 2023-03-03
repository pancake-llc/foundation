using System;
using System.Diagnostics;

namespace Pancake.Attribute
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [Conditional("UNITY_EDITOR")]
    public sealed class LabelWidthAttribute : System.Attribute
    {
        public float Width { get; }

        public LabelWidthAttribute(float width) { Width = width; }
    }
}