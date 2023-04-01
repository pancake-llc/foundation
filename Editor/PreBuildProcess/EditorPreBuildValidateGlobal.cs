using Pancake.Attribute;

namespace PancakeEditor
{
    [EditorIcon("scriptable_build")]
    public class EditorPreBuildValidateGlobal : EditorPreBuildCondition
    {
        public override (bool, string) Validate() { return (false, ""); }
    }
}