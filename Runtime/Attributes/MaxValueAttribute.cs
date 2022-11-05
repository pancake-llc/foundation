namespace Pancake
{
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