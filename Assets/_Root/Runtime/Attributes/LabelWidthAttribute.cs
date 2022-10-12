using System;

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class LabelWidthAttribute : DecoratorAttribute
    {
        public readonly float width;

        public LabelWidthAttribute(float width) { this.width = width; }
    }
}