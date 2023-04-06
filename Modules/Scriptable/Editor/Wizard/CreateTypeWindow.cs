using System.IO;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Scriptable
{
    public class CreateTypeWindow : PopupWindowContent
    {
        private const string PATH_GENERATE = "Assets/_Root/Scripts/Generated/Scriptables";

        private readonly Rect _position;
        private string _typeText = "EnterYourType";
        private bool _variable = true;
        private bool _event = true;
        private bool _eventListener = true;
        private bool _list = true;
        private bool _invalidTypeName;
        private readonly Vector2 _windowSize = new Vector2(310f, 300f);


        public override Vector2 GetWindowSize() => _windowSize;

        public CreateTypeWindow(Rect position) { _position = position; }

        public override void OnGUI(Rect rect)
        {
            editorWindow.position = Uniform.CenterInWindow(editorWindow.position, _position);
            Uniform.DrawHeader("Create New Type");

            EditorGUI.BeginChangeCheck();
            _typeText = EditorGUILayout.TextField(_typeText, new GUIStyle(EditorStyles.textField) {margin = new RectOffset(5, 5, 5, 0)});
            if (EditorGUI.EndChangeCheck()) _invalidTypeName = !IsTypeNameValid();

            var guiStyle = new GUIStyle(EditorStyles.label) {normal = {textColor = _invalidTypeName ? Uniform.Red : Color.white}, fontStyle = FontStyle.Bold};
            var errorMessage = _invalidTypeName ? "Invalid type name." : "";
            EditorGUILayout.LabelField(errorMessage, guiStyle);

            DrawTypeToggles();

            GUILayout.Space(20);
            guiStyle = new GUIStyle(EditorStyles.label) {fontStyle = FontStyle.Italic};
            EditorGUILayout.LabelField($"Output: {PATH_GENERATE}", guiStyle);

            DrawButtons();
        }

        private void DrawButtons()
        {
            if (GUILayout.Button("Create", GUILayout.ExpandHeight(true)))
            {
                if (!IsTypeNameValid()) return;

                TextAsset newFile = null;
                var progress = 0f;
                EditorUtility.DisplayProgressBar("Progress", "Start", progress);

                if (_variable)
                {
                    if (!CreateFromTemplate(EditorResources.ScriptableVariableTemplate, "{0}Variable.cs", out newFile))
                    {
                        Close();
                        return;
                    }
                }

                progress += 0.25f;
                EditorUtility.DisplayProgressBar("Progress", "Generating...", progress);

                if (_event)
                {
                    if (!CreateFromTemplate(EditorResources.ScriptableEventTemplate, "ScriptableEvent{0}.cs", out newFile))
                    {
                        Close();
                        return;
                    }
                }

                progress += 0.25f;
                EditorUtility.DisplayProgressBar("Progress", "Generating...", progress);

                if (_eventListener)
                {
                    if (!CreateFromTemplate(EditorResources.ScriptableEventListenerTemplate, "EventListener{0}.cs", out newFile))
                    {
                        Close();
                        return;
                    }
                }

                progress += 0.25f;
                EditorUtility.DisplayProgressBar("Progress", "Generating...", progress);

                if (_list)
                {
                    if (!CreateFromTemplate(EditorResources.ScriptableListTemplate, "ScriptableList{0}.cs", out newFile))
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

        private bool IsTypeNameValid() { return System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(_typeText); }

        private void Close(bool hasError = true)
        {
            EditorUtility.ClearProgressBar();
            editorWindow.Close();
            if (hasError) EditorUtility.DisplayDialog("Error", $"Failed to create {_typeText}", "OK");
        }

        private bool CreateFromTemplate(TextAsset template, string nameFormat, out TextAsset result)
        {
            result = CreateTypeFromScriptableTemplate(template, _typeText, string.Format(nameFormat, _typeText), PATH_GENERATE);
            return result != null;
        }

        /// <summary>
        /// Creates a new C# class from a template.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="typeName"></param>
        /// <param name="fileName"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private static TextAsset CreateTypeFromScriptableTemplate(TextAsset template, string typeName, string fileName, string path)
        {
            string templateCode = template.text;
            templateCode = templateCode.Replace("#TYPE#", typeName);
            try
            {
                var newFile = Editor.CreateTextFile(templateCode, fileName, path);
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