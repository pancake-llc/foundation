using System;
using System.Diagnostics;

namespace Pancake.Attribute
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    [Conditional("UNITY_EDITOR")]
    public class TabAttribute : System.Attribute
    {
        public TabAttribute(string tab) { TabName = tab; }

        public string TabName { get; }
    }
}