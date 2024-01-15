using Pancake.Scriptable;
using UnityEditor;
using UnityEngine;

namespace Pancake.ScriptableEditor
{
    [InitializeOnLoad]
    internal class ScriptableAssetInspectorGUI
    {
        static ScriptableAssetInspectorGUI() { Editor.finishedDefaultHeaderGUI += OnPostHeaderGUI; }

        private static void OnPostHeaderGUI(Editor editor)
        {
            if (!EditorUtility.IsPersistent(editor.target)) return;
            if (editor.targets.Length > 1)
            {
                EditorGUILayout.LabelField(EditorGUIUtility.TrTempContent("[Multiple objects selected]"));
            }
            else if (editor.targets.Length > 0)
            {
                var t = editor.targets[0];
                if (!t.GetType().IsSubclassOf(typeof(ScriptableBase))) return;

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Show In Wizard", EditorStyles.miniButton, GUILayout.Width(100)))
                {
                    var window = ScriptableWizardWindow.Show();
                    window.SelectAndScrollTo(t);
                }

                EditorGUILayout.EndHorizontal();
            }
        }
    }
}