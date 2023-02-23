using Pancake.AttributeDrawer;

[assembly: RegisterGroupDrawer(typeof(VerticalGroupDrawer))]

namespace Pancake.AttributeDrawer
{
    public class VerticalGroupDrawer : GroupDrawer<DeclareVerticalGroupAttribute>
    {
        public override PropertyCollectionBaseInspectorElement CreateElement(DeclareVerticalGroupAttribute attribute) { return new VerticalGroupInspectorElement(); }
    }
}