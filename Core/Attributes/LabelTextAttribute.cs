using System;
using System.Diagnostics;

namespace Pancake.Attribute
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [Conditional("UNITY_EDITOR")]
    public sealed class LabelTextAttribute : System.Attribute
    {
        public string Text { get; }

        public LabelTextAttribute(string text) { Text = text; }
    }
}