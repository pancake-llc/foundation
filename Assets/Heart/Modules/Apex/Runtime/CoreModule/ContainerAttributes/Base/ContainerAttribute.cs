using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    public abstract class ContainerAttribute : ApexAttribute
    {
        public readonly string Name;

        protected ContainerAttribute(string name) { this.Name = name; }
    }
}