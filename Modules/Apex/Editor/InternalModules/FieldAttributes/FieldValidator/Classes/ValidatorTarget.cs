using System;

namespace Pancake.ApexEditor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ValidatorTarget : Attribute
    {
        public readonly Type target;

        public ValidatorTarget(Type target) { this.target = target; }
    }
}