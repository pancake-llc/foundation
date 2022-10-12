using System;

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class MinValueAttribute : ValidatorAttribute
    {
        public readonly float value;
        public readonly string property;

        public MinValueAttribute(float value) { this.value = value; }

        public MinValueAttribute(string property) { this.property = property; }
    }
}