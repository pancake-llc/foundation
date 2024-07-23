using System;
using UnityEditor;
using UnityEngine;
using Event = UnityEngine.Event;

namespace Sisus.Init.EditorOnly
{
    internal sealed class AdvancedDropdownGUI
    {
        private static readonly GUIContent groupName = new("", "");
        private static readonly Color separatorColor = new(0.5f, 0.5f, 0.5f, 0.2f);

        private static class Styles
        {
            public static GUIStyle header = "In BigTitle";
            public static GUIStyle rightArrow = "AC RightArrow";
            public static GUIStyle leftArrow = "AC LeftArrow";

            static Styles()
            {
                header.font = EditorStyles.boldLabel.font;
                header.margin = new RectOffset(0, 0, 0, 0);
            }
        }
        
        
        private Rect searchRect;
        public float HeaderHeight => searchRect.height;
        public const float WindowHeight = 395 - 80;
        private Vector2 iconSize = new Vector2(16, 16);
        public Vector2 IconSize => iconSize;
                
        public void DrawItem(AdvancedDropdownItem item, bool selected, bool hasSearch)
        {
            if(item.IsSeparator())
            {
                DrawLineSeparator();
                return;
            }

            var content = !hasSearch ? item.Content : item.ContentWhenSearching;
            var rect = GUILayoutUtility.GetRect(content, item.lineStyle, GUILayout.ExpandWidth(true));
            if(item.IsSeparator() || Event.current.type != EventType.Repaint)
            {
                return;
            }

            // add offset if has no icon
            if(content.image == null)
            {
                item.lineStyle.Draw(rect, GUIContent.none, false, false, selected, selected);
                rect.x += IconSize.x;
                rect.width -= IconSize.x;
            }

            item.lineStyle.Draw(rect, content, false, false, selected, selected);

            if(!item.drawArrow)
            {
                return;
            }

            var size = Styles.rightArrow.lineHeight;
            var yOffset = item.lineStyle.fixedHeight / 2 - size / 2;
            Rect arrowRect = new Rect(rect.x + rect.width - size, rect.y + yOffset, size, size);
            Styles.rightArrow.Draw(arrowRect, false, false, false, false);
        }

        private void DrawLineSeparator()
        {
            var rect = GUILayoutUtility.GetRect(Screen.width, 5f);
            rect.y += 2f;
            rect.height = 1f;
            rect.x += 10f;
            rect.width -= 20f;
            EditorGUI.DrawRect(rect, separatorColor);
        }

        public void DrawHeader(AdvancedDropdownItem group, Action backButtonPressed)
        {
            groupName.text = group.Name;
            var headerRect = GUILayoutUtility.GetRect(groupName, Styles.header, GUILayout.ExpandWidth(true));
            headerRect.width -= 2f; // hack needed to fix rect clipping over white outline that surrounds the window

            if(Event.current.type == EventType.Repaint)
            {
                Styles.header.Draw(headerRect, groupName, false, false, false, false);
            }

            if(group.Parent == null)
            {
                return;
            }

            var arrowSize = 13;
            var y = headerRect.y + (headerRect.height / 2 - arrowSize / 2);
            var arrowRect = new Rect(headerRect.x + 4, y, arrowSize, arrowSize);
            if(Event.current.type == EventType.Repaint)
            {
                Styles.leftArrow.Draw(arrowRect, false, false, false, false);
            }
            else if(Event.current.type == EventType.MouseDown && headerRect.Contains(Event.current.mousePosition))
            {
                backButtonPressed();
                Event.current.Use();
            }
        }

        public void DrawSearchField(bool isSearchFieldDisabled, string searchString, Action<string> searchChanged)
        {
            if(!isSearchFieldDisabled)
            {
                EditorGUI.FocusTextInControl("TypeSearch");
            }

            using(new EditorGUI.DisabledScope(isSearchFieldDisabled))
            {
                GUI.SetNextControlName("TypeSearch");

                var newSearch = DrawSearchFieldControl(searchString);

                if(newSearch != searchString)
                {
                    searchChanged(newSearch);
                }
            }
        }

        internal string DrawSearchFieldControl(string searchString)
        {
            float padding = 8f;
            searchRect = GUILayoutUtility.GetRect(0f, 0f);
            searchRect.x += padding;
            searchRect.y = 7f;
            searchRect.width -= padding * 2f;
            searchRect.height = 30f;
            var newSearch = EditorGUI.TextField(searchRect, searchString, EditorStyles.toolbarSearchField);
            return newSearch;
        }

        public Rect GetAnimRect(Rect position, float anim)
        {
            var rect = new Rect(position);
            rect.x = position.width * anim;
            rect.y = HeaderHeight;
            rect.height -= HeaderHeight;
            rect.x += 1f;
            return rect;
        }
    }
}