namespace Pancake.Editor
{
    public abstract class CustomDrawer : PropertyExtension
    {
        internal int Order { get; set; }

        public abstract InspectorElement CreateElementInternal(Property property, InspectorElement next);
    }
}