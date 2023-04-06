using Pancake.Attribute;

namespace PancakeEditor
{
    [EditorIcon("scriptable_build")]
    public class EditorPreBuildValidateFirebase : EditorPreBuildCondition
    {
        public override (bool, string) Validate() { return (false, ""); }
    }

}