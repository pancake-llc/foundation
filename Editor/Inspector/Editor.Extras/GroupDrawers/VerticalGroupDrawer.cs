using Pancake.Editor;

[assembly: RegisterTriGroupDrawer(typeof(VerticalGroupDrawer))]

namespace Pancake.Editor
{
    public class VerticalGroupDrawer : GroupDrawer<DeclareVerticalGroupAttribute>
    {
        public override PropertyCollectionBaseInspectorElement CreateElement(DeclareVerticalGroupAttribute attribute) { return new VerticalGroupInspectorElement(); }
    }
}