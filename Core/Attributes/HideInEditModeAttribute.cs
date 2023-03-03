using System;
using System.Diagnostics;

namespace Pancake.Attribute
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    [Conditional("UNITY_EDITOR")]
    public class HideInEditModeAttribute : System.Attribute
    {
        public bool Inverse { get; protected set; }
    }
}