using Pancake.Editor;

[assembly: RegisterTriGroupDrawer(typeof(HorizontalGroupDrawer))]

namespace Pancake.Editor
{
    public class HorizontalGroupDrawer : GroupDrawer<DeclareHorizontalGroupAttribute>
    {
        public override PropertyCollectionBaseInspectorElement CreateElement(DeclareHorizontalGroupAttribute attribute) { return new HorizontalGroupInspectorElement(); }
    }
}