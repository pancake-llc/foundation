using Pancake.AttributeDrawer;

[assembly: RegisterGroupDrawer(typeof(BoxGroupDrawer))]

namespace Pancake.AttributeDrawer
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