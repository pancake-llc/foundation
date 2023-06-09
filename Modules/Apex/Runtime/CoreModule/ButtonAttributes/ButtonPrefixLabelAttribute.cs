using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ButtonPrefixLabelAttribute : ApexAttribute
    {
        public readonly string label;

        public ButtonPrefixLabelAttribute(string label) { this.label = label; }
    }
}