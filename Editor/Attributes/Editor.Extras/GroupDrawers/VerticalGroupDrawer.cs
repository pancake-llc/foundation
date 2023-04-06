using Pancake.Attribute;
using PancakeEditor.Attribute;

[assembly: RegisterGroupDrawer(typeof(VerticalGroupDrawer))]

namespace PancakeEditor.Attribute
{
    public class VerticalGroupDrawer : GroupDrawer<DeclareVerticalGroupAttribute>
    {
        public override PropertyCollectionBaseInspectorElement CreateElement(DeclareVerticalGroupAttribute attribute) { return new VerticalGroupInspectorElement(); }
    }
}