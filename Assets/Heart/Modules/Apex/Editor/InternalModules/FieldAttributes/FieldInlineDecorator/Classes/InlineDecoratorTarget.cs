using System;

namespace Pancake.ApexEditor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class InlineDecoratorTarget : Attribute
    {
        public readonly Type target;

        public InlineDecoratorTarget(Type target) { this.target = target; }
    }
}