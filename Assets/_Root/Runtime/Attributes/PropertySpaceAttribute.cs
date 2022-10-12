namespace Pancake
{
    public sealed class PropertySpaceAttribute : DecoratorAttribute
    {
        public readonly float space;

        public PropertySpaceAttribute() { this.space = 15.0f; }

        public PropertySpaceAttribute(float space) { this.space = space; }
    }
}