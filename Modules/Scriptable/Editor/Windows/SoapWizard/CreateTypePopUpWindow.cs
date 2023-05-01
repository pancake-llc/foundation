using System;
using System.IO;
using System.Linq;
using PancakeEditor;
using UnityEditor;
using UnityEngine;

namespace Obvious.Soap.Editor
{
    public class CreateTypePopUpWindow : PopupWindowContent
    {
        private readonly GUISkin _skin;
        private readonly Rect _position;
        private string _typeText = "YourType";
        private bool _variable = true;
        private bool _event = true;
        private bool _eventListener = true;
        private bool _list = true;
        private bool _invalidTypeName;
        private string _path;
        private readonly Vector2 _dimensions = new Vector2(300, 300);
        private readonly GUIStyle _bgStyle;

        public override Vector2 GetWindowSize() => _dimensions;

        public CreateTypePopUpWindow(Rect origin)
        {
            _position = origin;
            _skin = Resources.Load<GUISkin>("GUISkins/SoapWizardGUISkin");
            _bgStyle = new GUIStyle(GUIStyle.none);
            _bgStyle.normal.background = PancakeEditor.Editor.CreateTexture(Uniform.FieryRose);
        }

        public override void OnGUI(Rect rect)
        {
            editorWindow.position = Uniform.CenterInWindow(editorWindow.position, _position);

            DrawTitle();
            EditorGUI.BeginChangeCheck();
            _typeText = EditorGUILayout.TextField(_typeText, _skin.textField);
            if (EditorGUI.EndChangeCheck())
            {
                _invalidTypeName = !IsTypeNameValid();
            }

            var guiStyle = new GUIStyle(EditorStyles.label);
            guiStyle.normal.textColor = _invalidTypeName ? Uniform.FieryRose : Color.white;
            guiStyle.fontStyle = FontStyle.Bold;
            var errorMessage = _invalidTypeName ? "Invalid type name." : "";
            EditorGUILayout.LabelField(errorMessage, guiStyle);

            DrawTypeToggles();

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Selected path:", EditorStyles.boldLabel);
            guiStyle = new GUIStyle(EditorStyles.label);
            guiStyle.fontStyle = FontStyle.Italic;
            _path = PancakeEditor.Editor.DEFAULT_PATH_SCRIPT_GENERATED;
            EditorGUILayout.LabelField($"{_path}", guiStyle);

            DrawButtons();
        }

        private void DrawTypeToggles()
        {
            EditorGUILayout.BeginVertical();

            _variable = GUILayout.Toggle(_variable, "ScriptableVariable");
            GUILayout.Space(5);
            _event = GUILayout.Toggle(_event, "ScriptableEvent");
            GUILayout.Space(5);
            _eventListener = GUILayout.Toggle(_eventListener && _event, "EventListener");
            GUILayout.Space(5);
            _list = GUILayout.Toggle(_list, "ScriptableList");

            EditorGUILayout.EndVertical();
        }

        private void DrawTitle()
        {
            GUILayout.BeginVertical(_bgStyle);
            var titleStyle = _skin.customStyles.ToList().Find(x => x.name == "title");
            EditorGUILayout.LabelField("Create new Type", titleStyle);
            GUILayout.EndVertical();
        }

        private void DrawButtons()
        {
            if (GUILayout.Button("Create", GUILayout.ExpandHeight(true)))
            {
                if (!IsTypeNameValid())
                    return;

                TextAsset newFile = null;
                var progress = 0f;
                EditorUtility.DisplayProgressBar("Progress", "Start", progress);

                if (_variable)
                {
                    newFile = CreateNewClass(EditorResources.ScriptableVariableTemplate.text, _typeText, $"{_typeText}Variable.cs", _path);
                    if (newFile == null)
                    {
                        Close();
                        return;
                    }
                }

                progress += 0.25f;
                EditorUtility.DisplayProgressBar("Progress", "Generating...", progress);

                if (_event)
                {
                    newFile = CreateNewClass(EditorResources.ScriptableEventTemplate.text,
                        _typeText,
                        nameof(EditorResources.ScriptableEventTemplate).Replace("Template", _typeText),
                        _path);
                    if (newFile == null)
                    {
                        Close();
                        return;
                    }
                }

                progress += 0.25f;
                EditorUtility.DisplayProgressBar("Progress", "Generating...", progress);

                if (_eventListener)
                {
                    newFile = CreateNewClass(EditorResources.ScriptableEventListenerTemplate.text,
                        _typeText,
                        nameof(EditorResources.ScriptableEventListenerTemplate).Replace("Template", _typeText),
                        _path);
                    if (newFile == null)
                    {
                        Close();
                        return;
                    }
                }

                progress += 0.25f;
                EditorUtility.DisplayProgressBar("Progress", "Generating...", progress);

                if (_list)
                {
                    newFile = CreateNewClass(EditorResources.ScriptableListTemplate.text,
                        _typeText,
                        nameof(EditorResources.ScriptableListTemplate).Replace("Template", _typeText),
                        _path);
                    if (newFile == null)
                    {
                        Close();
                        return;
                    }
                }

                progress += 0.25f;
                EditorUtility.DisplayProgressBar("Progress", "Completed!", progress);

                EditorUtility.DisplayDialog("Success", $"{_typeText} was created!", "OK");
                Close(false);
                EditorGUIUtility.PingObject(newFile);
            }

            if (GUILayout.Button("Cancel", GUILayout.ExpandHeight(true)))
            {
                editorWindow.Close();
            }
        }

        private void Close(bool hasError = true)
        {
            EditorUtility.ClearProgressBar();
            editorWindow.Close();
            if (hasError)
                EditorUtility.DisplayDialog("Error", $"Failed to create {_typeText}", "OK");
        }

        private bool IsTypeNameValid()
        {
            var valid = System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(_typeText);
            return valid;
        }

        private TextAsset CreateNewClass(string contentTemplate, string typeName, string fileName, string path)
        {
            contentTemplate = contentTemplate.Replace("#TYPE#", typeName);
            try
            {
                var newFile = PancakeEditor.Editor.CreateTextFile(contentTemplate, fileName, path);
                return newFile;
            }
            catch (IOException e)
            {
                EditorUtility.DisplayDialog("Could not create class", e.Message, "OK");
                return null;
            }
        }
    }
}