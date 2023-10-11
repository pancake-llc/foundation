using Pancake.ExLibEditor;
using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    public static class ApexStyles
    {
        private static string ThemeFolder;
        static ApexStyles() { ThemeFolder = EditorGUIUtility.isProSkin ? "DarkTheme" : "LightTheme"; }

        private static GUIStyle _Label;

        public static GUIStyle Label
        {
            get
            {
                if (_Label == null)
                {
                    _Label = new GUIStyle();

                    _Label.fontSize = 12;
                    _Label.fontStyle = FontStyle.Normal;
                    _Label.alignment = TextAnchor.MiddleLeft;

                    Color32 textColor = EditorGUIUtility.isProSkin ? new Color32(200, 200, 200, 255) : new Color32(3, 3, 3, 255);

                    _Label.normal.textColor = textColor;
                    _Label.onNormal.textColor = textColor;
                    _Label.active.textColor = textColor;
                    _Label.onActive.textColor = textColor;
                    _Label.focused.textColor = textColor;
                    _Label.onFocused.textColor = textColor;
                    _Label.hover.textColor = textColor;
                    _Label.onHover.textColor = textColor;
                }

                return _Label;
            }
        }

        private static GUIStyle _LabelBold;

        public static GUIStyle LabelBold
        {
            get
            {
                if (_LabelBold == null)
                {
                    _LabelBold = new GUIStyle(Label);
                    _LabelBold.fontStyle = FontStyle.Bold;
                }

                return _LabelBold;
            }
        }

        private static GUIStyle _SuffixMessage;

        public static GUIStyle SuffixMessage
        {
            get
            {
                if (_SuffixMessage == null)
                {
                    _SuffixMessage = new GUIStyle(Label);
                    _SuffixMessage.fontSize = 10;
                    _SuffixMessage.alignment = TextAnchor.MiddleRight;
                    _SuffixMessage.fontStyle = FontStyle.Italic;
                }

                return _SuffixMessage;
            }
        }

        private static GUIStyle _BoxHeader;

        public static GUIStyle BoxHeader
        {
            get
            {
                if (_BoxHeader == null)
                {
                    _BoxHeader = new GUIStyle();

                    _BoxHeader.fontSize = 12;
                    _BoxHeader.fontStyle = FontStyle.Bold;
                    _BoxHeader.alignment = TextAnchor.MiddleLeft;
                    _BoxHeader.border = new RectOffset(2, 2, 2, 2);
                    _BoxHeader.padding = new RectOffset(10, 0, 2, 2);

                    string theme = EditorGUIUtility.isProSkin ? "DarkTheme" : "LightTheme";
                    Color32 textColor = EditorGUIUtility.isProSkin ? new Color32(200, 200, 200, 255) : new Color32(3, 3, 3, 255);

                    _BoxHeader.normal.textColor = textColor;
                    _BoxHeader.normal.background = EditorResources.ReorderableArrayNormal(theme);
                    _BoxHeader.normal.scaledBackgrounds = new Texture2D[1] {_BoxHeader.normal.background};
                }

                return _BoxHeader;
            }
        }

        private static GUIStyle _BoxEntryBkg;

        public static GUIStyle BoxEntryBkg
        {
            get
            {
                if (_BoxEntryBkg == null)
                {
                    _BoxEntryBkg = new GUIStyle();

                    _BoxEntryBkg.fontSize = 12;
                    _BoxEntryBkg.fontStyle = FontStyle.Normal;
                    _BoxEntryBkg.alignment = TextAnchor.MiddleLeft;
                    _BoxEntryBkg.border = new RectOffset(2, 2, 2, 2);
                    _BoxEntryBkg.padding = new RectOffset(10, 0, 2, 2);

                    string theme = EditorGUIUtility.isProSkin ? "DarkTheme" : "LightTheme";
                    Color32 textColor = EditorGUIUtility.isProSkin ? new Color32(200, 200, 200, 255) : new Color32(3, 3, 3, 255);

                    _BoxEntryBkg.normal.textColor = textColor;
                    _BoxEntryBkg.normal.background = EditorResources.ReorderableArrayEntryBackground(theme);
                    _BoxEntryBkg.normal.scaledBackgrounds = new Texture2D[1] {_BoxEntryBkg.normal.background};

                    _BoxEntryBkg.active.textColor = textColor;
                    _BoxEntryBkg.active.background = _BoxEntryBkg.normal.background;
                    _BoxEntryBkg.active.scaledBackgrounds = new Texture2D[1] {_BoxEntryBkg.normal.background};

                    _BoxEntryBkg.focused.textColor = textColor;
                    _BoxEntryBkg.focused.background = _BoxEntryBkg.normal.background;
                    _BoxEntryBkg.focused.scaledBackgrounds = new Texture2D[1] {_BoxEntryBkg.normal.background};

                    _BoxEntryBkg.hover.textColor = textColor;
                    _BoxEntryBkg.hover.background = _BoxEntryBkg.normal.background;
                    _BoxEntryBkg.hover.scaledBackgrounds = new Texture2D[1] {_BoxEntryBkg.normal.background};
                }

                return _BoxEntryBkg;
            }
        }

        private static GUIStyle _BoxButton;

        public static GUIStyle BoxButton
        {
            get
            {
                if (_BoxButton == null)
                {
                    _BoxButton = new GUIStyle(BoxHeader);

                    string theme = EditorGUIUtility.isProSkin ? "DarkTheme" : "LightTheme";
                    Color32 textColor = EditorGUIUtility.isProSkin ? new Color32(200, 200, 200, 255) : new Color32(3, 3, 3, 255);

                    _BoxButton.active.textColor = textColor;
                    _BoxButton.onActive.textColor = textColor;
                    _BoxButton.active.background = EditorResources.ReorderableArrayPress(theme);
                    _BoxButton.onActive.background = _BoxButton.active.background;
                    _BoxButton.active.scaledBackgrounds = new Texture2D[1] {_BoxButton.active.background};
                    _BoxButton.onActive.scaledBackgrounds = new Texture2D[1] {_BoxButton.active.background};

                    _BoxButton.focused.textColor = textColor;
                    _BoxButton.onFocused.textColor = textColor;
                    _BoxButton.focused.background = EditorResources.ReorderableArrayHover(theme);
                    _BoxButton.onFocused.background = _BoxHeader.focused.background;
                    _BoxButton.focused.scaledBackgrounds = new Texture2D[1] {_BoxButton.focused.background};
                    _BoxButton.onFocused.scaledBackgrounds = new Texture2D[1] {_BoxButton.focused.background};

                    _BoxButton.hover.textColor = textColor;
                    _BoxButton.onHover.textColor = textColor;
                    _BoxButton.hover.background = EditorResources.ReorderableArrayHover(theme);
                    _BoxButton.onHover.background = _BoxHeader.hover.background;
                    _BoxButton.hover.scaledBackgrounds = new Texture2D[1] {_BoxButton.hover.background};
                    _BoxButton.onHover.scaledBackgrounds = new Texture2D[1] {_BoxButton.hover.background};
                }

                return _BoxButton;
            }
        }

        private static GUIStyle _BoxCenteredButton;

        public static GUIStyle BoxCenteredButton
        {
            get
            {
                if (_BoxCenteredButton == null)
                {
                    _BoxCenteredButton = new GUIStyle(BoxButton);
                    _BoxCenteredButton.fontStyle = FontStyle.Normal;
                    _BoxCenteredButton.alignment = TextAnchor.MiddleCenter;
                    _BoxCenteredButton.padding = new RectOffset(0, 0, 0, 0);
                }

                return _BoxCenteredButton;
            }
        }

        private static GUIStyle _BoxEntryEven;

        public static GUIStyle BoxEntryEven
        {
            get
            {
                if (_BoxEntryEven == null)
                {
                    _BoxEntryEven = new GUIStyle();

                    _BoxEntryEven.fontSize = 12;
                    _BoxEntryEven.fontStyle = FontStyle.Normal;
                    _BoxEntryEven.alignment = TextAnchor.MiddleLeft;
                    _BoxEntryEven.border = new RectOffset(2, 2, 2, 2);
                    _BoxEntryEven.padding = new RectOffset(10, 0, 0, 1);

                    string theme = EditorGUIUtility.isProSkin ? "DarkTheme" : "LightTheme";
                    Color32 textColor = EditorGUIUtility.isProSkin ? new Color32(200, 200, 200, 255) : new Color32(3, 3, 3, 255);

                    _BoxEntryEven.normal.textColor = textColor;
                    _BoxEntryEven.onNormal.textColor = textColor;
                    _BoxEntryEven.normal.background = EditorResources.ReorderableArrayEntryEven(theme);
                    _BoxEntryEven.onNormal.background = EditorResources.ReorderableArrayEntryActive(theme);
                    _BoxEntryEven.normal.scaledBackgrounds = new Texture2D[1] {_BoxEntryEven.normal.background};
                    _BoxEntryEven.onNormal.scaledBackgrounds = new Texture2D[1] {_BoxEntryEven.onNormal.background};

                    _BoxEntryEven.active.textColor = textColor;
                    _BoxEntryEven.onActive.textColor = textColor;
                    _BoxEntryEven.active.background = _BoxEntryEven.onNormal.background;
                    _BoxEntryEven.onActive.background = _BoxEntryEven.onNormal.background;
                    _BoxEntryEven.active.scaledBackgrounds = new Texture2D[1] {_BoxEntryEven.onNormal.background};
                    _BoxEntryEven.onActive.scaledBackgrounds = new Texture2D[1] {_BoxEntryEven.onNormal.background};

                    _BoxEntryEven.focused.textColor = textColor;
                    _BoxEntryEven.onFocused.textColor = textColor;
                    _BoxEntryEven.focused.background = EditorResources.ReorderableArrayEntryFocus(theme);
                    _BoxEntryEven.onFocused.background = _BoxEntryEven.focused.background;
                    _BoxEntryEven.focused.scaledBackgrounds = new Texture2D[1] {_BoxEntryEven.focused.background};
                    _BoxEntryEven.onFocused.scaledBackgrounds = new Texture2D[1] {_BoxEntryEven.focused.background};

                    _BoxEntryEven.hover.textColor = textColor;
                    _BoxEntryEven.onHover.textColor = textColor;
                    _BoxEntryEven.hover.background = _BoxEntryEven.normal.background;
                    _BoxEntryEven.onHover.background = _BoxEntryEven.normal.background;
                    _BoxEntryEven.hover.scaledBackgrounds = new Texture2D[1] {_BoxEntryEven.normal.background};
                    _BoxEntryEven.onHover.scaledBackgrounds = new Texture2D[1] {_BoxEntryEven.normal.background};
                }

                return _BoxEntryEven;
            }
        }

        private static GUIStyle _BoxEntryOdd;

        public static GUIStyle BoxEntryOdd
        {
            get
            {
                if (_BoxEntryOdd == null)
                {
                    _BoxEntryOdd = new GUIStyle();

                    _BoxEntryOdd.fontSize = 12;
                    _BoxEntryOdd.fontStyle = FontStyle.Normal;
                    _BoxEntryOdd.alignment = TextAnchor.MiddleLeft;
                    _BoxEntryOdd.border = new RectOffset(2, 2, 2, 2);
                    _BoxEntryOdd.padding = new RectOffset(10, 0, 0, 1);

                    string theme = EditorGUIUtility.isProSkin ? "DarkTheme" : "LightTheme";
                    Color32 textColor = EditorGUIUtility.isProSkin ? new Color32(200, 200, 200, 255) : new Color32(3, 3, 3, 255);

                    _BoxEntryOdd.normal.textColor = textColor;
                    _BoxEntryOdd.onNormal.textColor = textColor;
                    _BoxEntryOdd.normal.background = EditorResources.ReorderableArrayEntryOdd(theme);
                    _BoxEntryOdd.onNormal.background = EditorResources.ReorderableArrayEntryActive(theme);
                    _BoxEntryOdd.normal.scaledBackgrounds = new Texture2D[1] {_BoxEntryOdd.normal.background};
                    _BoxEntryOdd.onNormal.scaledBackgrounds = new Texture2D[1] {_BoxEntryOdd.onNormal.background};

                    _BoxEntryOdd.active.textColor = textColor;
                    _BoxEntryOdd.onActive.textColor = textColor;
                    _BoxEntryOdd.active.background = _BoxEntryOdd.onNormal.background;
                    _BoxEntryOdd.onActive.background = _BoxEntryOdd.onNormal.background;
                    _BoxEntryOdd.active.scaledBackgrounds = new Texture2D[1] {_BoxEntryOdd.onNormal.background};
                    _BoxEntryOdd.onActive.scaledBackgrounds = new Texture2D[1] {_BoxEntryOdd.onNormal.background};

                    _BoxEntryOdd.focused.textColor = textColor;
                    _BoxEntryOdd.onFocused.textColor = textColor;
                    _BoxEntryOdd.focused.background = EditorResources.ReorderableArrayEntryFocus(theme);
                    _BoxEntryOdd.onFocused.background = _BoxEntryOdd.focused.background;
                    _BoxEntryOdd.focused.scaledBackgrounds = new Texture2D[1] {_BoxEntryOdd.focused.background};
                    _BoxEntryOdd.onFocused.scaledBackgrounds = new Texture2D[1] {_BoxEntryOdd.focused.background};

                    _BoxEntryOdd.hover.textColor = textColor;
                    _BoxEntryOdd.onHover.textColor = textColor;
                    _BoxEntryOdd.hover.background = _BoxEntryOdd.normal.background;
                    _BoxEntryOdd.onHover.background = _BoxEntryOdd.normal.background;
                    _BoxEntryOdd.hover.scaledBackgrounds = new Texture2D[1] {_BoxEntryOdd.normal.background};
                    _BoxEntryOdd.onHover.scaledBackgrounds = new Texture2D[1] {_BoxEntryOdd.normal.background};
                }

                return _BoxEntryOdd;
            }
        }

        private static GUIStyle _BoldFoldout;

        public static GUIStyle BoldFoldout
        {
            get
            {
                if (_BoldFoldout == null)
                {
                    _BoldFoldout = new GUIStyle(EditorStyles.foldout);
                    _BoldFoldout.padding = new RectOffset(14, 0, 3, 3);
                    _BoldFoldout.fontStyle = FontStyle.Bold;
                }

                return _BoldFoldout;
            }
        }
    }
}