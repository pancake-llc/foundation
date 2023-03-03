using Pancake.Attribute;
using PancakeEditor.Attribute;

[assembly: RegisterGroupDrawer(typeof(BoxGroupDrawer))]

namespace PancakeEditor.Attribute
{
    public class BoxGroupDrawer : GroupDrawer<DeclareBoxGroupAttribute>
    {
        public override PropertyCollectionBaseInspectorElement CreateElement(DeclareBoxGroupAttribute attribute)
        {
            return new BoxGroupInspectorElement(new BoxGroupInspectorElement.Props()
            {
                title = attribute.Title, titleMode = attribute.HideTitle ? BoxGroupInspectorElement.TitleMode.Hidden : BoxGroupInspectorElement.TitleMode.Normal
            });
        }
    }
}