using System.Collections.Generic;
using System.Text;
using Pancake.Attribute;
using PancakeEditor.Attribute;


[assembly: RegisterAttributeDrawer(typeof(ShowDrawerChainDrawer), DrawerOrder.System)]

namespace PancakeEditor.Attribute
{
    public class ShowDrawerChainDrawer : AttributeDrawer<ShowDrawerChainAttribute>
    {
        public override InspectorElement CreateElement(Property property, InspectorElement next) { return new DrawerChainInfoInspectorElement(property.AllDrawers, next); }
    }

    public class DrawerChainInfoInspectorElement : InspectorElement
    {
        public DrawerChainInfoInspectorElement(IReadOnlyList<CustomDrawer> drawers, InspectorElement next)
        {
            var info = new StringBuilder();

            info.Append("Drawer Chain:");

            for (var i = 0; i < drawers.Count; i++)
            {
                var drawer = drawers[i];
                info.AppendLine();
                info.Append(i).Append(": ").Append(drawer.GetType().Name);
            }

            AddChild(new InfoBoxInspectorElement(info.ToString()));
            AddChild(next);
        }
    }
}