using System;
using System.Diagnostics;

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Property)]
    [Conditional("UNITY_EDITOR")]
    public class ShowInInspector : Attribute
    {
    }
}