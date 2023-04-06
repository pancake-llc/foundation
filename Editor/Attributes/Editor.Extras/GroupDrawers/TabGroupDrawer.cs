using Pancake.Attribute;
using PancakeEditor.Attribute;

[assembly: RegisterGroupDrawer(typeof(TabGroupDrawer))]

namespace PancakeEditor.Attribute
{
    public class TabGroupDrawer : GroupDrawer<DeclareTabGroupAttribute>
    {
        public override PropertyCollectionBaseInspectorElement CreateElement(DeclareTabGroupAttribute attribute) { return new TabGroupInspectorElement(); }
    }
}