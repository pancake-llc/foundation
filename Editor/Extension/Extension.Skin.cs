using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    public struct Uniform
    {
        private static readonly Dictionary<string, GUIStyle> CustomStyles = new Dictionary<string, GUIStyle>();

        private static GUIStyle toolboxArea;
        private static GUIStyle htmlText;
        private static GUIStyle buttonStyle;
        private static GUIStyle headerLablel;
        private static GUIStyle italicLablel;

        private static GUIStyle contentBox;
        private static GUIStyle box;
        private static GUIStyle boxWithPadding;
        private static GUIStyle foldoutButton;
        private static GUIStyle cheveron;
        private static GUIStyle groupHeader;
        private static GUIStyle groupHeaderCollapse;
        private static GUIStyle toggleToolbar;

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
            return GetFoldoutState(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetFoldoutState(string key)
        {
            if (!FoldoutSettings.Settings.ContainsKey(key)) FoldoutSettings.Settings.Add(key, false);
            return FoldoutSettings.Settings[key];
        }

        public static void SetFoldoutState<T>(string name, bool state)
        {
            var key = $"{nameof(T)}_{name}";
            SetFoldoutState(key, state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetFoldoutState(string key, bool state) { FoldoutSettings.Settings[key] = state; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoadFoldoutSetting() { FoldoutSettings.LoadSetting(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SaveFoldoutSetting() { FoldoutSettings.SaveSetting(); }

        public static GUIStyle ToggleToolbar
        {
            get
            {
                if (toggleToolbar == null)
                    toggleToolbar = new GUIStyle
                    {
                        normal = new GUIStyleState() {background = EditorResources.ToggleNormalBackground},
                        onNormal = new GUIStyleState {background = EditorResources.ToggleOnNormalBackground},
                        margin = new RectOffset(2, 2, 2, 2),
                        padding = new RectOffset(4, 4, 4, 4),
                        fixedHeight = 24,
                        fixedWidth = 24,
                        stretchWidth = true
                    };
                return toggleToolbar;
            }
        }

        public static GUIStyle ToolboxArea
        {
            get
            {
                if (toolboxArea == null)
                    toolboxArea = new GUIStyle
                    {
                        normal = new GUIStyleState {background = EditorResources.ToolboxAreaNormalBackground},
                        border = new RectOffset(6, 6, 6, 6),
                        padding = new RectOffset(4, 4, 4, 4),
                        richText = true,
                        stretchWidth = true
                    };
                return toolboxArea;
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
                if (buttonStyle == null)
                {
                    var normalTexture = EditorResources.ButtonNormalTexture;
                    var hoverTexture = EditorResources.ButtonHoverTexture;
                    var pressTexture = EditorResources.ButtonPressTexture;

                    buttonStyle = new GUIStyle
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

                return buttonStyle;
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
                        margin = new RectOffset(2, 2, 2, 2),
                        normal = {background = EditorGUIUtility.isProSkin ? EditorResources.BoxBackgroundDark : EditorResources.BoxBackground},
                    };
                }

                return box;
            }
        }

        public static GUIStyle BoxWithPadding
        {
            get
            {
                if (boxWithPadding == null)
                {
                    boxWithPadding = new GUIStyle(Box) {padding = new RectOffset(2, 2, 2, 2),};
                }

                return boxWithPadding;
            }
        }

        public static GUIStyle FoldoutButton
        {
            get
            {
                if (foldoutButton == null)
                {
                    foldoutButton = new GUIStyle
                    {
                        normal = new GUIStyleState {textColor = Color.white},
                        margin = new RectOffset(4, 4, 4, 4),
                        padding = new RectOffset(0, 0, 2, 3),
                        stretchWidth = true,
                        richText = true
                    };
                }

                return foldoutButton;
            }
        }

        public static GUIStyle Cheveron
        {
            get
            {
                if (cheveron == null)
                {
                    cheveron = new GUIStyle {padding = new RectOffset(0, 0, 3, 0)};
                }

                return cheveron;
            }
        }

        public static GUIStyle GroupHeader
        {
            get
            {
                if (groupHeader == null)
                {
                    groupHeader = new GUIStyle
                    {
                        padding = new RectOffset(4, 0, 0, 0), overflow = new RectOffset(0, 0, 3, 3), stretchWidth = true, fixedHeight = 25,
                    };
                }

                return groupHeader;
            }
        }

        public static GUIStyle GroupHeaderCollapse
        {
            get
            {
                if (groupHeaderCollapse == null)
                {
                    groupHeaderCollapse = new GUIStyle(GroupHeader) {normal = new GUIStyleState()};
                }

                return groupHeaderCollapse;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="foldout"></param>
        /// <returns></returns>
        public static Texture2D GetChevronIcon(bool foldout)
        {
            if (EditorGUIUtility.isProSkin) return foldout ? EditorResources.ChevronUpDark : EditorResources.ChevronDownDark;

            return foldout ? EditorResources.ChevronUpLight : EditorResources.ChevronDownLight;
        }

        /// <summary>
        /// Draw group selection with header
        /// </summary>
        /// <param name="key"></param>
        /// <param name="sectionName"></param>
        /// <param name="drawer"></param>
        /// <param name="defaultFoldout"></param>
        public static void DrawGroupFoldout(string key, string sectionName, Action drawer, bool defaultFoldout = true)
        {
            bool foldout = GetFoldoutState(key);

            EditorGUILayout.BeginVertical(BoxWithPadding, GUILayout.MinHeight(foldout ? 30 : 0));
            EditorGUILayout.BeginHorizontal(foldout ? GroupHeader : GroupHeaderCollapse);

            // Header label (and button).
            if (GUILayout.Button(sectionName, FoldoutButton)) SetFoldoutState(key, !foldout);

            // The expand/collapse icon.
            var buttonRect = GUILayoutUtility.GetLastRect();
            var iconRect = new Rect(buttonRect.x + buttonRect.width - CHEVRON_ICON_WIDTH - CHEVRON_ICON_RIGHT_MARGIN,
                buttonRect.y,
                CHEVRON_ICON_WIDTH,
                buttonRect.height);
            GUI.Label(iconRect, GetChevronIcon(foldout), Cheveron);

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
        public static void DrawGroupFoldoutWithRightClick(string key, string sectionName, Action drawer, Action actionRightClick, bool defaultFoldout = true)
        {
            if (!FoldoutSettings.Settings.ContainsKey(key)) FoldoutSettings.Settings.Add(key, defaultFoldout);

            bool foldout = FoldoutSettings.Settings[key];

            EditorGUILayout.BeginVertical(BoxWithPadding, GUILayout.MinHeight(foldout ? 30 : 0));
            EditorGUILayout.BeginHorizontal(foldout ? groupHeader : GroupHeaderCollapse);

            // Header label (and button).
            if (GUILayout.Button(sectionName, FoldoutButton))
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
            GUI.Label(iconRect, GetChevronIcon(foldout), Cheveron);

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