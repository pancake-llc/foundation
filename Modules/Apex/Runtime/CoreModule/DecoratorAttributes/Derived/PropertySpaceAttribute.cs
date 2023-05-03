namespace Pancake.Apex
{
    public sealed class PropertySpaceAttribute : DecoratorAttribute
    {
        public readonly float space;

        public PropertySpaceAttribute() { space = 8; }

        public PropertySpaceAttribute(float space) { this.space = space; }
    }
}