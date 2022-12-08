using Pancake.Editor;

[assembly: RegisterGroupDrawer(typeof(BoxGroupDrawer))]

namespace Pancake.Editor
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