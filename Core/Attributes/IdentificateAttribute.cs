using System;
using System.Diagnostics;

namespace Pancake.Attribute
{
    [AttributeUsage(AttributeTargets.Method)]
    [Conditional("UNITY_EDITOR")]
    public class IdentificateAttribute : System.Attribute
    {
        public string Value { get; }
        public IdentificateAttribute(string value) { Value = value; }
    }
}