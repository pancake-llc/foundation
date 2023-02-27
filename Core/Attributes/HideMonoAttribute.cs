using System;
using System.Diagnostics;

namespace Pancake
{
    [AttributeUsage((AttributeTargets.Class | AttributeTargets.Struct))]
    [Conditional("UNITY_EDITOR")]
    public sealed class HideMonoAttribute : Attribute
    {
    }
}