using UnityEditor;
using UnityEngine;

namespace Pancake.ExLibEditor
{
    public static class ExEditorStyles
    {
        private static GUIStyle arrayLabel;

        public static GUIStyle Label
        {
            get
            {
                if (arrayLabel == null)
                {
                    arrayLabel = new GUIStyle();

                    arrayLabel.fontSize = 12;
                    arrayLabel.fontStyle = FontStyle.Normal;
                    arrayLabel.alignment = TextAnchor.MiddleLeft;

                    Color32 textColor = EditorGUIUtility.isProSkin ? new Color32(200, 200, 200, 255) : new Color32(3, 3, 3, 255);

                    arrayLabel.normal.textColor = textColor;
                    arrayLabel.onNormal.textColor = textColor;
                    arrayLabel.active.textColor = textColor;
                    arrayLabel.onActive.textColor = textColor;
                    arrayLabel.focused.textColor = textColor;
                    arrayLabel.onFocused.textColor = textColor;
                    arrayLabel.hover.textColor = textColor;
                    arrayLabel.onHover.textColor = textColor;
                }

                return arrayLabel;
            }
        }

        private static GUIStyle arrayLabelBold;

        public static GUIStyle LabelBold
        {
            get
            {
                if (arrayLabelBold == null)
                {
                    arrayLabelBold = new GUIStyle(Label);
                    arrayLabelBold.fontStyle = FontStyle.Bold;
                }

                return arrayLabelBold;
            }
        }

        private static GUIStyle arrayHeader;

        public static GUIStyle ArrayHeader
        {
            get
            {
                if (arrayHeader == null)
                {
                    arrayHeader = new GUIStyle();

                    arrayHeader.fontSize = 12;
                    arrayHeader.fontStyle = FontStyle.Bold;
                    arrayHeader.alignment = TextAnchor.MiddleLeft;
                    arrayHeader.border = new RectOffset(2, 2, 2, 2);
                    arrayHeader.padding = new RectOffset(10, 0, 0, 1);

                    string theme = EditorGUIUtility.isProSkin ? "DarkTheme" : "LightTheme";
                    Color32 textColor = EditorGUIUtility.isProSkin ? new Color32(200, 200, 200, 255) : new Color32(3, 3, 3, 255);

                    arrayHeader.normal.textColor = textColor;
                    arrayHeader.normal.background = EditorResources.ReorderableArrayNormal(theme);
                    arrayHeader.normal.scaledBackgrounds = new Texture2D[1] {arrayHeader.normal.background};
                }

                return arrayHeader;
            }
        }

        private static GUIStyle arrayEntryBkg;

        public static GUIStyle ArrayEntryBkg
        {
            get
            {
                if (arrayEntryBkg == null)
                {
                    arrayEntryBkg = new GUIStyle();

                    arrayEntryBkg.fontSize = 12;
                    arrayEntryBkg.fontStyle = FontStyle.Normal;
                    arrayEntryBkg.alignment = TextAnchor.MiddleLeft;
                    arrayEntryBkg.border = new RectOffset(2, 2, 2, 2);
                    arrayEntryBkg.padding = new RectOffset(10, 0, 0, 1);

                    string theme = EditorGUIUtility.isProSkin ? "DarkTheme" : "LightTheme";
                    Color32 textColor = EditorGUIUtility.isProSkin ? new Color32(200, 200, 200, 255) : new Color32(3, 3, 3, 255);

                    arrayEntryBkg.normal.textColor = textColor;
                    arrayEntryBkg.normal.background = EditorResources.ReorderableArrayEntryBackground(theme);
                    arrayEntryBkg.normal.scaledBackgrounds = new Texture2D[1] {arrayEntryBkg.normal.background};

                    arrayEntryBkg.active.textColor = textColor;
                    arrayEntryBkg.active.background = arrayEntryBkg.normal.background;
                    arrayEntryBkg.active.scaledBackgrounds = new Texture2D[1] {arrayEntryBkg.normal.background};

                    arrayEntryBkg.focused.textColor = textColor;
                    arrayEntryBkg.focused.background = arrayEntryBkg.normal.background;
                    arrayEntryBkg.focused.scaledBackgrounds = new Texture2D[1] {arrayEntryBkg.normal.background};

                    arrayEntryBkg.hover.textColor = textColor;
                    arrayEntryBkg.hover.background = arrayEntryBkg.normal.background;
                    arrayEntryBkg.hover.scaledBackgrounds = new Texture2D[1] {arrayEntryBkg.normal.background};
                }

                return arrayEntryBkg;
            }
        }

