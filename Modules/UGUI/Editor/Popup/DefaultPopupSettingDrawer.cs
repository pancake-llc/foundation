using Pancake.ExLibEditor;
using Pancake.UI;
using UnityEditor;

namespace PancakeEditor
{
    [CustomEditor(typeof(DefaultTransitionSetting), true)]
    public class DefaultPopupSettingDrawer : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            string[] propertiesToHide = {"m_Script"};
            Uniform.DrawInspectorExcept(serializedObject, propertiesToHide);
        }
    }
}