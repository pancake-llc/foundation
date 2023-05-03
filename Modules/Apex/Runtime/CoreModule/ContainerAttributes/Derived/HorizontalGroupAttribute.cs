using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class HorizontalGroupAttribute : ContainerAttribute
    {
        public HorizontalGroupAttribute(string name)
            : base(name)
        {
        }
    }
}