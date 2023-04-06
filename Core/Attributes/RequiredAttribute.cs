using System;

namespace Pancake.Attribute
{
    [AttributeUsage((AttributeTargets.Field | AttributeTargets.Property))]
    public class RequiredAttribute : System.Attribute
    {
        public string Message { get; set; }
    }
}