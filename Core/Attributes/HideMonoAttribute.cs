using System;
using System.Diagnostics;

namespace Pancake.Attribute
{
    [AttributeUsage((AttributeTargets.Class | AttributeTargets.Struct))]
    [Conditional("UNITY_EDITOR")]
    public sealed class HideMonoAttribute : System.Attribute
    {
    }
}