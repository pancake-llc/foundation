using Pancake.Attribute;

namespace PancakeEditor
{
    
    [EditorIcon("scriptable_build")]
    public class EditorPreBuildValidateAdjust : EditorPreBuildCondition
    {
        public override bool Validate() { return false; }
    }
}