using System;
using System.Diagnostics;

namespace Pancake.Attribute
{
    [AttributeUsage(AttributeTargets.Method)]
    [Conditional("UNITY_EDITOR")]
    public class ButtonAttribute : System.Attribute
    {
        public ButtonAttribute() { }

        public ButtonAttribute(string name) { Name = name; }

        public ButtonAttribute(ButtonSize buttonSize) { ButtonSize = (int) buttonSize; }

        public string Name { get; set; }
        public int ButtonSize { get; }
    }
}