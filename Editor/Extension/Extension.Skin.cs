using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    public struct Uniform
    {
        private static readonly Dictionary<string, GUIStyle> CustomStyles = new Dictionary<string, GUIStyle>();
        private static GUIStyle uppercaseSectionHeaderExpand;
        private static GUIStyle uppercaseSectionHeaderCollapse;
        private static GUIStyle toggleButtonToolbar;
        private static GUIStyle boxArea;
        private static GUIStyle textImportant;
        private static GUIStyle htmlText;
        private static GUIStyle button;
        private static GUIStyle buttonItem;
        private static GUIStyle tabButton;
        private static GUIStyle boldFoldout;
        private static GUIStyle actionButton;
        private static GUIStyle headerLablel;
        private static GUIStyle italicLablel;
        private static GUIStyle contentBackground;
        private static GUIStyle header;

        private static GUIStyle contentBox;
        private static GUIStyle box;
        public static GUIStyle TabOnlyOne { get; } = "Tab onlyOne";
        public static GUIStyle TabFirst { get; } = "Tab first";
        public static GUIStyle TabMiddle { get; } = "Tab middle";
        public static GUIStyle TabLast { get; } = "Tab last";

        private const int CHEVRON_ICON_WIDTH = 10;
        private const int CHEVRON_ICON_RIGHT_MARGIN = 5;
        private const float SPACE_HALF_LINE = 2f;
        private const float SPACE_ONE_LINE = 4f;
        private const float SPACE_TWO_LINE = 8f;
        private const float SPACE_THREE_LINE = 12f;
        public const float DEFAULT_LABEL_WIDTH = 120f;

        public static readonly Color Green = new Color(0.18f, 1f, 0.45f, 0.66f);
        public static readonly Color Orange = new Color(1f, 0.46f, 0f, 0.66f);
        public static readonly Color Blue = new Color(0f, 0.75f, 1f, 0.27f);
        public static readonly Color Purple = new Color(1f, 0.56f, 1f, 0.39f);
        public static readonly Color Red = new Color(1f, 0.1f, 0.13f, 0.66f);
        public static readonly Color InspectorLock = new Color(.6f, .6f, .6f, 1);
        public static readonly Color InspectorNullError = new Color(1f, .5f, .5f, 1);

        private static InEditor.ProjectSetting<UniformFoldoutState> FoldoutSettings { get; set; } = new InEditor.ProjectSetting<UniformFoldoutState>();

        public static bool GetFoldoutState<T>(string name)
        {
            var key = $"{nameof(T)}_{name}";
            if (!FoldoutSettings.Settings.ContainsKey(key)) FoldoutSettings.Settings.Add(key, false);
            return FoldoutSettings.Settings[key];
        }

        public static void SetFoldoutState<T>(string name, bool state)
        {
            var key = $"{nameof(T)}_{name}";
            FoldoutSettings.Settings[key] = state;
        }

        public static void LoadFoldoutSetting() { FoldoutSettings.LoadSetting(); }
        public static void SaveFoldoutSetting() { FoldoutSettings.SaveSetting(); }

        public static GUIStyle UppercaseSectionHeaderExpand
        {
            get
            {
                if (uppercaseSectionHeaderExpand == null) uppercaseSectionHeaderExpand = GetCustomStyle("Uppercase Section Header");
                return uppercaseSectionHeaderExpand;
            }
        }

        public static GUIStyle UppercaseSectionHeaderCollapse
        {
            get
            {
                if (uppercaseSectionHeaderCollapse == null)
                    uppercaseSectionHeaderCollapse = new GUIStyle(GetCustomStyle("Uppercase Section Header")) {normal = new GUIStyleState()};
                return uppercaseSectionHeaderCollapse;
            }
        }

        public static GUIStyle ToggleButtonToolbar
        {
            get
            {
                if (toggleButtonToolbar == null) toggleButtonToolbar = new GUIStyle(GetCustomStyle("ToggleButton"));
                return toggleButtonToolbar;
            }
        }

        public static GUIStyle BoxArea
        {
            get
            {
                if (boxArea == null) boxArea = new GUIStyle(GetCustomStyle("BoxArea"));
                return boxArea;
            }
        }

        public static GUIStyle TextImportant
        {
            get
            {
                if (textImportant == null) textImportant = new GUIStyle(EditorStyles.label) {normal = {textColor = Uniform.InspectorNullError}};
                return textImportant;
            }
        }

        public static GUIStyle HtmlText
        {
            get
            {
                if (htmlText == null) htmlText = new GUIStyle(EditorStyles.label);
                htmlText.richText = true;
                return htmlText;
            }
        }

        public static GUIStyle ButtonStyle
        {
            get
            {
                if (button == null)
                {
                    var normalTexture = EditorResources.HeaderTexture;
                    var hoverTexture = EditorResources.HoverTexture;
                    var pressTexture = EditorResources.PressTexture;

                    button = new GUIStyle
                    {
                        alignment = TextAnchor.MiddleCenter,
                        border = new RectOffset(2, 2, 2, 2),
                        normal = {textColor = Color.white, background = normalTexture, scaledBackgrounds = new Texture2D[1] {normalTexture}},
                        onNormal = {background = normalTexture, scaledBackgrounds = new Texture2D[1] {normalTexture}},
                        hover = {textColor = Color.white, background = hoverTexture, scaledBackgrounds = new Texture2D[1] {hoverTexture}},
                        onHover = {background = hoverTexture, scaledBackgrounds = new Texture2D[1] {hoverTexture}},
                        focused = {textColor = Color.white, background = hoverTexture, scaledBackgrounds = new Texture2D[1] {hoverTexture}},
                        onFocused = {background = hoverTexture, scaledBackgrounds = new Texture2D[1] {hoverTexture}},
                        active = {textColor = Color.white, background = pressTexture, scaledBackgrounds = new Texture2D[1] {pressTexture}},
                        onActive = {background = pressTexture, scaledBackgrounds = new Texture2D[1] {pressTexture}}
                    };
                }

                return button;
            }
        }

        public static GUIStyle ButtonItem
        {
            get
            {
                if (buttonItem == null)
                {
                    buttonItem = new GUIStyle(ButtonStyle)
                    {
                        fontSize = 12,
                        alignment = TextAnchor.MiddleLeft,
                        contentOffset = new Vector2(10, 0),
                        normal = {textColor = new Color(0.8f, 0.8f, 0.8f, 1.0f)},
                        onNormal = {textColor = new Color(0.8f, 0.8f, 0.8f, 1.0f)},
                        hover = {textColor = Color.white},
                        onHover = {textColor = Color.white},
                        focused = {textColor = Color.white},
                        onFocused = {textColor = Color.white},
                        active = {textColor = Color.white},
                        onActive = {textColor = Color.white}
                    };
                }

                return buttonItem;
            }
        }

        public static GUIStyle TabButton
        {
            get
            {
                if (tabButton == null)
                {
                    tabButton = new GUIStyle(ButtonStyle)
                    {
                        fontSize = HeaderLabel.fontSize,
                        fontStyle = HeaderLabel.fontStyle,
                        normal = {textColor = HeaderLabel.normal.textColor},
                        active = {textColor = HeaderLabel.active.textColor},
                        focused = {textColor = HeaderLabel.focused.textColor},
                        hover = {textColor = HeaderLabel.hover.textColor},
                        onNormal = {textColor = HeaderLabel.normal.textColor},
                        onActive = {textColor = HeaderLabel.active.textColor},
                        onFocused = {textColor = HeaderLabel.focused.textColor},
                        onHover = {textColor = HeaderLabel.hover.textColor}
                    };
                }

                return tabButton;
            }
        }

        public static GUIStyle HeaderLabel
        {
            get
            {
                if (headerLablel == null)
                {
                    headerLablel = new GUIStyle(GUI.skin.label) {fontStyle = FontStyle.Bold, fontSize = 12, alignment = TextAnchor.MiddleCenter};
                }

                return headerLablel;
            }
        }

        public static GUIStyle BoldFoldout
        {
            get
            {
                if (boldFoldout == null)
                {
                    boldFoldout = new GUIStyle(EditorStyles.foldout) {contentOffset = new Vector2(2, 0), fontStyle = FontStyle.Bold};
                }

                return boldFoldout;
            }
        }

        public static GUIStyle ActionButton
        {
            get
            {
                if (actionButton == null)
                {
                    actionButton = new GUIStyle(ButtonStyle) {alignment = TextAnchor.MiddleCenter};
                }

                return actionButton;
            }
        }

        public static GUIStyle ItalicLabel
        {
            get
            {
                if (italicLablel == null)
                {
                    italicLablel = new GUIStyle(GUI.skin.label) {fontStyle = FontStyle.Italic};
                }

                return italicLablel;
            }
        }

        public static GUIStyle ContentBackground
        {
            get
            {
                if (contentBackground == null)
                {
                    var texture = EditorResources.ContentBackgroundTexture;

                    contentBackground = new GUIStyle
                    {
                        normal = {background = texture, scaledBackgrounds = new Texture2D[1] {texture}}, border = new RectOffset(2, 2, 2, 2)
                    };
                }

                return contentBackground;
            }
        }

        public static GUIStyle Header
        {
            get
            {
                if (header == null)
                {
                    var texture = EditorResources.HeaderTexture;

                    header = new GUIStyle {normal = {background = texture, scaledBackgrounds = new Texture2D[1] {texture}}, border = new RectOffset(2, 2, 2, 2)};
                }

                return header;
            }
        }

        public static GUIStyle ContentBox
        {
            get
            {
                if (contentBox == null)
                {
                    contentBox = new GUIStyle
                    {
                        border = new RectOffset(2, 2, 2, 2),
                        normal = {background = EditorGUIUtility.isProSkin ? EditorResources.ContentBackgroundDark : EditorResources.ContentBackground},
                    };
                }

                return contentBox;
            }
        }

        public static GUIStyle Box
        {
            get
            {
                if (box == null)
                {
                    box = new GUIStyle
                    {
                        border = new RectOffset(2, 2, 2, 2),
                        normal = {background = EditorGUIUtility.isProSkin ? EditorResources.BoxBackgroundDark : EditorResources.BoxBackground},
                    };
                }

                return box;
            }
        }


        public static GUIStyle GetCustomStyle(string styleName)
        {
            if (CustomStyles.ContainsKey(styleName)) return CustomStyles[styleName];

            if (Skin != null)
            {
                var style = Skin.FindStyle(styleName);

                if (style == null) Debug.LogError("Couldn't find style " + styleName);
                else CustomStyles.Add(styleName, style);

                return style;
            }

            return null;
        }

        public static GUISkin Skin => EditorResources.Skin;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="foldout"></param>
        /// <returns></returns>
        public static Texture2D GetChevronIcon(bool foldout) { return foldout ? EditorResources.ChevronUp : EditorResources.ChevronDown; }

        /// <summary>
        /// Draw group selection with header
        /// </summary>
        /// <param name="key"></param>
        /// <param name="sectionName"></param>
        /// <param name="drawer"></param>
        /// <param name="defaultFoldout"></param>
        public static void DrawUppercaseSection(string key, string sectionName, Action drawer, bool defaultFoldout = true)
        {
            if (!FoldoutSettings.Settings.ContainsKey(key)) FoldoutSettings.Settings.Add(key, defaultFoldout);

            bool foldout = FoldoutSettings.Settings[key];

            EditorGUILayout.BeginVertical(GetCustomStyle("Uppercase Section Box"), GUILayout.MinHeight(foldout ? 30 : 0));
            EditorGUILayout.BeginHorizontal(foldout ? UppercaseSectionHeaderExpand : UppercaseSectionHeaderCollapse);

            // Header label (and button).
            if (GUILayout.Button(sectionName, GetCustomStyle("Uppercase Section Header Label")))
                FoldoutSettings.Settings[key] = !FoldoutSettings.Settings[key];

            // The expand/collapse icon.
            var buttonRect = GUILayoutUtility.GetLastRect();
            var iconRect = new Rect(buttonRect.x + buttonRect.width - CHEVRON_ICON_WIDTH - CHEVRON_ICON_RIGHT_MARGIN,
                buttonRect.y,
                CHEVRON_ICON_WIDTH,
                buttonRect.height);
            GUI.Label(iconRect, GetChevronIcon(foldout), GetCustomStyle("Uppercase Section Header Chevron"));

            EditorGUILayout.EndHorizontal();

            // Draw the section content.
            if (foldout) GUILayout.Space(5);
            if (foldout && drawer != null) drawer();

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draw group selection with header
        /// </summary>
        /// <param name="key"></param>
        /// <param name="sectionName"></param>
        /// <param name="drawer"></param>
        /// <param name="actionRightClick"></param>
        /// <param name="defaultFoldout"></param>
        public static void DrawUppercaseSectionWithRightClick(string key, string sectionName, Action drawer, Action actionRightClick, bool defaultFoldout = true)
        {
            if (!FoldoutSettings.Settings.ContainsKey(key)) FoldoutSettings.Settings.Add(key, defaultFoldout);

            bool foldout = FoldoutSettings.Settings[key];

            EditorGUILayout.BeginVertical(GetCustomStyle("Uppercase Section Box"), GUILayout.MinHeight(foldout ? 30 : 0));
            EditorGUILayout.BeginHorizontal(foldout ? UppercaseSectionHeaderExpand : UppercaseSectionHeaderCollapse);

            // Header label (and button).
            if (GUILayout.Button(sectionName, GetCustomStyle("Uppercase Section Header Label")))
            {
                if (Event.current.button == 1)
                {
                    actionRightClick?.Invoke();
                    return;
                }

                FoldoutSettings.Settings[key] = !FoldoutSettings.Settings[key];
            }

            // The expand/collapse icon.
            var buttonRect = GUILayoutUtility.GetLastRect();
            var iconRect = new Rect(buttonRect.x + buttonRect.width - CHEVRON_ICON_WIDTH - CHEVRON_ICON_RIGHT_MARGIN,
                buttonRect.y,
                CHEVRON_ICON_WIDTH,
                buttonRect.height);
            GUI.Label(iconRect, GetChevronIcon(foldout), GetCustomStyle("Uppercase Section Header Chevron"));

            EditorGUILayout.EndHorizontal();

            // Draw the section content.
            if (foldout) GUILayout.Space(5);
            if (foldout && drawer != null) drawer();

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Icon content
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tooltip"></param>
        /// <returns></returns>
        public static GUIContent IconContent(string name, string tooltip)
        {
            var builtinIcon = EditorGUIUtility.IconContent(name);
            return new GUIContent(builtinIcon.image, tooltip);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldTitle"></param>
        /// <param name="text"></param>
        /// <param name="labelWidth"></param>
        /// <param name="textFieldWidthOption"></param>
        /// <returns></returns>
        public static string DrawTextField(string fieldTitle, string text, GUILayoutOption labelWidth, GUILayoutOption textFieldWidthOption = null)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(4);
            EditorGUILayout.LabelField(new GUIContent(fieldTitle), labelWidth);
            GUILayout.Space(4);
            text = textFieldWidthOption == null ? GUILayout.TextField(text) : GUILayout.TextField(text, textFieldWidthOption);
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
            GUILayout.Space(4);

            return text;
        }

        /// <summary>
        /// Draw vertical group
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="options"></param>
        public static void Vertical(Action callback, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginVertical(options);
            callback();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draw vertical group
        /// </summary>
        /// <param name="style"></param>
        /// <param name="callback"></param>
        public static void Vertical(GUIStyle style, Action callback)
        {
            EditorGUILayout.BeginVertical(style);
            callback();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draw vertical group
        /// </summary>
        /// <param name="style"></param>
        /// <param name="callback"></param>
        /// <param name="options"></param>
        public static void Vertical(GUIStyle style, Action callback, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginVertical(style, options);
            callback();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draw vertical group
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="options"></param>
        public static void VerticalScope(Action callback, params GUILayoutOption[] options)
        {
            using (new EditorGUILayout.VerticalScope(options))
            {
                callback();
            }
        }

        /// <summary>
        /// Draw vertical group
        /// </summary>
        /// <param name="style"></param>
        /// <param name="callback"></param>
        public static void VerticalScope(GUIStyle style, Action callback)
        {
            using (new EditorGUILayout.VerticalScope(style))
            {
                callback();
            }
        }

        /// <summary>
        /// Draw vertical group
        /// </summary>
        /// <param name="style"></param>
        /// <param name="callback"></param>
        /// <param name="options"></param>
        public static void VerticalScope(GUIStyle style, Action callback, params GUILayoutOption[] options)
        {
            using (new EditorGUILayout.VerticalScope(style, options))
            {
                callback();
            }
        }

        /// <summary>
        /// Draw horizontal group
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="options"></param>
        public static void Horizontal(Action callback, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginHorizontal(options);
            callback();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw horizontal group
        /// </summary>
        /// <param name="style"></param>
        /// <param name="callback"></param>
        public static void Horizontal(GUIStyle style, Action callback)
        {
            EditorGUILayout.BeginHorizontal(style);
            callback();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw horizontal group
        /// </summary>
        /// <param name="style"></param>
        /// <param name="callback"></param>
        /// <param name="options"></param>
        public static void Horizontal(GUIStyle style, Action callback, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginHorizontal(style, options);
            callback();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw horizontal scope
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="options"></param>
        public static void HorizontalScope(Action callback, params GUILayoutOption[] options)
        {
            using (new EditorGUILayout.HorizontalScope(options))
            {
                callback();
            }
        }

        /// <summary>
        /// Draw horizontal scope
        /// </summary>
        /// <param name="style"></param>
        /// <param name="callback"></param>
        public static void HorizontalScope(GUIStyle style, Action callback)
        {
            using (new EditorGUILayout.HorizontalScope(style))
            {
                callback();
            }
        }

        /// <summary>
        /// Draw horizontal scope
        /// </summary>
        /// <param name="style"></param>
        /// <param name="callback"></param>
        /// <param name="options"></param>
        public static void HorizontalScope(GUIStyle style, Action callback, params GUILayoutOption[] options)
        {
            using (new EditorGUILayout.HorizontalScope(style, options))
            {
                callback();
            }
        }

        /// <summary>
        /// Create button in editor gui
        /// </summary>
        /// <param name="label"></param>
        /// <param name="callback"></param>
        /// <param name="color"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static bool Button(string label, Action callback = null, Color? color = null, params GUILayoutOption[] options)
        {
            return Button(new GUIContent(label), callback, color, options);
        }

        /// <summary>
        /// Create button in editor gui
        /// </summary>
        /// <param name="content"></param>
        /// <param name="callback"></param>
        /// <param name="color"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static bool Button(GUIContent content, Action callback = null, Color? color = null, params GUILayoutOption[] options)
        {
            var c = GUI.color;
            GUI.color = color ?? c;
            bool b = GUILayout.Button(content, options);
            if (b) callback?.Invoke();
            GUI.color = c;
            return b;
        }

        /// <summary>
        /// create mini button
        /// </summary>
        /// <param name="label"></param>
        /// <param name="callback"></param>
        /// <param name="color"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static bool MiniButton(string label, Action callback = null, Color? color = null, params GUILayoutOption[] options)
        {
            var c = GUI.color;
            GUI.color = color ?? c;
            bool b = GUILayout.Button(new GUIContent(label), new GUIStyle(EditorStyles.miniButton) {fontSize = 11, font = EditorStyles.label.font}, options);
            if (b) callback?.Invoke();
            GUI.color = c;
            return b;
        }

        /// <summary>
        /// Show panel to pickup folder
        /// </summary>
        /// <param name="pathResult"></param>
        /// <param name="defaultPath"></param>
        /// <param name="keySave"></param>
        /// <param name="options"></param>
        public static void PickFolderPath(ref string pathResult, string defaultPath = "", string keySave = "", params GUILayoutOption[] options)
        {
            GUI.backgroundColor = Color.gray;
            if (GUILayout.Button(IconContent("d_Project", "Pick folder"), options))
            {
                string path = EditorUtility.OpenFolderPanel("Select folder", string.IsNullOrEmpty(pathResult) ? defaultPath : pathResult, "");
                if (!string.IsNullOrEmpty(path))
                {
                    pathResult = path;
                    if (!string.IsNullOrEmpty(keySave))
                    {
                        EditorPrefs.SetString(keySave, pathResult);
                    }
                }

                GUI.FocusControl(null);
            }

            GUI.backgroundColor = Color.white;
        }

        /// <summary>
        /// Show panel to pickup file
        /// </summary>
        /// <param name="pathResult"></param>
        /// <param name="defaultPath"></param>
        /// <param name="extension">extension type of file</param>
        /// <param name="keySave"></param>
        /// <param name="options"></param>
        public static void PickFilePath(ref string pathResult, string defaultPath = "", string extension = "db", string keySave = "", params GUILayoutOption[] options)
        {
            GUI.backgroundColor = Color.gray;
            if (GUILayout.Button(IconContent("d_editicon.sml", "Pick file"), options))
            {
                var path = EditorUtility.OpenFilePanel("Select file", string.IsNullOrEmpty(pathResult) ? defaultPath : pathResult, extension);
                if (!string.IsNullOrEmpty(path))
                {
                    pathResult = path;
                    if (!string.IsNullOrEmpty(keySave)) EditorPrefs.SetString(keySave, pathResult);
                }

                GUI.FocusControl(null);
            }

            GUI.backgroundColor = Color.white;
        }

        /// <summary>
        /// Disable groups
        /// </summary>
        public static void DisabledSection(Action onGUI = null, Func<bool> isDisabled = null)
        {
            EditorGUI.BeginDisabledGroup(isDisabled?.Invoke() ?? true);
            onGUI?.Invoke();
            EditorGUI.EndDisabledGroup();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        public static void HelpBox(string message, MessageType type = MessageType.None) { EditorGUILayout.HelpBox(message, type); }

        public static void Toggle(SerializedProperty property, GUIContent content, float width = 120f, Action optionalDraw = null)
        {
            Horizontal(() =>
            {
                EditorGUILayout.LabelField(content, GUILayout.Width(width));
                property.boolValue = GUILayout.Toggle(property.boolValue, new GUIContent(""));
                optionalDraw?.Invoke();
            });
        }

        public static void SpaceHalfLine() => GUILayout.Space(SPACE_HALF_LINE);
        public static void SpaceOneLine() => GUILayout.Space(SPACE_ONE_LINE);
        public static void SpaceTwoLine() => GUILayout.Space(SPACE_TWO_LINE);
        public static void SpaceThreeLine() => GUILayout.Space(SPACE_THREE_LINE);

        public static void Foldout(Rect rect, Property property)
        {
            var content = property.DisplayNameContent;
            property.IsExpanded = EditorGUI.Foldout(rect, property.IsExpanded, content, true);
        }

        public static void DrawBox(Rect position, GUIStyle style, bool isHover = false, bool isActive = false, bool on = false, bool hasKeyboardFocus = false)
        {
            if (Event.current.type == EventType.Repaint)
            {
                style.Draw(position,
                    GUIContent.none,
                    isHover,
                    isActive,
                    on,
                    hasKeyboardFocus);
            }
        }
    }
}