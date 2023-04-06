using System;
using System.Diagnostics;

namespace Pancake.Attribute
{
    [AttributeUsage((AttributeTargets.Field | AttributeTargets.Property))]
    [Conditional("UNITY_EDITOR")]
    public class DisplayAsStringAttribute : System.Attribute
    {
        
    }
}