using InspectorUnityInternalBridge;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    public class InfoBoxInspectorElement : InspectorElement
    {
        private readonly GUIContent _message;
        private readonly Texture2D _icon;
        private readonly Color _color;

        public InfoBoxInspectorElement(string message, EMessageType type = EMessageType.None, Color? color = null)
        {
            var messageType = GetMessageType(type);
            _icon = EditorGUIUtilityProxy.GetHelpIcon(messageType);
            _message = new GUIContent(message);
            _color = color ?? GetColor(type);
        }

        public override float GetHeight(float width)
        {
            var style = _icon == null ? Styles.InfoBoxContentNone : Styles.InfoBoxContent;
            var height = style.CalcHeight(_message, width);
            return Mathf.Max(26, height);
        }

        public override void OnGUI(Rect position)
        {
            using (GuiHelper.PushColor(_color))
            {
                GUI.Label(position, string.Empty, Styles.InfoBoxBg);
            }

            if (_icon != null)
            {
                var iconRect = new Rect(position) {xMin = position.xMin + 4, width = 20,};

                GUI.Label(position, _message, Styles.InfoBoxContent);
                GUI.DrawTexture(iconRect, _icon, ScaleMode.ScaleToFit);
            }
            else
            {
                GUI.Label(position, _message, Styles.InfoBoxContentNone);
            }
        }

        private static Color GetColor(EMessageType type)
        {
            switch (type)
            {
                case EMessageType.Error:
                    return new Color(1f, 0.4f, 0.4f);

                case EMessageType.Warning:
                    return new Color(1f, 0.8f, 0.2f);

                default:
                    return Color.white;
            }
        }

        private static MessageType GetMessageType(EMessageType type)
        {
            switch (type)
            {
                case EMessageType.None: return MessageType.None;
                case EMessageType.Info: return MessageType.Info;
                case EMessageType.Warning: return MessageType.Warning;
                case EMessageType.Error: return MessageType.Error;
                default: return MessageType.None;
            }
        }

        private static class Styles
        {
            public static readonly GUIStyle InfoBoxBg;
            public static readonly GUIStyle InfoBoxContent;
            public static readonly GUIStyle InfoBoxContentNone;

            static Styles()
            {
                InfoBoxBg = new GUIStyle(EditorStyles.helpBox);
                InfoBoxContentNone = new GUIStyle(EditorStyles.label)
                {
                    padding = new RectOffset(4, 4, 4, 4), fontSize = InfoBoxBg.fontSize, alignment = TextAnchor.MiddleLeft, wordWrap = true,
                };
                InfoBoxContent = new GUIStyle(InfoBoxContentNone) {padding = new RectOffset(26, 4, 4, 4),};
            }
        }
    }
}