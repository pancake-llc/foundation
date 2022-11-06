using Pancake.Editor;
using UnityEngine;

[assembly: RegisterTriAttributeDrawer(typeof(IndentDrawer), DrawerOrder.Decorator)]

namespace Pancake.Editor
{
    public class IndentDrawer : AttributeDrawer<IndentAttribute>
    {
        public override void OnGUI(Rect position, Property property, InspectorElement next)
        {
            using (var indentedRectScope = GuiHelper.PushIndentedRect(position, Attribute.Indent))
            {
                next.OnGUI(indentedRectScope.IndentedRect);
            }
        }
    }
}