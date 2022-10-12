using System;

namespace Pancake.Editor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ManipulatorTarget : Attribute
    {
        public readonly Type target;

        public ManipulatorTarget(Type target) { this.target = target; }
    }
}