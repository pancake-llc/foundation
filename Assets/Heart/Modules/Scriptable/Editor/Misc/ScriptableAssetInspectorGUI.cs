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

                var scriptableSetting = Resources.Load<ScriptableEditorSetting>(nameof(ScriptableEditorSetting));
                if (scriptableSetting == null) return;

                EditorGUILayout.BeginHorizontal();
                var scriptableBase = t as ScriptableBase;
                string[] categories = ScriptableEditorSetting.Categories.ToArray();
                GUILayout.Label(new GUIContent("Category"));
                var totalRect = EditorGUILayout.GetControlRect();
                EditorGUI.BeginChangeCheck();
                int newCategoryIndex = EditorGUI.Popup(totalRect, scriptableBase.categoryIndex, categories);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(scriptableBase, "Change Category");
                    scriptableBase.categoryIndex = newCategoryIndex;
                    EditorUtility.SetDirty(scriptableBase);
                }

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