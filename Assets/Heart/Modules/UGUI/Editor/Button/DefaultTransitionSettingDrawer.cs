using Pancake.ExLibEditor;
using Pancake.UI;
using UnityEditor;

namespace PancakeEditor
{
    [CustomEditor(typeof(DefaultTransitionSetting), true)]
    public class DefaultTransitionSettingDrawer : UnityEditor.Editor
    {
        public override void OnInspectorGUI() { Uniform.DrawInspectorExcept(serializedObject, string.Empty); }
    }
}