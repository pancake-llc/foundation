using System;

namespace Pancake.Editor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class DecoratorTarget : Attribute
    {
        public readonly Type target;

        public DecoratorTarget(Type target) { this.target = target; }
    }
}