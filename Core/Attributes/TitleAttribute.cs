using System;
using System.Diagnostics;

namespace Pancake.Attribute
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    [Conditional("UNITY_EDITOR")]
    public sealed class TitleAttribute : System.Attribute
    {
        public string Title { get; }
        public bool HorizontalLine { get; set; } = true;

        public TitleAttribute(string title) { Title = title; }
    }
}