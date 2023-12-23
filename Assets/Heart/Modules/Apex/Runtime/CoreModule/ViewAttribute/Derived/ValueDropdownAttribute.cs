using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ValueDropdownAttribute : ViewAttribute
    {
        public readonly string name;

        public ValueDropdownAttribute(string member) { this.name = member; }
    }
}