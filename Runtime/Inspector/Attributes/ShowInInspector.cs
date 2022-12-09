using System;
using System.Diagnostics;

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public class ShowInInspector : Attribute
    {
    }
}