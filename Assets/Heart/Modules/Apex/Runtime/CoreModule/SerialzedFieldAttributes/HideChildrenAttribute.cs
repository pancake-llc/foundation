using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class HideChildrenAttribute : ApexAttribute
    {
        public readonly string[] names;

        public HideChildrenAttribute(params string[] names) { this.names = names; }
    }
}