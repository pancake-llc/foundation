using System;
using System.Diagnostics;

namespace Pancake.Attribute
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [Conditional("UNITY_EDITOR")]
    public class ValidateInputAttribute : System.Attribute
    {
        public string Method { get; }

        public ValidateInputAttribute(string method) { Method = method; }
    }
}