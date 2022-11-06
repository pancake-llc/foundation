using Pancake.Editor;

[assembly: RegisterTriGroupDrawer(typeof(TabGroupDrawer))]

namespace Pancake.Editor
{
    public class TabGroupDrawer : GroupDrawer<DeclareTabGroupAttribute>
    {
        public override PropertyCollectionBaseInspectorElement CreateElement(DeclareTabGroupAttribute attribute) { return new TabGroupInspectorElement(); }
    }
}