using System;

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class IndentAttribute : ManipulatorAttribute
    {
        public readonly int level;

        public IndentAttribute() { level = 1; }

        public IndentAttribute(int level) { this.level = level; }
    }
}