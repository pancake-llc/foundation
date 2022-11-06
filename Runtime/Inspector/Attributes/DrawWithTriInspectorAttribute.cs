using System;
using System.Diagnostics;

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
    [Conditional("UNITY_EDITOR")]
    public class DrawWithTriInspectorAttribute : Attribute
    {
    }
}