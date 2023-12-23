using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class MaxValueAttribute : ValidatorAttribute
    {
        public readonly float value;
        public readonly string property;

        public MaxValueAttribute(float value) { this.value = value; }

        public MaxValueAttribute(string property)
        {
            this.property = property;
            this.value = 100;
        }
    }
}