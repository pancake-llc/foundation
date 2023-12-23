using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class MaxArraySizeAttribute : ValidatorAttribute
    {
        public readonly int size;

        public MaxArraySizeAttribute(int size) { this.size = size; }
    }
}