using System;
using System.Diagnostics;

namespace Pancake.Attribute
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [Conditional("UNITY_EDITOR")]
    public class PropertyTooltipAttribute : System.Attribute
    {
        public string Tooltip { get; }

        public PropertyTooltipAttribute(string tooltip) { Tooltip = tooltip; }
    }
}