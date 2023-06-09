using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ButtonStyleAttribute : ApexAttribute
    {
        public readonly string name;

        public ButtonStyleAttribute(string name) { this.name = name; }
    }
}