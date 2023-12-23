using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class LabelWidthAttribute : ManipulatorAttribute
    {
        public readonly float width;

        public LabelWidthAttribute(float width) { this.width = width; }
    }
}