        private static GUIStyle arrayButton;

        public static GUIStyle ArrayButton
        {
            get
            {
                if (arrayButton == null)
                {
                    arrayButton = new GUIStyle(ArrayHeader);

                    string theme = EditorGUIUtility.isProSkin ? "DarkTheme" : "LightTheme";
                    Color32 textColor = EditorGUIUtility.isProSkin ? new Color32(200, 200, 200, 255) : new Color32(3, 3, 3, 255);

                    arrayButton.active.textColor = textColor;
                    arrayButton.onActive.textColor = textColor;
                    arrayButton.active.background = EditorResources.ReorderableArrayPress(theme);
                    arrayButton.onActive.background = arrayButton.active.background;
                    arrayButton.active.scaledBackgrounds = new Texture2D[1] {arrayButton.active.background};
                    arrayButton.onActive.scaledBackgrounds = new Texture2D[1] {arrayButton.active.background};

                    arrayButton.focused.textColor = textColor;
                    arrayButton.onFocused.textColor = textColor;
                    arrayButton.focused.background = EditorResources.ReorderableArrayHover(theme);
                    arrayButton.onFocused.background = arrayHeader.focused.background;
                    arrayButton.focused.scaledBackgrounds = new Texture2D[1] {arrayButton.focused.background};
                    arrayButton.onFocused.scaledBackgrounds = new Texture2D[1] {arrayButton.focused.background};

                    arrayButton.hover.textColor = textColor;
                    arrayButton.onHover.textColor = textColor;
                    arrayButton.hover.background = EditorResources.ReorderableArrayHover(theme);
                    arrayButton.onHover.background = arrayHeader.hover.background;
                    arrayButton.hover.scaledBackgrounds = new Texture2D[1] {arrayButton.hover.background};
                    arrayButton.onHover.scaledBackgrounds = new Texture2D[1] {arrayButton.hover.background};
                }

                return arrayButton;
            }
        }

        private static GUIStyle arrayCenteredButton;

        public static GUIStyle ArrayCenteredButton
        {
            get
            {
                if (arrayCenteredButton == null)
                {
                    arrayCenteredButton = new GUIStyle(ArrayButton);
                    arrayCenteredButton.fontStyle = FontStyle.Normal;
                    arrayCenteredButton.alignment = TextAnchor.MiddleCenter;
                    arrayCenteredButton.padding = new RectOffset(0, 0, 0, 0);
                }

                return arrayCenteredButton;
            }
        }

        private static GUIStyle arrayEntryEven;

        public static GUIStyle ArrayEntryEven
        {
            get
            {
                if (arrayEntryEven == null)
                {
                    arrayEntryEven = new GUIStyle();

                    arrayEntryEven.fontSize = 12;
                    arrayEntryEven.fontStyle = FontStyle.Normal;
                    arrayEntryEven.alignment = TextAnchor.MiddleLeft;
                    arrayEntryEven.border = new RectOffset(2, 2, 2, 2);
                    arrayEntryEven.padding = new RectOffset(10, 0, 0, 1);

                    string theme = EditorGUIUtility.isProSkin ? "DarkTheme" : "LightTheme";
                    Color32 textColor = EditorGUIUtility.isProSkin ? new Color32(200, 200, 200, 255) : new Color32(3, 3, 3, 255);

                    arrayEntryEven.normal.textColor = textColor;
                    arrayEntryEven.onNormal.textColor = textColor;
                    arrayEntryEven.normal.background = EditorResources.ReorderableArrayEntryEven(theme);
                    arrayEntryEven.onNormal.background = EditorResources.ReorderableArrayEntryActive(theme);
                    arrayEntryEven.normal.scaledBackgrounds = new Texture2D[1] {arrayEntryEven.normal.background};
                    arrayEntryEven.onNormal.scaledBackgrounds = new Texture2D[1] {arrayEntryEven.onNormal.background};

                    arrayEntryEven.active.textColor = textColor;
                    arrayEntryEven.onActive.textColor = textColor;
                    arrayEntryEven.active.background = arrayEntryEven.onNormal.background;
                    arrayEntryEven.onActive.background = arrayEntryEven.onNormal.background;
                    arrayEntryEven.active.scaledBackgrounds = new Texture2D[1] {arrayEntryEven.onNormal.background};
                    arrayEntryEven.onActive.scaledBackgrounds = new Texture2D[1] {arrayEntryEven.onNormal.background};

                    arrayEntryEven.focused.textColor = textColor;
                    arrayEntryEven.onFocused.textColor = textColor;
                    arrayEntryEven.focused.background = EditorResources.ReorderableArrayEntryFocus(theme);
                    arrayEntryEven.onFocused.background = arrayEntryEven.focused.background;
                    arrayEntryEven.focused.scaledBackgrounds = new Texture2D[1] {arrayEntryEven.focused.background};
                    arrayEntryEven.onFocused.scaledBackgrounds = new Texture2D[1] {arrayEntryEven.focused.background};

                    arrayEntryEven.hover.textColor = textColor;
                    arrayEntryEven.onHover.textColor = textColor;
                    arrayEntryEven.hover.background = arrayEntryEven.normal.background;
                    arrayEntryEven.onHover.background = arrayEntryEven.normal.background;
                    arrayEntryEven.hover.scaledBackgrounds = new Texture2D[1] {arrayEntryEven.normal.background};
                    arrayEntryEven.onHover.scaledBackgrounds = new Texture2D[1] {arrayEntryEven.normal.background};
                }

                return arrayEntryEven;
            }
        }

