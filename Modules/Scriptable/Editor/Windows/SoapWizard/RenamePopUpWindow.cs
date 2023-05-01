using System.Linq;
using PancakeEditor;
using UnityEditor;
using UnityEngine;

namespace Obvious.Soap.Editor
{
    public class RenamePopUpWindow : PopupWindowContent
    {
        private readonly GUISkin _skin;
        private string _newName = "";
        private readonly Rect _position;
        private readonly Vector2 _dimensions = new Vector2(300, 170);
        private readonly ScriptableBase _scriptableBase = null;
        private readonly float _buttonHeight = 40f;
        private readonly GUIStyle _bgStyle;

        public override Vector2 GetWindowSize() => _dimensions;

        public RenamePopUpWindow(Rect origin, ScriptableBase scriptableBase)
        {
            _position = origin;
            _scriptableBase = scriptableBase;
            _skin = Resources.Load<GUISkin>("GUISkins/SoapWizardGUISkin");
            _newName = _scriptableBase.name;
            _bgStyle = new GUIStyle(GUIStyle.none);
            _bgStyle.normal.background = PancakeEditor.Editor.CreateTexture(Uniform.FieryRose);
        }

        public override void OnGUI(Rect rect)
        {
            editorWindow.position = Uniform.CenterInWindow(editorWindow.position, _position);

            DrawTitle();

            _newName = EditorGUILayout.TextField(_newName, _skin.textField);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Rename", GUILayout.MaxHeight(_buttonHeight)))
            {
                PancakeEditor.Editor.RenameAsset(_scriptableBase, _newName);
                editorWindow.Close();
            }

            if (GUILayout.Button("Cancel", GUILayout.MaxHeight(_buttonHeight)))
                editorWindow.Close();
        }

        private void DrawTitle()
        {
            GUILayout.BeginVertical(_bgStyle);
            var titleStyle = _skin.customStyles.ToList().Find(x => x.name == "title");
            EditorGUILayout.LabelField("Rename", titleStyle);
            GUILayout.EndVertical();
        }
    }
}