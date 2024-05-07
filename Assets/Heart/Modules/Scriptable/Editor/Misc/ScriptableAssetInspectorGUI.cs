using PancakeEditor.Common;
using Pancake.Scriptable;
using UnityEditor;
using UnityEngine;
using Editor = UnityEditor.Editor;

namespace PancakeEditor.Scriptable
{
    [InitializeOnLoad]
    internal class ScriptableAssetInspectorGUI
    {
        private static bool isEditingDescription;
        private static string newDescription;
        private static GUIStyle buttonStyle;

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

                if (!ScriptableEditorSetting.IsExist()) return;
                var scriptableBase = t as ScriptableBase;
                if (scriptableBase == null) return;
                DrawDescriptionAndCategory(scriptableBase);
            }
        }

        private static void DrawDescriptionAndCategory(ScriptableBase scriptableBase)
        {
            EditorGUILayout.BeginHorizontal();
            GUIStyle labelStyle = new GUIStyle(EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField("Description:", labelStyle, GUILayout.Width(65));

            var icon = isEditingDescription ? EditorResources.IconCancel : EditorResources.IconEdit;
            string tooltip = isEditingDescription ? "Cancel" : "Edit Description";
            var buttonContent = new GUIContent(icon, tooltip);
            buttonStyle ??= new GUIStyle(GUI.skin.button) {padding = new RectOffset(4, 4, 4, 4)};

            if (GUILayout.Button(buttonContent, buttonStyle, GUILayout.Height(18), GUILayout.Width(18)))
            {
                if (isEditingDescription)
                {
                    newDescription = scriptableBase.description;
                    isEditingDescription = false;
                    EditorGUILayout.EndHorizontal();
                    return;
                }

                isEditingDescription = true;
                newDescription = scriptableBase.description;
            }

            GUILayout.FlexibleSpace();
            DrawCategory(scriptableBase);
            EditorGUILayout.EndHorizontal();

            if (isEditingDescription)
            {
                //Draw the text area
                GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textArea);
                textAreaStyle.wordWrap = true;
                newDescription = EditorGUILayout.TextArea(newDescription, textAreaStyle, GUILayout.Height(50));

                //Draw the confirm and cancel buttons
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Confirm", GUILayout.MaxHeight(30f)))
                {
                    Undo.RecordObject(scriptableBase, "Change Description");
                    scriptableBase.description = newDescription;
                    EditorUtility.SetDirty(scriptableBase);
                    isEditingDescription = false;
                }

                if (GUILayout.Button("Cancel", GUILayout.MaxHeight(30f)) || Event.current.keyCode == KeyCode.Escape)
                {
                    newDescription = scriptableBase.description;
                    isEditingDescription = false;
                }

                EditorGUILayout.EndHorizontal();
                return;
            }

            if (string.IsNullOrEmpty(scriptableBase.description)) return;

            EditorGUILayout.HelpBox(scriptableBase.description, MessageType.None);
        }

        private static bool DrawCategory(ScriptableBase scriptableBase)
        {
            var hasChanged = false;
            string[] categories = ScriptableEditorSetting.Categories.ToArray();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Category:", EditorStyles.miniBoldLabel, GUILayout.Width(55f));
            EditorGUI.BeginChangeCheck();
            int newCategoryIndex = EditorGUILayout.Popup(scriptableBase.categoryIndex, categories, GUILayout.MaxWidth(175));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(scriptableBase, "Change Category");
                scriptableBase.categoryIndex = newCategoryIndex;
                EditorUtility.SetDirty(scriptableBase);
                hasChanged = true;
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("See In Wizard", EditorStyles.miniButton, GUILayout.Width(100)))
            {
                var window = WizardWindow.Show();
                window.SelectAndScrollTo(scriptableBase);
            }

            EditorGUILayout.EndHorizontal();
            return hasChanged;
        }
    }
}