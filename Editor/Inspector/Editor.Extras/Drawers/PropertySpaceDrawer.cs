using Pancake.Editor;
using UnityEngine;

[assembly: RegisterTriAttributeDrawer(typeof(PropertySpaceDrawer), DrawerOrder.Inspector)]

namespace Pancake.Editor
{
    public class PropertySpaceDrawer : AttributeDrawer<PropertySpaceAttribute>
    {
        public override float GetHeight(float width, Property property, InspectorElement next)
        {
            var totalSpace = Attribute.SpaceBefore + Attribute.SpaceAfter;

            return next.GetHeight(width) + totalSpace;
        }

        public override void OnGUI(Rect position, Property property, InspectorElement next)
        {
            var contentPosition = new Rect(position) {yMin = position.yMin + Attribute.SpaceBefore, yMax = position.yMax - Attribute.SpaceAfter,};

            next.OnGUI(contentPosition);
        }
    }
}