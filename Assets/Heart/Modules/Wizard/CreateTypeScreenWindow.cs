using System.IO;
using Pancake.Common;
using PancakeEditor.Common;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    internal class CreateTypeScreenWindow : PopupWindowContent
    {
        private readonly Rect _position;
        private string _typeText = "YourType";
        private bool _page = true;
        private bool _popup = true;
        private bool _sheet = true;
        private bool _invalidTypeName;
        private readonly Vector2 _dimensions = new(250, 250);

        public override Vector2 GetWindowSize() => _dimensions;

        public CreateTypeScreenWindow(Rect origin) { _position = origin; }

        public override void OnGUI(Rect rect)
        {
            editorWindow.position = Uniform.CenterInWindow(editorWindow.position, _position);

            GUILayout.Label("Name Class".ToBold(), Uniform.CenterRichLabel);
            EditorGUI.BeginChangeCheck();
            _typeText = EditorGUILayout.TextField(_typeText, EditorStyles.textField);
            if (EditorGUI.EndChangeCheck()) _invalidTypeName = !IsTypeNameValid();

            var guiStyle = new GUIStyle(EditorStyles.label) {normal = {textColor = _invalidTypeName ? Uniform.Rose_400 : Color.white}, fontStyle = FontStyle.Bold};
            if (_invalidTypeName) EditorGUILayout.LabelField("Invalid type name.", guiStyle);
            else EditorGUILayout.LabelField("Result: " + GetResultNameClass().ToBold().SetColor(Uniform.Green_500), Uniform.RichLabel);

            GUILayout.Label("Type".ToBold(), Uniform.CenterRichLabel);
            DrawTypeToggles();

            GUILayout.Space(10);
            GUILayout.Label("Output".ToBold(), Uniform.CenterRichLabel);
            EditorGUILayout.LabelField(ProjectDatabase.DEFAULT_PATH_SCRIPT_GENERATED.ToItalic().SetColor(Uniform.Warning), Uniform.CenterRichLabel);

            GUILayout.FlexibleSpace();
            DrawButtons();
        }

        private bool _previousPageStaus;
        private bool _previousPopupStaus;
        private bool _previousSheetStaus;

        private void DrawTypeToggles()
        {
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
        }

        private void DrawButtons()
        {
            if (GUILayout.Button("Create", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
            {
                if (!IsTypeNameValid()) return;

                TextAsset newFile = null;
                var progress = 0f;
                EditorUtility.DisplayProgressBar("Progress", "Start", progress);

                if (_page)
                {
                    const string p = ProjectDatabase.DEFAULT_PATH_SCRIPT_GENERATED + "/Page";
                    CreateView(_typeText, $"{_typeText}View.cs", p);
                    newFile = CreatePresenter(_typeText,
                        $"{_typeText}Page.cs",
                        p,
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
                    const string p = ProjectDatabase.DEFAULT_PATH_SCRIPT_GENERATED + "/Popup";
                    CreateView(_typeText, $"{_typeText}View.cs", p);
                    newFile = CreatePresenter(_typeText,
                        $"{_typeText}Popup.cs",
                        p,
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
                    const string p = ProjectDatabase.DEFAULT_PATH_SCRIPT_GENERATED + "/Sheet";
                    CreateView(_typeText, $"{_typeText}View.cs", p);
                    newFile = CreatePresenter(_typeText,
                        $"{_typeText}Sheet.cs",
                        p,
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

                EditorUtility.DisplayDialog("Success", $"{GetResultNameClass()} was created!", "OK");
                Close(false);
                EditorGUIUtility.PingObject(newFile);
            }

            if (GUILayout.Button("Cancel", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT))) editorWindow.Close();
        }

        private string GetResultNameClass()
        {
            string str = _typeText;
            if (_page) str += "Page";
            else if (_sheet) str += "Sheet";
            else if (_popup) str += "Popup";

            return str;
        }

        private void Close(bool hasError = true)
        {
            EditorUtility.ClearProgressBar();
            editorWindow.Close();
            if (hasError) EditorUtility.DisplayDialog("Error", $"Failed to create {GetResultNameClass()}", "OK");
        }

        private bool IsTypeNameValid()
        {
            bool valid = System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(_typeText);
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