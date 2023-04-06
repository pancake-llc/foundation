using System;
using System.Diagnostics;

namespace Pancake.Attribute
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
    [Conditional("UNITY_EDITOR")]
    public class DrawWithInspectorAttribute : System.Attribute
    {
    }
}