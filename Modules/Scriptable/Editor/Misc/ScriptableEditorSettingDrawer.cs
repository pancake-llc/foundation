using Pancake.ExLibEditor;

namespace Pancake.ScriptableEditor
{
    [UnityEditor.CustomEditor(typeof(ScriptableEditorSetting), true)]
    public class ScriptableEditorSettingDrawer : UnityEditor.Editor
    {
        public override void OnInspectorGUI() { Uniform.DrawInspectorExcept(serializedObject, new[] {"m_Script"}); }
    }
}