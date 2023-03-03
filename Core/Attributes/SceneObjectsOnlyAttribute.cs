using System;
using System.Diagnostics;

namespace Pancake.Attribute
{
    [AttributeUsage((AttributeTargets.Field | AttributeTargets.Property))]
    [Conditional("UNITY_EDITOR")]
    public sealed class SceneObjectsOnlyAttribute : System.Attribute
    {
    }
}