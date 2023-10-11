using System;

namespace Pancake.ApexEditor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ViewTarget : Attribute
    {
        public readonly Type target;

        public ViewTarget(Type target) { this.target = target; }
    }
}