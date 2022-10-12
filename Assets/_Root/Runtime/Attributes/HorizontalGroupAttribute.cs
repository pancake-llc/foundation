using System;

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class HorizontalGroupAttribute : ContainerAttribute
    {
        public HorizontalGroupAttribute(string name)
            : base(name)
        {
        }
    }
}