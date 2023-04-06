using Pancake.Attribute;
using PancakeEditor.Attribute;

[assembly: RegisterGroupDrawer(typeof(HorizontalGroupDrawer))]

namespace PancakeEditor.Attribute
{
    public class HorizontalGroupDrawer : GroupDrawer<DeclareHorizontalGroupAttribute>
    {
        public override PropertyCollectionBaseInspectorElement CreateElement(DeclareHorizontalGroupAttribute attribute)
        {
            return new HorizontalGroupInspectorElement(attribute.Sizes);
        }
    }
}