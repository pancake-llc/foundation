using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public struct Uniform
    {
        #region field

        private static GUIStyle contentList;
        private static GUIStyle contentListBlue;
        private static GUIStyle contentListDark;
        private static GUIStyle contentBox;
        private static GUIStyle box;

        private static readonly Dictionary<string, GUIContent> CachedIconContent = new Dictionary<string, GUIContent>();

        #endregion


        #region prop

        public static GUIStyle ContentList
        {
            get
            {
                if (contentList == null) contentList = new GUIStyle {border = new RectOffset(2, 2, 2, 2), normal = {background = EditorResources.EvenBackground},};
                return contentList;
            }
        }

        public static GUIStyle ContentListBlue
        {
            get
            {
                if (contentListBlue == null)
                    contentListBlue = new GUIStyle {border = new RectOffset(2, 2, 2, 2), normal = {background = EditorResources.EvenBackgroundBlue},};
                return contentListBlue;
            }
        }

        public static GUIStyle ContentListDark
        {
            get
            {
                if (contentListDark == null)
                    contentListDark = new GUIStyle {border = new RectOffset(2, 2, 2, 2), normal = {background = EditorResources.EvenBackgroundDark},};
                return contentListDark;
            }
        }

        public static GUIStyle ContentBox
        {
            get
            {
                if (contentBox == null)
                    contentBox = new GUIStyle
                    {
                        border = new RectOffset(2, 2, 2, 2),
                        normal = {background = EditorGUIUtility.isProSkin ? EditorResources.ContentBackgroundDark : EditorResources.ContentBackground},
                    };

                return contentBox;
            }
        }

        public static GUIStyle Box
        {
            get
            {
                if (box == null)
                    box = new GUIStyle
                    {
                        border = new RectOffset(2, 2, 2, 2),
                        margin = new RectOffset(2, 2, 2, 2),
                        normal = {background = EditorGUIUtility.isProSkin ? EditorResources.BoxBackgroundDark : EditorResources.BoxBackground},
                    };
                return box;
            }
        }

        #endregion


        #region color

        public static readonly Color Green = new(0.31f, 0.98f, 0.48f, 0.66f);
        public static readonly Color Orange = new(1f, 0.72f, 0.42f, 0.66f);
        public static readonly Color Blue = new(0f, 1f, 0.97f, 0.27f);
        public static readonly Color Purple = new(0.74f, 0.58f, 0.98f, 0.39f);
        public static readonly Color Red = new(1f, 0.16f, 0.16f, 0.66f);
        public static readonly Color Pink = new(1f, 0.47f, 0.78f, 0.66f);
        public static readonly Color CharcoalPurple = new Color(0.04f, 0.05f, 0.11f);

        #endregion

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
        public static void DrawHeader(string title)
        {
            var bgStyle = new GUIStyle(GUIStyle.none) {normal = {background = Editor.CreateTexture(CharcoalPurple)}};
            GUILayout.BeginVertical(bgStyle, GUILayout.Height(60));
            GUILayout.BeginHorizontal();

            var iconStyle = new GUIStyle(GUIStyle.none) {padding = new RectOffset(2, 0, 2, 2)};
            GUILayout.Box(EditorResources.Dreamblale, iconStyle, GUILayout.Width(60), GUILayout.Height(60));

            var titleStyle = new GUIStyle(GUIStyle.none)
            {
                margin = new RectOffset(4, 4, 4, 4),
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false,
                richText = true,
                imagePosition = ImagePosition.ImageLeft,
                fixedHeight = 50,
                stretchHeight = true,
                stretchWidth = true,
                normal = {textColor = Color.white}
            };

            EditorGUILayout.LabelField(title, titleStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
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
        /// Draws a line in the inspector.
        /// </summary>
        /// <param name="height"></param>
        public static void DrawLine(int height = 1)
        {
            var rect = EditorGUILayout.GetControlRect(false, height);
            rect.height = height;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
        }

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
    }
}