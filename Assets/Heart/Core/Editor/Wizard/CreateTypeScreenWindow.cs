using System.IO;
using Pancake.ExLibEditor;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public class CreateTypeScreenWindow : PopupWindowContent
    {
        private readonly Rect _position;
        private string _typeText = "YourType";
        private bool _page = true;
        private bool _popup = true;
        private bool _sheet = true;
        private bool _invalidTypeName;
        private string _path;
        private readonly Vector2 _dimensions = new Vector2(300, 300);
        private readonly GUIStyle _bgStyle;

        public override Vector2 GetWindowSize() => _dimensions;

        public CreateTypeScreenWindow(Rect origin)
        {
            _position = origin;
            _bgStyle = new GUIStyle(GUIStyle.none) {normal = {background = EditorCreator.CreateTexture(Uniform.FieryRose)}};
        }

        public override void OnGUI(Rect rect)
        {
            editorWindow.position = Uniform.CenterInWindow(editorWindow.position, _position);

            Uniform.DrawHeader("Create new Type");
            EditorGUI.BeginChangeCheck();
            _typeText = EditorGUILayout.TextField(_typeText, EditorStyles.textField);
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
            _path = ProjectDatabase.DEFAULT_PATH_SCRIPT_GENERATED;
            EditorGUILayout.LabelField($"{_path}", guiStyle);

            DrawButtons();
        }

        private bool _previousPageStaus;
        private bool _previousPopupStaus;
        private bool _previousSheetStaus;

        private void DrawTypeToggles()
        {
            EditorGUILayout.BeginVertical();

            _page = GUILayout.Toggle(_page, "Page");
            if (_previousPageStaus != _page && _page)
            {
                _previousPageStaus = _page;
                _previousPopupStaus = false;
                _previousPageStaus = false;
                _popup = false;
                _sheet = false;
            }

            GUILayout.Space(5);
            _popup = GUILayout.Toggle(_popup, "Popup");
            if (_previousPopupStaus != _popup && _popup)
            {
                _previousPopupStaus = _popup;
                _previousSheetStaus = false;
                _previousPageStaus = false;
                _page = false;
                _sheet = false;
            }

            GUILayout.Space(5);
            _sheet = GUILayout.Toggle(_sheet, "Sheet");
            if (_previousSheetStaus != _sheet && _sheet)
            {
                _previousSheetStaus = _sheet;
                _previousPopupStaus = false;
                _previousPageStaus = false;
                _page = false;
                _popup = false;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawButtons()
        {
            if (GUILayout.Button("Create", GUILayout.ExpandHeight(true)))
            {
                if (!IsTypeNameValid()) return;

                TextAsset newFile = null;
                var progress = 0f;
                EditorUtility.DisplayProgressBar("Progress", "Start", progress);

                if (_page)
                {
                    CreateView(_typeText, $"{_typeText}View.cs", _path + "/Page");
                    newFile = CreatePresenter(_typeText,
                        $"{_typeText}Page.cs",
                        _path + "/Page",
                        true,
                        false,
                        false);
                    if (newFile == null)
                    {
                        Close();
                        return;
                    }
                }

                progress += 0.25f;
                EditorUtility.DisplayProgressBar("Progress", "Generating...", progress);

                if (_popup)
                {
                    CreateView(_typeText, $"{_typeText}View.cs", _path + "/Popup");
                    newFile = CreatePresenter(_typeText,
                        $"{_typeText}Popup.cs",
                        _path + "/Popup",
                        false,
                        true,
                        false);
                    if (newFile == null)
                    {
                        Close();
                        return;
                    }
                }

                progress += 0.25f;
                EditorUtility.DisplayProgressBar("Progress", "Generating...", progress);

                if (_sheet)
                {
                    CreateView(_typeText, $"{_typeText}View.cs", _path + "/Sheet");
                    newFile = CreatePresenter(_typeText,
                        $"{_typeText}Sheet.cs",
                        _path + "/Sheet",
                        false,
                        false,
                        true);
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
            if (hasError) EditorUtility.DisplayDialog("Error", $"Failed to create {_typeText}", "OK");
        }

        private bool IsTypeNameValid()
        {
            var valid = System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(_typeText);
            return valid;
        }

        private TextAsset CreateView(string typeName, string fileName, string path)
        {
            string content = EditorResources.ScreenViewTemplate.text;
            content = content.Replace("#TYPE#", typeName);
            try
            {
                var newFile = EditorCreator.CreateTextFile(content, fileName, path);
                return newFile;
            }
            catch (IOException e)
            {
                EditorUtility.DisplayDialog("Could not create class", e.Message, "OK");
                return null;
            }
        }

        private TextAsset CreatePresenter(string typeName, string fileName, string path, bool page, bool popup, bool sheet)
        {
            string content = EditorResources.ScreenPresenterTemplate.text;
            content = content.Replace("#TYPE#", typeName);
            var str = "Page";
            if (popup) str = "Popup";
            if (sheet) str = "Sheet";

            content = content.Replace("#TYPE2#", str);
            try
            {
                var newFile = EditorCreator.CreateTextFile(content, fileName, path);
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