using System;
using System.Diagnostics;

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Method)]
    [Conditional("UNITY_EDITOR")]
    public sealed class ButtonAttribute : Attribute
    {
        public ButtonAttribute() { }

        public ButtonAttribute(string name) { Name = name; }

        public ButtonAttribute(ButtonSize buttonSize) { ButtonSize = (int) buttonSize; }

        public string Name { get; set; }
        public int ButtonSize { get; }
    }
}