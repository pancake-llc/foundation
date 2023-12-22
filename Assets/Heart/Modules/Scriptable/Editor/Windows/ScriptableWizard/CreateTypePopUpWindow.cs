using System.IO;
using Pancake.ExLibEditor;
using UnityEditor;
using UnityEngine;

namespace Pancake.ScriptableEditor
{
    public class CreateTypePopUpWindow : PopupWindowContent
    {
        private readonly Rect _position;
        private string _typeText = "Type";
        private bool _baseClass = false;
        private bool _monoBehaviour = true;
        private bool _variable = true;
        private bool _event = true;
        private bool _eventListener = true;
        private bool _list = true;
        private bool _invalidTypeName;
        private string _path;
        private readonly Vector2 _dimensions = new Vector2(350, 350);
        private readonly GUIStyle _bgStyle;

        public override Vector2 GetWindowSize() => _dimensions;

        public CreateTypePopUpWindow(Rect origin)
        {
            _position = origin;
            _bgStyle = new GUIStyle(GUIStyle.none);
            _bgStyle.normal.background = EditorCreator.CreateTexture(Uniform.FieryRose);
        }

        public override void OnGUI(Rect rect)
        {
            editorWindow.position = Uniform.CenterInWindow(editorWindow.position, _position);

            Uniform.DrawHeader("Create new Type");
            DrawTextField();
            DrawTypeToggles();
            GUILayout.Space(10);
            DrawPath();
            DrawButtons();
        }

        private void DrawPath()
        {
            EditorGUILayout.LabelField("Selected path:", EditorStyles.boldLabel);
            var guiStyle = new GUIStyle(EditorStyles.label) {fontStyle = FontStyle.Italic};
            _path = ProjectDatabase.DEFAULT_PATH_SCRIPT_GENERATED;
            EditorGUILayout.LabelField($"{_path}", guiStyle);
        }

        private void DrawTextField()
        {
            EditorGUI.BeginChangeCheck();
            _typeText = EditorGUILayout.TextField(_typeText, EditorStyles.textField);
            if (EditorGUI.EndChangeCheck()) _invalidTypeName = !IsTypeNameValid();

            var guiStyle = new GUIStyle(EditorStyles.label) {normal = {textColor = _invalidTypeName ? Uniform.FieryRose : Color.white}, fontStyle = FontStyle.Bold};
            string errorMessage = _invalidTypeName ? "Invalid type name." : "";
            EditorGUILayout.LabelField(errorMessage, guiStyle);
        }

        private void DrawTypeToggles()
        {
            var nameType = $"{_typeText.ToCamelCase()}";
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            if (!EditorExtend.IsBuiltInType(_typeText))
            {
                DrawToggle(ref _baseClass, nameType, "", EditorGUIUtility.IconContent("cs Script Icon").image, true,140);
                if (_baseClass) _monoBehaviour = GUILayout.Toggle(_monoBehaviour, "MonoBehaviour?");
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);
            DrawToggle(ref _variable,
                nameType,
                "Variable",
                EditorResources.ScriptableVariable,
                true);
            GUILayout.Space(5);
            DrawToggle(ref _event, "ScriptableEvent", nameType, EditorResources.ScriptableEvent);
            GUILayout.Space(5);
            DrawToggle(ref _eventListener, "EventListener", nameType, EditorResources.ScriptableEventListener);
            GUILayout.Space(5);
            DrawToggle(ref _list, "ScriptableList", nameType, EditorResources.ScriptableList);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawToggle(ref bool toggleValue, string typeName, string second, Texture icon, bool isFirstRed = false, int maxWidth = 200)
        {
            EditorGUILayout.BeginHorizontal();
            var style = new GUIStyle(GUIStyle.none);
            GUILayout.Box(icon, style, GUILayout.Width(18), GUILayout.Height(18));
            toggleValue = GUILayout.Toggle(toggleValue, "", GUILayout.Width(maxWidth));
            var firstStyle = new GUIStyle(GUI.skin.label) {padding = {left = 15 - maxWidth}};
            if (isFirstRed) firstStyle.normal.textColor = Uniform.FieryRose;
            GUILayout.Label(typeName, firstStyle);
            var secondStyle = new GUIStyle(GUI.skin.label) {padding = {left = -6}};
            if (!isFirstRed) secondStyle.normal.textColor = Uniform.FieryRose;
            GUILayout.Label(second, secondStyle);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawButtons()
        {
            GUI.enabled = !_invalidTypeName;
            if (GUILayout.Button("Create", GUILayout.ExpandHeight(true)))
            {
                if (!IsTypeNameValid()) return;

                TextAsset newFile = null;
                var progress = 0f;
                EditorUtility.DisplayProgressBar("Progress", "Start", progress);
                
                if (_baseClass && !EditorExtend.IsBuiltInType(_typeText))
                {
                    string template = _monoBehaviour ? EditorResources.MonoBehaviourTemplate.text : EditorResources.ClassTemplate.text;
                    newFile = CreateNewClass(template, _typeText, $"{_typeText}.cs", _path);
                    if (newFile == null)
                    {
                        Close();
                        return;
                    }
                }
                
                progress += 0.2f;
                EditorUtility.DisplayProgressBar("Progress", "Generating...", progress);
                
                if (_variable)
                {
                    newFile = CreateNewClass(EditorResources.ScriptableVariableTemplate.text, _typeText, $"{_typeText}Variable.cs", _path);
                    if (newFile == null)
                    {
                        Close();
                        return;
                    }
                }

                progress += 0.2f;
                EditorUtility.DisplayProgressBar("Progress", "Generating...", progress);

                if (_event)
                {
                    newFile = CreateNewClass(EditorResources.ScriptableEventTemplate.text,
                        _typeText,
                        $"{nameof(EditorResources.ScriptableEventTemplate).Replace("Template", _typeText)}.cs",
                        _path);
                    if (newFile == null)
                    {
                        Close();
                        return;
                    }
                }

                progress += 0.2f;
                EditorUtility.DisplayProgressBar("Progress", "Generating...", progress);

                if (_eventListener)
                {
                    newFile = CreateNewClass(EditorResources.ScriptableEventListenerTemplate.text,
                        _typeText,
                        $"{nameof(EditorResources.ScriptableEventListenerTemplate).Replace("Template", _typeText)}.cs",
                        _path);
                    if (newFile == null)
                    {
                        Close();
                        return;
                    }
                }

                progress += 0.2f;
                EditorUtility.DisplayProgressBar("Progress", "Generating...", progress);

                if (_list)
                {
                    newFile = CreateNewClass(EditorResources.ScriptableListTemplate.text,
                        _typeText,
                        $"{nameof(EditorResources.ScriptableListTemplate).Replace("Template", _typeText)}.cs",
                        _path);
                    if (newFile == null)
                    {
                        Close();
                        return;
                    }
                }

                progress += 0.2f;
                EditorUtility.DisplayProgressBar("Progress", "Completed!", progress);

                EditorUtility.DisplayDialog("Success", $"{_typeText} was created!", "OK");
                Close(false);
                EditorGUIUtility.PingObject(newFile);
            }

            if (GUILayout.Button("Cancel", GUILayout.ExpandHeight(true)))  editorWindow.Close();
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
                var newFile = EditorCreator.CreateTextFile(contentTemplate, fileName, path);
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