using Pancake.AttributeDrawer;

[assembly: RegisterGroupDrawer(typeof(TabGroupDrawer))]

namespace Pancake.AttributeDrawer
{
    public class TabGroupDrawer : GroupDrawer<DeclareTabGroupAttribute>
    {
        public override PropertyCollectionBaseInspectorElement CreateElement(DeclareTabGroupAttribute attribute) { return new TabGroupInspectorElement(); }
    }
}