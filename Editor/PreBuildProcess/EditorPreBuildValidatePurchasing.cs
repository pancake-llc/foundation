using Pancake.Attribute;

namespace PancakeEditor
{
    [EditorIcon("scriptable_build")]
    public class EditorPreBuildValidatePurchasing : EditorPreBuildCondition
    {
        public override bool Validate() { return false; }
    }
}