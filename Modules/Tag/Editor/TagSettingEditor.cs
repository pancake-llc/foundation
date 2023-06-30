using Pancake.Tag;
using Pancake.ExLibEditor;
using UnityEditor;

namespace Pancake.TagEditor
{
    [CustomEditor(typeof(TagSetting), true)]
    public class TagSettingEditor : Editor
    {
        public override void OnInspectorGUI() { Uniform.DrawInspectorExcept(serializedObject, new[] {"m_Script"}); }
    }
}