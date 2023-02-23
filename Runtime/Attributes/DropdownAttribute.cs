using System;
using System.Diagnostics;

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [Conditional("UNITY_EDITOR")]
    public sealed class DropdownAttribute : Attribute
    {
        public string Values { get; }

        public DropdownAttribute(string values) { Values = values; }
    }
}