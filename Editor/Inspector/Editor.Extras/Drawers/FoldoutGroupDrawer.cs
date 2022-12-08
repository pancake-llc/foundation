using Pancake.Editor;

[assembly: RegisterGroupDrawer(typeof(FoldoutGroupDrawer))]

namespace Pancake.Editor
{
    public class FoldoutGroupDrawer : GroupDrawer<DeclareFoldoutGroupAttribute>
    {
        public override PropertyCollectionBaseInspectorElement CreateElement(DeclareFoldoutGroupAttribute attribute)
        {
            return new BoxGroupInspectorElement(new BoxGroupInspectorElement.Props()
            {
                title = attribute.Title, titleMode = BoxGroupInspectorElement.TitleMode.Foldout, expandedByDefault = attribute.Expanded
            });
        }
    }
}