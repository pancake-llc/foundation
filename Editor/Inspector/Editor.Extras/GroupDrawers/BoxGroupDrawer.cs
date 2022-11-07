using Pancake.Editor;

[assembly: RegisterGroupDrawer(typeof(BoxGroupDrawer))]

namespace Pancake.Editor
{
    public class BoxGroupDrawer : GroupDrawer<DeclareBoxGroupAttribute>
    {
        public override PropertyCollectionBaseInspectorElement CreateElement(DeclareBoxGroupAttribute attribute) { return new BoxGroupInspectorElement(attribute); }
    }
}