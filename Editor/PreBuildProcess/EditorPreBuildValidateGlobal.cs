using Pancake.Attribute;

namespace PancakeEditor
{
    [EditorIcon("scriptable_build")]
    public class EditorPreBuildValidateGlobal : EditorPreBuildCondition
    {
        public override bool Validate() { return false; }
    }
}