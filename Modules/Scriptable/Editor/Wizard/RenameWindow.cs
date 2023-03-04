using Pancake.Scriptable;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Scriptable
{
    public class RenameWindow : PopupWindowContent
    {
        private readonly Rect _position;
        private readonly ScriptableBase _scriptableBase;
        private string _newName;

        public override Vector2 GetWindowSize() => new Vector2(300f, 170f);

        public RenameWindow(Rect position, ScriptableBase scriptableBase)
        {
            _position = position;
            _scriptableBase = scriptableBase;
            _newName = _scriptableBase.name;
        }

        public override void OnGUI(Rect rect)
        {
            editorWindow.position = Uniform.CenterInWindow(editorWindow.position, _position);
            Uniform.DrawHeader("Rename");

            _newName = EditorGUILayout.TextField(_newName, new GUIStyle(EditorStyles.textField) {margin = new RectOffset(5, 5, 5, 0)});

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Rename", GUILayout.MaxHeight(36)))
            {
                Editor.RenameAsset(_scriptableBase, _newName);
                editorWindow.Close();
            }

            if (GUILayout.Button("Cancel", GUILayout.MaxHeight(36))) editorWindow.Close();
        }
    }
}