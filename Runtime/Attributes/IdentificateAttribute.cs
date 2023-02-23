using System;
using System.Diagnostics;

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Method)]
    [Conditional("UNITY_EDITOR")]
    public class IdentificateAttribute : Attribute
    {
        public string Value { get; }
        public IdentificateAttribute(string value) { Value = value; }
    }
}