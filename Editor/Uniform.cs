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
    }
}