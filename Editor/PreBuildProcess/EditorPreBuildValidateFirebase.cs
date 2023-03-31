using Pancake.Attribute;

namespace PancakeEditor
{
    [EditorIcon("scriptable_build")]
    public class EditorPreBuildValidateFirebase : EditorPreBuildCondition
    {
        public override bool Validate()
        {
            return false;
        }
    }

}