using System;
using System.Diagnostics;

namespace Pancake.Attribute
{
    [AttributeUsage((AttributeTargets.Class | AttributeTargets.Struct))]
    [Conditional("UNITY_EDITOR")]
    public class HideMonoAttribute : System.Attribute
    {
    }
}