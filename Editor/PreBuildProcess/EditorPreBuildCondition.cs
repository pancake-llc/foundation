using System;
using Pancake;
using Pancake.Attribute;

namespace PancakeEditor
{
    [HideMono]
    [Serializable]
    [EditorIcon("scriptable_build")]
    public abstract class EditorPreBuildCondition : ScriptableSettings<EditorPreBuildCondition>
    {
        public abstract bool Validate();
    }
}