using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Common
{
    public static class TooltipWindow
    {
        private static Event GetCurrent()
        {
            var fieldInfo = typeof(Event).GetField(name: "s_Current", bindingAttr: BindingFlags.Static | BindingFlags.NonPublic);

            return (Event) fieldInfo?.GetValue(null);
        }

        public static void Show(string message, float duration = 1f, int fontSize = 16)
        {
            var current = GetCurrent();
            var activatorRect = new Rect(current.mousePosition, Vector2.one);
            var content = new TooltipWindowContent(message, duration, fontSize);
            PopupWindow.Show(activatorRect, content);
        }
    }

    internal sealed class TooltipWindowContent : PopupWindowContent
    {
        private static readonly Vector2 WindowSizeOffset = new(6, 4);

        private readonly string _message;
        private readonly GUIStyle _style;
        private readonly Vector2 _size;
        private readonly GUILayoutOption _height;
        private readonly float _time;

        public TooltipWindowContent(string text, float time, int fontSize)
        {
            _style = new GUIStyle(EditorStyles.label) {alignment = TextAnchor.UpperLeft, fontSize = fontSize};

            var guiContent = new GUIContent(text);
            var labelSize = _style.CalcSize(guiContent);

            _message = text;
            _size = labelSize + WindowSizeOffset;
            _height = GUILayout.Height(labelSize.y);
            _time = time;
        }

        public override void OnOpen() { OnUpdate(); }

        public override void OnClose() { }

        private async void OnUpdate()
        {
            await Awaitable.WaitForSecondsAsync(_time);
            if (editorWindow == null) return;
            editorWindow.Close();
        }

        public override Vector2 GetWindowSize() { return _size; }

        public override void OnGUI(Rect rect) { EditorGUILayout.LabelField(_message, _style, _height); }
    }
}