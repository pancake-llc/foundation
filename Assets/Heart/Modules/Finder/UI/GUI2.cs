using System;
using System.Collections.Generic;
using PancakeEditor.Common;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PancakeEditor.Finder
{
    public static class GUI2
    {
        private static GUIStyle miniLabelAlignRight;

        public static Color darkRed = new(0.5f, .0f, 0f, 1f);
        public static Color darkGreen = new(0, .5f, 0f, 1f);
        public static Color darkBlue = new(0, .0f, 0.5f, 1f);
        public static Color lightRed = new(1f, 0.5f, 0.5f, 1f);
        private static readonly GUILayoutOption Glw22 = GUILayout.Width(22f);

        public static GUIStyle MiniLabelAlignRight
        {
            get
            {
                if (miniLabelAlignRight != null) return miniLabelAlignRight;
                return miniLabelAlignRight = new GUIStyle(EditorStyles.miniLabel) {alignment = TextAnchor.MiddleRight};
            }
        }

        public static void Color(Action a, Color c, float? alpha = null)
        {
            if (a == null) return;

            var cColor = GUI.color;
            if (alpha != null) c.a = alpha.Value;

            GUI.color = c;
            a();
            GUI.color = cColor;
        }

        public static void BackgroundColor(Action a, Color c, float? alpha = null)
        {
            if (a == null) return;

            var cColor = GUI.backgroundColor;
            if (alpha != null) c.a = alpha.Value;

            GUI.backgroundColor = c;
            a();
            GUI.backgroundColor = cColor;
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

        public static Object[] DropZone(string title, float w, float h)
        {
            var rect = GUILayoutUtility.GetRect(w, h);
            GUI.Box(rect, GUIContent.none, EditorStyles.textArea);

            float cx = rect.x + w / 2f;
            float cy = rect.y + h / 2f;
            float pz = w / 3f; // plus size

            var plusRect = new Rect(cx - pz / 2f, cy - pz / 2f, pz, pz);
            Color(() => { GUI.DrawTexture(plusRect, Uniform.IconContent("ShurikenPlus").image, ScaleMode.ScaleToFit); }, UnityEngine.Color.white, 0.1f);

            GUI.Label(rect, title, EditorStyles.wordWrappedMiniLabel);

            var eventType = Event.current.type;
            var isAccepted = false;

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

        //        public static bool ColorIconButton(Rect r, Texture icon, Vector2? iconOffset, Color? c)
        //        {
        //            if (c != null) Rect(r, c.Value);
        //            
        //            // align center
        //            if (iconOffset != null)
        //            {
        //                r.x += iconOffset.Value.x;
        //                r.y += iconOffset.Value.y;
        //            }
        //            
        //            return GUI.Button(r, icon, GUIStyle.none);
        //        }

        public static bool ColorIconButton(Rect r, Texture icon, Color? c)
        {
            var oColor = GUI.color;
            if (c != null) GUI.color = c.Value;
            bool result = GUI.Button(r, icon, GUIStyle.none);
            GUI.color = oColor;
            return result;
        }

        public static bool Toggle(ref bool value, GUIContent tex, GUIStyle style, params GUILayoutOption[] options)
        {
            bool vv = GUILayout.Toggle(value, tex, style, options);
            if (vv == value) return false;
            value = vv;
            return true;
        }

        public static bool Toggle(Rect rect, ref bool value)
        {
            bool vv = GUI.Toggle(rect, value, GUIContent.none);
            if (vv == value) return false;
            value = vv;
            return true;
        }

        internal static bool ToolbarToggle(ref bool value, Texture icon, Vector2 padding, string tooltip = null)
        {
            bool vv = GUILayout.Toggle(value, MyGUIContent.Tooltip(tooltip), EditorStyles.toolbarButton, Glw22);

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
    }
}