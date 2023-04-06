using Pancake.Attribute;

namespace PancakeEditor
{
    [EditorIcon("scriptable_build")]
    public class EditorPreBuildValidateNotification: EditorPreBuildCondition
    {
        public override (bool, string) Validate() { return (false, ""); }
    }

}