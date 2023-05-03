using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class GUIStyleAttribute : ApexAttribute
    {
        public readonly string name;

        public GUIStyleAttribute(string member) { this.name = member; }
    }
}