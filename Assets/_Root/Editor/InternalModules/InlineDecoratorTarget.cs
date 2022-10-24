using System;

namespace Pancake.Editor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class InlineDecoratorTarget : Attribute
    {
        public readonly Type target;

        public InlineDecoratorTarget(Type target) { this.target = target; }
    }
}