using System;
using System.Diagnostics;

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public sealed class HideEnumAttribute : Attribute
    {
        public object[] HiddenValues { get; }

        public HideEnumAttribute(params object[] values) { HiddenValues = values; }
    }
}