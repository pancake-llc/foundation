using UnityEditor;
using UnityEngine;

namespace Sisus.Init.EditorOnly
{
    internal static class Styles
    {
        internal static readonly GUIStyle RichTextLabel;
        internal static readonly GUIStyle RefTag;
        internal static readonly GUIStyle ServiceTag;
        internal static readonly GUIStyle ValueProvider;
        internal static readonly GUIStyle Discard;
        internal static readonly GUIContent AddButtonIcon;
        internal static readonly GUIStyle AddButtonStyle;
        internal static readonly GUIStyle AddButtonBackground;
        internal static readonly GUIStyle RichTextHelpBox;
        internal static readonly GUIContent ErrorIcon;
        internal static readonly GUIContent WarningIcon;
        internal static readonly GUIContent InfoIcon;

        static Styles()
        {
            RichTextLabel = new(EditorStyles.label) { richText = true };
            ServiceTag = new("AssetLabel") { contentOffset = new(0f, -1f) };
            ValueProvider = ServiceTag;
            RefTag = ServiceTag;
            Discard = new("SearchCancelButton") { alignment = TextAnchor.MiddleRight };
            AddButtonStyle = "RL FooterButton";
            AddButtonBackground = "RL Footer";
            RichTextHelpBox = new(EditorStyles.helpBox) { richText = true };
            var padding = RichTextHelpBox.padding;
            padding.left += 20;
            RichTextHelpBox.padding = padding;

            AddButtonIcon = new(EditorGUIUtility.TrIconContent("Toolbar Plus").image);
            ErrorIcon = new(EditorGUIUtility.IconContent("console.erroricon.sml").image);
            WarningIcon = new(EditorGUIUtility.IconContent("console.warnicon.sml").image);
            InfoIcon = new(EditorGUIUtility.IconContent("console.infoicon.sml").image);
        }
    }
}