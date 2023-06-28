using Pancake.BTag;
using Pancake.ExLibEditor;
using UnityEditor;

namespace Pancake.BTagEditor
{
    [CustomEditor(typeof(BTagSetting), true)]
    public class BTagSettingEditor : Editor
    {
        public override void OnInspectorGUI() { Uniform.DrawInspectorExcept(serializedObject, new[] {"m_Script"}); }
    }
}