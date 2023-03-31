using Pancake.Attribute;

namespace PancakeEditor
{
    [EditorIcon("scriptable_build")]
    public class EditorPreBuildValidateNotification: EditorPreBuildCondition
    {
        public override bool Validate()
        {
            return false;
        }
    }

}