        private static GUIStyle arrayEntryOdd;

        public static GUIStyle ArrayEntryOdd
        {
            get
            {
                if (arrayEntryOdd == null)
                {
                    arrayEntryOdd = new GUIStyle();

                    arrayEntryOdd.fontSize = 12;
                    arrayEntryOdd.fontStyle = FontStyle.Normal;
                    arrayEntryOdd.alignment = TextAnchor.MiddleLeft;
                    arrayEntryOdd.border = new RectOffset(2, 2, 2, 2);
                    arrayEntryOdd.padding = new RectOffset(10, 0, 0, 1);

                    string theme = EditorGUIUtility.isProSkin ? "DarkTheme" : "LightTheme";
                    Color32 textColor = EditorGUIUtility.isProSkin ? new Color32(200, 200, 200, 255) : new Color32(3, 3, 3, 255);

                    arrayEntryOdd.normal.textColor = textColor;
                    arrayEntryOdd.onNormal.textColor = textColor;
                    arrayEntryOdd.normal.background = EditorResources.ReorderableArrayEntryOdd(theme);
                    arrayEntryOdd.onNormal.background = EditorResources.ReorderableArrayEntryActive(theme);
                    arrayEntryOdd.normal.scaledBackgrounds = new Texture2D[1] {arrayEntryOdd.normal.background};
                    arrayEntryOdd.onNormal.scaledBackgrounds = new Texture2D[1] {arrayEntryOdd.onNormal.background};

                    arrayEntryOdd.active.textColor = textColor;
                    arrayEntryOdd.onActive.textColor = textColor;
                    arrayEntryOdd.active.background = arrayEntryOdd.onNormal.background;
                    arrayEntryOdd.onActive.background = arrayEntryOdd.onNormal.background;
                    arrayEntryOdd.active.scaledBackgrounds = new Texture2D[1] {arrayEntryOdd.onNormal.background};
                    arrayEntryOdd.onActive.scaledBackgrounds = new Texture2D[1] {arrayEntryOdd.onNormal.background};

                    arrayEntryOdd.focused.textColor = textColor;
                    arrayEntryOdd.onFocused.textColor = textColor;
                    arrayEntryOdd.focused.background = EditorResources.ReorderableArrayEntryFocus(theme);
                    arrayEntryOdd.onFocused.background = arrayEntryOdd.focused.background;
                    arrayEntryOdd.focused.scaledBackgrounds = new Texture2D[1] {arrayEntryOdd.focused.background};
                    arrayEntryOdd.onFocused.scaledBackgrounds = new Texture2D[1] {arrayEntryOdd.focused.background};

                    arrayEntryOdd.hover.textColor = textColor;
                    arrayEntryOdd.onHover.textColor = textColor;
                    arrayEntryOdd.hover.background = arrayEntryOdd.normal.background;
                    arrayEntryOdd.onHover.background = arrayEntryOdd.normal.background;
                    arrayEntryOdd.hover.scaledBackgrounds = new Texture2D[1] {arrayEntryOdd.normal.background};
                    arrayEntryOdd.onHover.scaledBackgrounds = new Texture2D[1] {arrayEntryOdd.normal.background};
                }

                return arrayEntryOdd;
            }
        }
    }
}