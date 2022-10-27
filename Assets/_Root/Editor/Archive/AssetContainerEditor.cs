using UnityEditor;

namespace Pancake.Editor
{
    [CustomEditor(typeof(AssetContainer))]
    public class AssetContainerEditor : UnityEditor.Editor
    {
        public static bool callFromEditorWindow = false;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            if (callFromEditorWindow) return;
            EditorGUILayout.Space();
            Uniform.HelpBox("This ScriptableObject holds the settings!. Please click the button below to edit it.", MessageType.Info);
            Uniform.Button("Edit", AssetContainerWindow.ShowWindow);
            serializedObject.ApplyModifiedProperties();
        }
    }
}