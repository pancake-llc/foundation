using System;
using System.Diagnostics;

namespace Pancake.Attribute
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [Conditional("UNITY_EDITOR")]
    public class OnValueChangedAttribute : System.Attribute
    {
        public OnValueChangedAttribute(string method) { Method = method; }

        public string Method { get; }
    }
}