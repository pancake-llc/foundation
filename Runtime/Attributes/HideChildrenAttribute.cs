using System;

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class HideChildrenAttribute : PancakeAttribute
    {
        public readonly string[] names;

        public HideChildrenAttribute(params string[] names) { this.names = names; }
    }
}