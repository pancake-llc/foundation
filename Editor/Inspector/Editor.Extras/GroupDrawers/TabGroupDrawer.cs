using Pancake.Editor;

[assembly: RegisterGroupDrawer(typeof(TabGroupDrawer))]

namespace Pancake.Editor
{
    public class TabGroupDrawer : GroupDrawer<DeclareTabGroupAttribute>
    {
        public override PropertyCollectionBaseInspectorElement CreateElement(DeclareTabGroupAttribute attribute) { return new TabGroupInspectorElement(); }
    }
}