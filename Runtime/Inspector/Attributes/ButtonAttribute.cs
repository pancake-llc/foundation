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

        public ButtonAttribute(EButtonSize eButtonSize) { ButtonSize = (int) eButtonSize; }

        public string Name { get; set; }
        public int ButtonSize { get; }
    }
}