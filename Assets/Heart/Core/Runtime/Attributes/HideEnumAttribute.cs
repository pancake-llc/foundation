using System;
using System.Diagnostics;
using UnityEngine;

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public sealed class HideEnumAttribute : PropertyAttribute
    {
        public object[] HiddenValues { get; }

        public HideEnumAttribute(params object[] values) { HiddenValues = values; }
    }
}