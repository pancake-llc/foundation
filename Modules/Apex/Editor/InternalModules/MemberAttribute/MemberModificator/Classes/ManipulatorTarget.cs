using System;

namespace Pancake.ApexEditor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ManipulatorTarget : Attribute
    {
        public readonly Type target;

        public ManipulatorTarget(Type target) { this.target = target; }
    }
}