using System;

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false)]
    public abstract class ContainerAttribute : PancakeAttribute
    {
        public readonly string Name;

        protected ContainerAttribute(string name)
        {
            this.Name = name;
        }
    }
}
