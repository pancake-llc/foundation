using Pancake.Editor;

[assembly: RegisterGroupDrawer(typeof(FoldoutGroupDrawer))]

namespace Pancake.Editor
{
    public class FoldoutGroupDrawer : GroupDrawer<DeclareFoldoutGroupAttribute>
    {
        public override PropertyCollectionBaseInspectorElement CreateElement(DeclareFoldoutGroupAttribute attribute)
        {
            return new BoxGroupInspectorElement(attribute.Title, new BoxGroupInspectorElement.Props() {foldout = true, expandedByDefault = attribute.Expanded});
        }
    }
}