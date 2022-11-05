using System;

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class GUIStyleAttribute : PancakeAttribute
    {
        public readonly string member;

        public GUIStyleAttribute(string member) { this.member = member; }
    }
}