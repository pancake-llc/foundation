using System;
using System.Collections.Generic;
using Pancake.ExLibEditor;
using UnityEditor;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace PancakeEditor
{
    internal static class GUI2
    {
        private static void Color(Action a, Color c, float? alpha = null)
        {
            if (a == null) return;

            var cColor = GUI.color;
            if (alpha != null) c.a = alpha.Value;

            GUI.color = c;
            a();
            GUI.color = cColor;
        }

        public static void ContentColor(Action a, Color c, float? alpha = null)
        {
            if (a == null) return;

            var cColor = GUI.contentColor;
            if (alpha != null) c.a = alpha.Value;

            GUI.contentColor = c;
            a();
            GUI.contentColor = cColor;
        }


        public static Color Theme(Color proColor, Color indieColor) { return EditorGUIUtility.isProSkin ? proColor : indieColor; }

        public static void Rect(Rect r, Color c, float? alpha = null)
        {
            var cColor = GUI.color;
            if (alpha != null) c.a = alpha.Value;

            GUI.color = c;
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = cColor;
        }

        public static UnityEngine.Object[] DropZone(string title, float w, float h)
        {
            var rect = GUILayoutUtility.GetRect(w, h);
            GUI.Box(rect, GUIContent.none, EditorStyles.textArea);

            var cx = rect.x + w / 2f;
            var cy = rect.y + h / 2f;
            var pz = w / 3f; // plus size

            var plusRect = new Rect(cx - pz / 2f, (cy - pz / 2f), pz, pz);
            Color(() => { GUI.DrawTexture(plusRect, Uniform.IconContent("ShurikenPlus").image, ScaleMode.ScaleToFit); }, UnityEngine.Color.white, 0.1f);

            GUI.Label(rect, title, EditorStyles.wordWrappedMiniLabel);

            EventType eventType = Event.current.type;
            bool isAccepted = false;

            if (eventType == EventType.DragUpdated || eventType == EventType.DragPerform)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (eventType == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    isAccepted = true;
                }

                Event.current.Use();
            }

            return isAccepted ? DragAndDrop.objectReferences : null;
        }

        public static bool ColorIconButton(Rect r, Texture icon, Color? c)
        {
            var oColor = GUI.color;
            if (c != null) GUI.color = c.Value;
            var result = GUI.Button(r, icon, GUIStyle.none);
            GUI.color = oColor;
            return result;
        }

        public static bool Toggle(ref bool value, GUIContent tex, GUIStyle style, params GUILayoutOption[] options)
        {
            var vv = GUILayout.Toggle(value, tex, style, options);
            if (vv == value) return false;
            value = vv;
            return true;
        }

        public static bool Toggle(Rect rect, ref bool value, GUIContent tex)
        {
            var vv = GUI.Toggle(rect, value, tex, GUIStyle.none);
            if (vv == value) return false;
            value = vv;
            return true;
        }

        public static bool Toggle(Rect rect, ref bool value)
        {
            var vv = GUI.Toggle(rect, value, GUIContent.none);
            if (vv == value) return false;
            value = vv;
            return true;
        }

        private static Dictionary<string, GUIContent> tooltipCache = new Dictionary<string, GUIContent>();

        private static GUIContent GetTooltip(string tooltip)
        {
            if (string.IsNullOrEmpty(tooltip)) return GUIContent.none;

            GUIContent result;
            if (tooltipCache.TryGetValue(tooltip, out result)) return result;
            result = new GUIContent(string.Empty, tooltip);
            tooltipCache.Add(tooltip, result);
            return result;
        }

        internal static bool ToolbarToggle(ref bool value, Texture icon, Vector2 padding, string tooltip = null)
        {
            var previousColor = GUI.backgroundColor;
            if (value) GUI.backgroundColor = new Color(0.66f, 0.87f, 1f);
            var vv = GUILayout.Toggle(value, GetTooltip(tooltip), EditorStyles.toolbarButton, GUILayout.Width(22f));
            GUI.backgroundColor = previousColor;

            if (icon != null)
            {
                var rect = GUILayoutUtility.GetLastRect();
                rect = Padding(rect, padding.x, padding.y);
                GUI.DrawTexture(rect, icon, ScaleMode.ScaleToFit);
            }

            if (vv == value) return false;
            value = vv;
            return true;
        }

        public static bool EnumPopup<T>(ref T mode, GUIContent icon, GUIStyle style, params GUILayoutOption[] options)
        {
            var obj = (Enum) (object) mode;
            var cRect = GUILayoutUtility.GetRect(16f, 16f);
            cRect.xMin -= 2f;
            cRect.yMin += 2f;
            GUI.Label(cRect, icon);

            var vv = EditorGUILayout.EnumPopup(obj, style, options);
            if (Equals(vv, obj))
            {
                return false;
            }

            mode = (T) (object) vv;
            return true;
        }

        public static Rect Padding(Rect r, float x, float y) { return new Rect(r.x + x, r.y + y, r.width - 2 * x, r.height - 2 * y); }

        public static Rect LeftRect(float w, ref Rect rect)
        {
            rect.x += w;
            rect.width -= w;
            return new Rect(rect.x - w, rect.y, w, rect.height);
        }

        public static Rect RightRect(float w, ref Rect rect)
        {
            rect.width -= w;
            return new Rect(rect.x + rect.width, rect.y, w, rect.height);
        }

        // -----------------------

        private static GUIStyle miniLabelAlignRight;

        public static GUIStyle MiniLabelAlignRight =>
            miniLabelAlignRight ?? (miniLabelAlignRight = new GUIStyle(EditorStyles.miniLabel) {alignment = TextAnchor.MiddleRight});

        public static Color darkRed = new Color(0.5f, .0f, 0f, 1f);
        public static Color darkGreen = new Color(0, .5f, 0f, 1f);
        public static Color darkBlue = new Color(0, .0f, 0.5f, 1f);
        public static Color lightRed = new Color(1f, 0.5f, 0.5f, 1f);
    }
}