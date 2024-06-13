using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    internal static class NavigatorWindow
    {
        public static void OnInspectorGUI()
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Create Modal", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
                {
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Create Page", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
                {
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Create Sheet", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
                {
                }

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}