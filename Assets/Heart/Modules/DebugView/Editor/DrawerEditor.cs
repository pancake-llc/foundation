using UnityEditor;
using UnityEngine;

namespace Pancake.DebugViewEditor
{
    [CustomEditor(typeof(Pancake.DebugView.Drawer), true)]
    public class DrawerEditor : UnityEditor.Editor
    {
        private bool _debugFoldout;
        private SerializedProperty _directionProp;
        private SerializedProperty _moveInsideSafeAreaProp;
        private SerializedProperty _openOnStartProp;
        private SerializedProperty _scriptProp;
        private SerializedProperty _sizeProp;

        protected virtual void OnEnable()
        {
            _scriptProp = serializedObject.FindProperty("m_Script");
            _directionProp = serializedObject.FindProperty("direction");
            _sizeProp = serializedObject.FindProperty("size");
            _moveInsideSafeAreaProp = serializedObject.FindProperty("moveInsideSafeArea");
            _openOnStartProp = serializedObject.FindProperty("openOnStart");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawProperties();
            serializedObject.ApplyModifiedProperties();

            _debugFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_debugFoldout, "Debug");
            if (_debugFoldout)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                DrawDebugMenu();
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        protected virtual void DrawProperties()
        {
            GUI.enabled = false;
            EditorGUILayout.PropertyField(_scriptProp);
            GUI.enabled = true;
            EditorGUILayout.PropertyField(_directionProp);
            EditorGUILayout.PropertyField(_sizeProp);
            EditorGUILayout.PropertyField(_moveInsideSafeAreaProp);
            EditorGUILayout.PropertyField(_openOnStartProp);
        }

        protected virtual void DrawDebugMenu()
        {
            var component = (Pancake.DebugView.Drawer) target;
            using (var ccs = new EditorGUI.ChangeCheckScope())
            {
                var progress = EditorGUILayout.Slider("Progress", component.Progress, 0.0f, 1.0f);
                if (ccs.changed)
                {
                    component.Progress = progress;
                    EditorApplication.QueuePlayerLoopUpdate();
                }
            }
        }
    }
}