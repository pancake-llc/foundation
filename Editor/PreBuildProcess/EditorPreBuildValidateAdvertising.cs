using Pancake.Attribute;

namespace PancakeEditor
{
    [EditorIcon("scriptable_build")]
    public class EditorPreBuildValidateAdvertising : EditorPreBuildCondition
    {
        public override bool Validate() { return false; }
    }
}