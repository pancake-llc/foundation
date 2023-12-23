using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    public class FoldoutContainer : ListContainer, IFoldoutContainer, IGUIChangedCallback
    {
        private string style;
        private float headerHeight;
        private bool isExpanded;
        private bool hasHover;

        /// <summary>
        /// Foldout container constructor.
        /// </summary>
        /// <param name="name">Foldout container name.</param>
        /// <param name="entities">Foldout container entities.</param>
        public FoldoutContainer(string name, List<VisualEntity> entities)
            : base(name, entities)
        {
            headerHeight = 22;
        }

        /// <summary>
        /// Foldout container constructor.
        /// </summary>
        /// <param name="name">Foldout container name.</param>
        /// <param name="style">Foldout container style.</param>
        /// <param name="entities">Foldout container entities.</param>
        public FoldoutContainer(string name, string style, List<VisualEntity> entities)
            : this(name, entities)
        {
            this.style = style;
        }

        #region [VisualEntity Implementation]

        /// <summary>
        /// Called for rendering and handling foldout container.
        /// </summary>
        /// <param name="position">Rectangle position.</param>
        public override void OnGUI(Rect position)
        {
            switch (style)
            {
                default:
                    OnClassicGUI(position);
                    break;
                case "Highlight":
                    OnHighlightGUI(position);
                    break;
                case "Header":
                    OnHeaderGUI(position);
                    break;
                case "Group":
                    OnGroupGUI(position);
                    break;
            }
        }

        /// <summary>
        /// Total height required to drawing foldout container.
        /// </summary>
        public override float GetHeight()
        {
            switch (style)
            {
                default:
                    return GetClassicHeight();
                case "Highlight":
                    return GetHighlightHeight();
                case "Header":
                    return GetHeaderHeight();
                case "Group":
                    return GetGroupHeight();
            }
        }

        #endregion

        #region [IFoldoutContainer Implementation]

        /// <summary>
        /// Check whether foldout is expanded.
        /// </summary>
        public bool IsExpanded() { return isExpanded; }

        #endregion

        #region [Classic GUI]

        /// <summary>
        /// Called when drawing classic foldout style.
        /// </summary>
        /// <param name="position">Rectangle position.</param>
        protected virtual void OnClassicGUI(Rect position)
        {
            float totalHeight = Mathf.Max(0, position.height - EditorGUIUtility.singleLineHeight);

            position.height = EditorGUIUtility.singleLineHeight;
            using (new EditorGUI.IndentLevelScope(1))
            {
                IsExpanded(EditorGUI.Foldout(position, isExpanded, GetName()));
            }

            if (isExpanded && totalHeight > 0)
            {
                position.y = position.yMax + EditorGUIUtility.standardVerticalSpacing;

                EditorGUI.indentLevel++;
                base.OnGUI(position);
                EditorGUI.indentLevel--;
            }
        }

        /// <summary>
        /// Get height which needed to classic foldout style.
        /// </summary>
        protected virtual float GetClassicHeight()
        {
            float height = EditorGUIUtility.singleLineHeight;
            if (isExpanded)
            {
                height += base.GetHeight() + EditorGUIUtility.standardVerticalSpacing;
            }

            return height;
        }

        #endregion

        #region [Highlight GUI]

        /// <summary>
        /// Called when drawing highlight foldout style.
        /// </summary>
        /// <param name="position">Rectangle position.</param>
        protected virtual void OnHighlightGUI(Rect position)
        {
            float totalHeight = Mathf.Max(0, position.height - EditorGUIUtility.singleLineHeight);

            position.height = EditorGUIUtility.singleLineHeight;

            Event current = Event.current;
            bool isHover = position.Contains(current.mousePosition);
            if (current.type == EventType.MouseDown)
            {
                if (isHover)
                {
                    IsExpanded(!isExpanded);
                }
            }
            else if (current.type == EventType.Repaint)
            {
                EditorStyles.foldoutHeader.Draw(position,
                    GetName(),
                    isHover,
                    false,
                    isExpanded,
                    false);
            }

            if (current.type == EventType.MouseMove && hasHover != isHover)
            {
                Repaint();
                hasHover = isHover;
            }

            if (isExpanded && totalHeight > 0)
            {
                position.y = position.yMax + EditorGUIUtility.standardVerticalSpacing;

                EditorGUI.indentLevel++;
                base.OnGUI(position);
                EditorGUI.indentLevel--;
            }
        }

        /// <summary>
        /// Get height which needed to highlight foldout style.
        /// </summary>
        protected virtual float GetHighlightHeight()
        {
            float height = 20;
            if (isExpanded)
            {
                height += base.GetHeight() + EditorGUIUtility.standardVerticalSpacing;
            }

            return height;
        }

        #endregion

        #region [Header GUI]

        /// <summary>
        /// Called when drawing header foldout style.
        /// </summary>
        /// <param name="position">Rectangle position.</param>
        protected virtual void OnHeaderGUI(Rect position)
        {
            float contentHeight = Mathf.Max(0, position.height - 22);

            position.height = 22;
            if (GUI.Button(position, GUIContent.none, ApexStyles.BoxButton))
            {
                IsExpanded(!isExpanded);
            }

            Event current = Event.current;
            bool isHover = position.Contains(current.mousePosition);
            if (current.type == EventType.Repaint)
            {
                position.x += 4;
                ApexStyles.BoldFoldout.Draw(position,
                    GetName(),
                    isHover,
                    false,
                    isExpanded,
                    false);
                position.x -= 4;
            }

            if (current.type == EventType.MouseMove && hasHover != isHover)
            {
                Repaint();
                hasHover = isHover;
            }

            if (isExpanded && contentHeight > 0)
            {
                position.y = position.yMax + EditorGUIUtility.standardVerticalSpacing;

                using (new BoxScope(ref position))
                {
                    base.OnGUI(position);
                }
            }
        }

        /// <summary>
        /// Get height which needed to header foldout style.
        /// </summary>
        protected virtual float GetHeaderHeight()
        {
            float height = headerHeight;
            if (isExpanded)
            {
                height += base.GetHeight() + EditorGUIUtility.standardVerticalSpacing;
            }

            return height;
        }

        #endregion

        #region [Group GUI]

        /// <summary>
        /// Called when drawing group foldout style.
        /// </summary>
        /// <param name="position">Rectangle position.</param>
        protected virtual void OnGroupGUI(Rect position)
        {
            float contentHeight = position.height - headerHeight;

            position.height = headerHeight;
            if (GUI.Button(position, GUIContent.none, ApexStyles.BoxButton))
            {
                IsExpanded(!isExpanded);
            }

            Event current = Event.current;
            bool isHover = position.Contains(current.mousePosition);
            if (current.type == EventType.Repaint)
            {
                position.x += 4;
                ApexStyles.BoldFoldout.Draw(position,
                    GetName(),
                    isHover,
                    false,
                    isExpanded,
                    false);
                position.x -= 4;
            }

            if (current.type == EventType.MouseMove && hasHover != isHover)
            {
                Repaint();
                hasHover = isHover;
            }

            if (isExpanded && contentHeight > 0)
            {
                position.y = position.yMax - 1;
                position.height = contentHeight;
                GUI.Box(position, GUIContent.none, ApexStyles.BoxEntryBkg);
                position.y += ApexGUIUtility.BoxBounds;

                using (new BoxScope(ref position, false))
                {
                    base.OnGUI(position);
                }
            }
        }

        /// <summary>
        /// Get height which needed to group foldout style.
        /// </summary>
        protected virtual float GetGroupHeight()
        {
            float height = headerHeight;
            if (isExpanded)
            {
                height += (ApexGUIUtility.BoxBounds * 2) + base.GetHeight();
            }

            return height;
        }

        #endregion

        #region [IGUIChangedCallback Implementation]

        /// <summary>
        /// Called when GUI has been changed.
        /// </summary>
        public event Action GUIChanged;

        #endregion

        #region [Event Callback Functions]

        /// <summary>
        /// Called when foldout expanded changed.
        /// </summary>
        public event Action<bool> OnExpanded;

        #endregion

        #region [Getter / Setter]

        public string GetStyle() { return style; }

        public void SetStyle(string value) { style = value; }

        public void IsExpanded(bool value)
        {
            isExpanded = value;
            OnExpanded?.Invoke(value);
            GUIChanged?.Invoke();
        }

        #endregion
    }
}