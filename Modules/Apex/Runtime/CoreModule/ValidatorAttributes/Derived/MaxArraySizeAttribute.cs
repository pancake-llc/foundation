namespace Pancake.Apex
{
    public sealed class MaxArraySizeAttribute : ValidatorAttribute
    {
        public readonly int size;

        public MaxArraySizeAttribute(int size) { this.size = size; }
    }
}