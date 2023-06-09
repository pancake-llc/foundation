using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ButtonHeightAttribute : ApexAttribute
    {
        public readonly float height;

        public ButtonHeightAttribute(float height) { this.height = height; }
    }
}