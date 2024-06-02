using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PancakeEditor.Common
{
    public struct Uniform
    {
        #region field

        private static GUIStyle contentList;
        private static GUIStyle contentListBlue;
        private static GUIStyle contentListDark;
        private static GUIStyle contentBox;
        private static GUIStyle box;
        private static GUIStyle foldoutButton;
        private static GUIStyle foldoutIcon;
        private static GUIStyle installedIcon;
        private static GUIStyle headerLabel;
        private static GUIStyle richCenterLabel;

        private static readonly Dictionary<string, GUIContent> CachedIconContent = new();
        private static readonly UniformFoldoutState FoldoutSettings = new();
        public static string Theme => EditorGUIUtility.isProSkin ? "DarkTheme" : "LightTheme";

        #endregion


        #region prop

        public static GUIStyle ContentList
        {
            get
            {
                if (contentList != null) return contentList;
                contentList = new GUIStyle {border = new RectOffset(2, 2, 2, 2), normal = {background = EditorResources.EvenBackground}};
                return contentList;
            }
        }

        public static GUIStyle BoxContent
        {
            get
            {
                if (contentBox != null) return contentBox;
                contentBox = new GUIStyle
                {
                    border = new RectOffset(2, 2, 2, 2),
                    normal = {background = EditorResources.BoxContentDark, scaledBackgrounds = new[] {EditorResources.BoxContentDark}}
                };
                return contentBox;
            }
        }

        public static GUIStyle Box
        {
            get
            {
                if (box != null) return box;
                box = new GUIStyle
                {
                    border = new RectOffset(2, 2, 2, 2),
                    margin = new RectOffset(2, 2, 2, 2),
                    normal = {background = EditorResources.BoxBackgroundDark, scaledBackgrounds = new[] {EditorResources.BoxBackgroundDark}}
                };
                return box;
            }
        }

        public static GUIStyle FoldoutButton
        {
            get
            {
                if (foldoutButton != null) return foldoutButton;

                foldoutButton = new GUIStyle
                {
                    normal = new GUIStyleState {textColor = Color.white},
                    padding = new RectOffset(0, 0, 3, 3),
                    stretchWidth = true,
                    richText = true,
                    fontSize = 11,
                    fontStyle = FontStyle.Bold
                };

                return foldoutButton;
            }
        }

        public static GUIStyle FoldoutIcon
        {
            get
            {
                if (foldoutIcon != null) return foldoutIcon;

                foldoutIcon = new GUIStyle {padding = new RectOffset(0, 0, 5, 0)};

                return foldoutIcon;
            }
        }

        public static GUIStyle InstalledIcon
        {
            get
            {
                if (installedIcon != null) return installedIcon;

                installedIcon = new GUIStyle {padding = new RectOffset(0, 0, 3, 0), fixedWidth = 30, fixedHeight = 30};

                return installedIcon;
            }
        }

        public static GUIStyle HeaderLabel
        {
            get
            {
                if (headerLabel != null) return headerLabel;
                headerLabel = new GUIStyle(EditorStyles.label) {fontSize = 13, fontStyle = FontStyle.Bold, padding = new RectOffset(0, 0, 6, 0)};
                return headerLabel;
            }
        }

        public static GUIStyle RichCenterLabel
        {
            get
            {
                if (richCenterLabel != null) return richCenterLabel;
                richCenterLabel = new GUIStyle(EditorStyles.label) {richText = true, alignment = TextAnchor.MiddleCenter};
                return richCenterLabel;
            }
        }

        #endregion


        #region color

        public static readonly Color GothicOlive = new(0.49f, 0.43f, 0.31f);
        public static readonly Color Maroon = new(0.52f, 0.27f, 0.33f);
        public static readonly Color ElegantNavy = new(0.15f, 0.21f, 0.41f);
        public static readonly Color CrystalPurple = new(0.33f, 0.24f, 0.42f);
        public static readonly Color RaisinBlack = new(0.13f, 0.13f, 0.13f);
        public static readonly Color Green = new(0.31f, 0.98f, 0.48f, 0.66f);
        public static readonly Color Orange = new(1f, 0.72f, 0.42f, 0.66f);
        public static readonly Color Red = new(1f, 0.16f, 0.16f, 0.66f);
        public static readonly Color Pink = new(1f, 0.47f, 0.78f, 0.66f);
        public static readonly Color SunsetOrange = new(0.99f, 0.33f, 0.37f);
        public static readonly Color SolidPink = new(0.55f, 0.2f, 0.23f);
        public static readonly Color ChinesePink = new(0.87f, 0.44f, 0.64f);
        public static readonly Color TwilightLavender = new(0.53f, 0.28f, 0.42f);
        public static readonly Color BabyBlueEyes = new(0.6f, 0.8f, 1f);
        public static readonly Color FluorescentBlue = new(0.2f, 1f, 1f);
        public static readonly Color Yellow = new(0.92f, 0.76f, 0.2f);
        public static readonly Color Jet = new(0.21f, 0.21f, 0.21f);
        public static Color Success => Green;
        public static Color Error => Red;
        public static Color Warning => Orange;
        public static Color Notice => Orange;

        #endregion


        #region draw

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

        /// <summary>
        /// Icon content
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tooltip"></param>
        /// <returns></returns>
        public static GUIContent IconContent(string name, string tooltip = "")
        {
            if (CachedIconContent.TryGetValue(name, out var result)) return result ?? GUIContent.none;
            var builtinIcon = EditorGUIUtility.IconContent(name) ?? new GUIContent(Texture2D.whiteTexture);
            result = new GUIContent(builtinIcon.image, tooltip);
            CachedIconContent.Add(name, result);
            return result;
        }

        /// <summary>
        /// Draw header with title
        /// </summary>
        /// <param name="title"></param>
        public static void DrawFooter(string title)
        {
            var bgStyle = new GUIStyle(GUIStyle.none) {normal = {background = EditorCreator.CreateTexture(RaisinBlack)}};
            GUILayout.BeginVertical(bgStyle, GUILayout.Height(20));

            var titleStyle = new GUIStyle(GUIStyle.none)
            {
                margin = new RectOffset(4, 4, 4, 4),
                padding = new RectOffset(0, 8, 0, 0),
                fontSize = 9,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleRight,
                wordWrap = false,
                richText = true,
                imagePosition = ImagePosition.TextOnly,
                stretchHeight = true,
                stretchWidth = true,
                normal = {textColor = Color.white}
            };

            EditorGUILayout.LabelField(title, titleStyle);
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Draw only the property specified.
        /// </summary>
        /// <param name="serializedObject"></param>
        /// <param name="fieldName"></param>
        /// <param name="isReadOnly"></param>
        public static void DrawOnlyField(SerializedObject serializedObject, string fieldName, bool isReadOnly)
        {
            serializedObject.Update();
            var prop = serializedObject.GetIterator();
            if (prop.NextVisible(true))
            {
                do
                {
                    if (prop.name != fieldName) continue;

                    GUI.enabled = !isReadOnly;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(prop.name), true);
                    GUI.enabled = true;
                } while (prop.NextVisible(false));
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draw property field with validate wrong value
        /// </summary>
        /// <param name="property"></param>
        /// <param name="error"></param>
        /// <param name="warning"></param>
        /// <param name="onError"></param>
        /// <param name="onWarning"></param>
        public static void DrawPropertyFieldValidate(SerializedProperty property, bool error, bool warning = false, Action onError = null, Action onWarning = null)
        {
            if (error) GUI.color = Red;
            else if (warning) GUI.color = Orange;
            EditorGUILayout.PropertyField(property, true);
            GUI.color = Color.white;
            if (error) onError?.Invoke();
            else if (warning) onWarning?.Invoke();
        }

        /// <summary>
        /// Draws a line in the inspector.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="height"></param>
        public static void DrawLine(Color color, float height = 1f)
        {
            var rect = EditorGUILayout.GetControlRect(false, height);
            rect.height = height;
            EditorGUI.DrawRect(rect, color);
        }

        /// <summary>
        /// Draws a line in the inspector.
        /// </summary>
        /// <param name="height"></param>
        public static void DrawLine(float height = 1f) { DrawLine(new Color(0.5f, 0.5f, 0.5f, 1f), height); }

        /// <summary>
        /// Draw a selectable object
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="labels"></param>
        public static void DrawSelectableObject(Object obj, string[] labels)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(labels[0], GUILayout.MaxWidth(300))) EditorGUIUtility.PingObject(obj);

            if (GUILayout.Button(labels[1], GUILayout.MaxWidth(75)))
            {
                EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));
                Selection.activeObject = obj;
                SceneView.FrameLastActiveSceneView();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(2);
        }

        /// <summary>
        /// Draws all properties like base.OnInspectorGUI() but excludes a field by name.
        /// </summary>
        /// <param name="serializedObject"></param>
        /// <param name="fieldToSkip">The name of the field that should be excluded. Example: "m_Script" will skip the default Script field.</param>
        public static void DrawInspectorExcept(SerializedObject serializedObject, string fieldToSkip) { DrawInspectorExcept(serializedObject, new[] {fieldToSkip}); }

        /// <summary>
        /// Draws all properties like base.OnInspectorGUI() but excludes the specified fields by name.
        /// </summary>
        /// <param name="serializedObject"></param>
        /// <param name="fieldsToSkip">
        /// An array of names that should be excluded.
        /// Example: new string[] { "m_Script" , "myInt" } will skip the default Script field and the Integer field myInt.
        /// </param>
        public static void DrawInspectorExcept(SerializedObject serializedObject, string[] fieldsToSkip)
        {
            serializedObject.Update();
            var prop = serializedObject.GetIterator();
            if (prop.NextVisible(true))
            {
                do
                {
                    if (fieldsToSkip.Any(prop.name.Contains)) continue;

                    EditorGUILayout.PropertyField(serializedObject.FindProperty(prop.name), true);
                } while (prop.NextVisible(false));
            }

            serializedObject.ApplyModifiedProperties();
        }

        public static void DrawVariableCustomInspector(SerializedObject serializedObject, string[] fieldsToSkip)
        {
            serializedObject.Update();
            var prop = serializedObject.GetIterator();
            var propertyCount = 0;

            if (prop.NextVisible(true))
            {
                do
                {
                    if (fieldsToSkip.Any(skipProp => prop.name == skipProp)) continue;

                    EditorGUILayout.PropertyField(serializedObject.FindProperty(prop.name), true);
                    propertyCount++;

                    if (propertyCount != 4) continue;

                    //Draw save properties
                    var isSaved = serializedObject.FindProperty("saved");
                    if (!isSaved.boolValue) continue;
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginHorizontal();
                    var guidProperty = serializedObject.FindProperty("guid");
                    var guidCreateMode = serializedObject.FindProperty("guidCreateMode");
                    int index = guidCreateMode.enumValueIndex;
                    EditorGUILayout.PropertyField(guidCreateMode, true);
                    if (index == 0)
                    {
                        GUI.enabled = false;
                        EditorGUILayout.TextField(guidProperty.stringValue);
                        GUI.enabled = true;
                    }
                    else
                    {
                        guidProperty.stringValue = EditorGUILayout.TextField(guidProperty.stringValue);
                    }

                    EditorGUILayout.EndHorizontal();
                    EditorGUI.indentLevel--;
                } while (prop.NextVisible(false));
            }

            serializedObject.ApplyModifiedProperties();
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
            bool foldout = GetFoldoutState(key, defaultFoldout);

            var rect = EditorGUILayout.BeginVertical(GUILayout.MinHeight(foldout ? 20 : 0));
            {
                GUI.Box(rect, GUIContent.none);
                EditorGUILayout.BeginHorizontal();

                var area = EditorGUILayout.BeginVertical();
                {
                    GUI.Box(new Rect(area) {xMin = 18}, GUIContent.none);
                    if (GUILayout.Button($"    {sectionName}", FoldoutButton)) SetFoldoutState(key, !foldout);
                }
                EditorGUILayout.EndVertical();

                var buttonRect = GUILayoutUtility.GetLastRect();
                var iconRect = new Rect(buttonRect.x, buttonRect.y, 10, buttonRect.height);
                GUI.Label(iconRect, foldout ? IconContent("d_IN_foldout_act_on") : IconContent("d_IN_foldout"), FoldoutIcon);

                EditorGUILayout.EndHorizontal();

                if (foldout) GUILayout.Space(4);
                if (foldout && drawer != null) drawer();
            }
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
            bool foldout = GetFoldoutState(key, defaultFoldout);

            var rect = EditorGUILayout.BeginVertical(GUILayout.MinHeight(foldout ? 20 : 0));
            {
                GUI.Box(rect, GUIContent.none);
                EditorGUILayout.BeginHorizontal();

                var area = EditorGUILayout.BeginVertical();
                {
                    GUI.Box(new Rect(area) {xMin = 18}, GUIContent.none);
                    if (GUILayout.Button($"    {sectionName}", FoldoutButton))
                    {
                        if (Event.current.button == 1) actionRightClick?.Invoke();

                        SetFoldoutState(key, !foldout);
                    }
                }
                EditorGUILayout.EndVertical();

                var buttonRect = GUILayoutUtility.GetLastRect();
                var iconRect = new Rect(buttonRect.x, buttonRect.y, 10, buttonRect.height);
                GUI.Label(iconRect, foldout ? IconContent("d_IN_foldout_act_on") : IconContent("d_IN_foldout"), FoldoutIcon);

                EditorGUILayout.EndHorizontal();

                if (foldout) GUILayout.Space(4);
                if (foldout && drawer != null) drawer();
            }

            EditorGUILayout.EndVertical();
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
        /// Centers a rect inside another window.
        /// </summary>
        /// <param name="window"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        public static Rect CenterInWindow(Rect window, Rect origin)
        {
            var pos = window;
            float w = (origin.width - pos.width) * 0.5f;
            float h = (origin.height - pos.height) * 0.5f;
            pos.x = origin.x + w;
            pos.y = origin.y + h;
            return pos;
        }

        /// <summary>
        /// Draw label installed and icon
        /// </summary>
        /// <param name="version"></param>
        /// <param name="labelMargin"></param>
        public static void DrawInstalled(string version, RectOffset labelMargin = null)
        {
            EditorGUILayout.BeginHorizontal();
            labelMargin ??= new RectOffset(0, 0, 0, 0);
            GUILayout.Label(version, new GUIStyle(EditorStyles.linkLabel) {alignment = TextAnchor.MiddleLeft, margin = labelMargin});
            var lastRect = GUILayoutUtility.GetLastRect();
            var iconRect = new Rect(lastRect.x + EditorStyles.label.CalcSize(new GUIContent(version)).x + 2f, lastRect.y + 1.3f, 10, lastRect.height);
            GUI.Label(iconRect, IconContent("CollabNew"), InstalledIcon);
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw slider for min-max value
        /// </summary>
        /// <param name="position"></param>
        /// <param name="property"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="width"></param>
        public static void DrawSlideVector2(ref Rect position, in SerializedProperty property, float min, float max, int width = 5)
        {
            float totalWidth = position.width;
            Vector2 vector = property.vector2Value;

            position.width = EditorGUIUtility.fieldWidth;
            vector.x = EditorGUI.FloatField(position, vector.x);
            if (vector.x < min) vector.x = min;
            else if (vector.x > vector.y) vector.x = vector.y;
            totalWidth -= position.width;

            position.x = position.xMax + width;
            position.width = totalWidth - position.width - (width * 2);
            EditorGUI.MinMaxSlider(position,
                ref vector.x,
                ref vector.y,
                min,
                max);

            position.x = position.xMax + width;
            position.width = EditorGUIUtility.fieldWidth;
            vector.y = EditorGUI.FloatField(position, vector.y);
            if (vector.y > max) vector.y = max;
            else if (vector.y < vector.x) vector.y = vector.x;

            property.vector2Value = vector;
        }

        /// <summary>
        /// Draw slider for min-max int value
        /// </summary>
        /// <param name="position"></param>
        /// <param name="property"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="width"></param>
        public static void DrawSilderVector2Int(ref Rect position, in SerializedProperty property, float min, float max, int width = 5)
        {
            float totalWidth = position.width;
            int start = Convert.ToInt32(min);
            int end = Convert.ToInt32(max);
            Vector2Int vectorInt = property.vector2IntValue;

            position.width = EditorGUIUtility.fieldWidth;
            vectorInt.x = EditorGUI.IntField(position, vectorInt.x);
            if (vectorInt.x < start) vectorInt.x = start;
            else if (vectorInt.x > vectorInt.y) vectorInt.x = vectorInt.y;
            totalWidth -= position.width;

            position.x = position.xMax + width;
            position.width = totalWidth - position.width - width * 2;
            float xInt = vectorInt.x;
            float yInt = vectorInt.y;
            EditorGUI.MinMaxSlider(position,
                ref xInt,
                ref yInt,
                start,
                end);
            vectorInt.x = Convert.ToInt32(xInt);
            vectorInt.y = Convert.ToInt32(yInt);

            position.x = position.xMax + width;
            position.width = EditorGUIUtility.fieldWidth;
            vectorInt.y = EditorGUI.IntField(position, vectorInt.y);
            if (vectorInt.y > end) vectorInt.y = end;
            else if (vectorInt.y < vectorInt.x) vectorInt.y = vectorInt.x;

            property.vector2IntValue = vectorInt;
        }

        public static void DrawTitleField(string title, Rect rect = default)
        {
            var area = EditorGUILayout.BeginVertical();
            {
                GUI.Box(rect == default ? new Rect(area) {xMin = 18} : rect, GUIContent.none);
                var targetStyle = new GUIStyle {fontSize = 11, normal = {textColor = Color.white}, alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold};
                if (rect == default) EditorGUILayout.LabelField(title, targetStyle);
                else EditorGUI.LabelField(rect, title, targetStyle);
            }
            EditorGUILayout.EndVertical();
        }

        #endregion


        #region foldout state

        private static bool GetFoldoutState(string key, bool defaultFoldout = true)
        {
            if (!FoldoutSettings.ContainsKey(key)) FoldoutSettings.Add(key, defaultFoldout);
            return FoldoutSettings[key];
        }

        private static void SetFoldoutState(string key, bool state)
        {
            if (!FoldoutSettings.ContainsKey(key))
            {
                FoldoutSettings.Add(key, state);
            }
            else
            {
                FoldoutSettings[key] = state;
            }
        }

        [Serializable]
        public class FoldoutState
        {
            public string key;
            public bool state;
        }

        [Serializable]
        public class UniformFoldoutState
        {
            public List<FoldoutState> uniformFoldouts;

            public UniformFoldoutState() { uniformFoldouts = new List<FoldoutState>(); }

            public bool ContainsKey(string key) { return uniformFoldouts.Any(foldoutState => foldoutState.key.Equals(key)); }

            public void Add(string key, bool value) { uniformFoldouts.Add(new FoldoutState {key = key, state = value}); }

            public bool this[string key]
            {
                get
                {
                    foreach (var foldoutState in uniformFoldouts)
                    {
                        if (foldoutState.key.Equals(key))
                        {
                            return foldoutState.state;
                        }
                    }

                    return false;
                }
                set
                {
                    foreach (var foldoutState in uniformFoldouts)
                    {
                        if (foldoutState.key.Equals(key))
                        {
                            foldoutState.state = value;
                            break;
                        }
                    }
                }
            }
        }

        #endregion
    }
}