using System;
using System.Diagnostics;

namespace Pancake.Attribute
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    [Conditional("UNITY_EDITOR")]
    public class IndentAttribute : System.Attribute
    {
        public int Indent { get; }

        public IndentAttribute()
            : this(1)
        {
        }

        public IndentAttribute(int indent) { Indent = indent; }
    }
}