using Pancake.Attribute;

namespace PancakeEditor
{
    [EditorIcon("scriptable_build")]
    public class EditorPreBuildValidateAdvertising : EditorPreBuildCondition
    {
        public override (bool, string) Validate() { return (false, ""); }
    }
}