using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class MinValueAttribute : ValidatorAttribute
    {
        public readonly float value;
        public readonly string property;

        public MinValueAttribute(float value) { this.value = value; }

        public MinValueAttribute(string property) { this.property = property; }
    }
}