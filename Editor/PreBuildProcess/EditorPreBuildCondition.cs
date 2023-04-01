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
        /// <summary>
        /// 
        /// </summary>
        /// <returns>(result + reason)</returns>
        public abstract (bool, string) Validate();
    }
}