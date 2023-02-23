using Pancake.AttributeDrawer;

[assembly: RegisterGroupDrawer(typeof(HorizontalGroupDrawer))]

namespace Pancake.AttributeDrawer
{
    public class HorizontalGroupDrawer : GroupDrawer<DeclareHorizontalGroupAttribute>
    {
        public override PropertyCollectionBaseInspectorElement CreateElement(DeclareHorizontalGroupAttribute attribute) { return new HorizontalGroupInspectorElement(); }
    }
}