using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    public class FoldoutContainer : EntityListContainer
    {
        private string style;
        private bool isExpanded;

        /// <summary>
        /// Foldout constructor.
        /// </summary>
        /// <param name="name">Foldout title content.</param>
        /// <param name="children">Foldout children elements.</param>
        public FoldoutContainer(string name, List<VisualEntity> children)
            : base(name, children)
        {
        }

        /// <summary>
        /// Foldout constructor.
        /// </summary>
        /// <param name="title">Foldout title content.</param>
        /// <param name="children">Foldout children elements.</param>
        public FoldoutContainer(string title, string style, List<VisualEntity> children)
            : this(title, children)
        {
            this.style = style;
            menuButtonStyle = Uniform.ActionButton;
            menuIconContent = EditorGUIUtility.IconContent("_Popup@2x");
        }

        /// <summary>
        /// Called for rendering and handling layout element.
        /// </summary>
        /// <param name="position">Rectangle position.</param>
        public override void OnGUI(Rect position)
        {
            if (!string.IsNullOrEmpty(style))
            {
                if (style == "Highlight")
                {
                    OnHighlightStyleGUI(position);
                }
                else if (style == "Group" || style == "Header")
                {
                    OnGroupStyleGUI(position);
                }
            }
            else
            {
                position.height = Mathf.Max(0, position.height - EditorGUIUtility.singleLineHeight);
                if (position.height >= 0)
                {
                    Rect foldoutPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                    isExpanded = EditorGUI.Foldout(foldoutPosition, isExpanded, GetName(), true);
                    Event current = Event.current;
                    if (current.type == EventType.DragUpdated && foldoutPosition.Contains(current.mousePosition))
                    {
                        isExpanded = true;
                    }

                    if (isExpanded && position.height > 0)
                    {
                        position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        if (onChildrenGUI != null)
                        {
                            onChildrenGUI.Invoke(position);
                        }
                        else
                        {
                            base.OnGUI(position);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Implement this method to make custom GUI for Highlight style.
        /// </summary>
        /// <param name="position">Rectangle position.</param>
        protected virtual void OnHighlightStyleGUI(Rect position)
        {
            position.height = Mathf.Max(0, position.height - EditorGUIUtility.singleLineHeight);
            if (position.height >= 0)
            {
                Rect foldoutPosition = new Rect(position.x, position.y, position.width - 4, EditorGUIUtility.singleLineHeight);
                foldoutPosition.xMin += 2;
                foldoutPosition = EditorGUI.IndentedRect(foldoutPosition);
                isExpanded = EditorGUI.BeginFoldoutHeaderGroup(foldoutPosition,
                    isExpanded,
                    GetName(),
                    null,
                    onMenuButtonClick,
                    menuButtonStyle);
                Event current = Event.current;
                if (current.type == EventType.DragUpdated && foldoutPosition.Contains(current.mousePosition))
                {
                    isExpanded = true;
                }

                if (isExpanded && position.height > 0)
                {
                    position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    if (onChildrenGUI != null)
                        onChildrenGUI.Invoke(position);
                    else
                        base.OnGUI(position);
                }

                EditorGUI.EndFoldoutHeaderGroup();
            }
        }

        /// <summary>
        /// Implement this method to make custom GUI for Group style.
        /// </summary>
        /// <param name="position">Rectangle position.</param>
        protected virtual void OnGroupStyleGUI(Rect position)
        {
            using (new ContainerScope(ref position))
            {
                const float HEADER_HEIGHT = 22;
                position.height = Mathf.Max(0, position.height - HEADER_HEIGHT);
                Rect headerPosition = EditorGUI.IndentedRect(position);
                headerPosition.height = HEADER_HEIGHT;

                Rect foldoutButtonPosition = headerPosition;
                if (onMenuButtonClick != null)
                {
                    foldoutButtonPosition.width -= 25;
                    Rect menuButtonPosition = new Rect(foldoutButtonPosition.xMax - 1, foldoutButtonPosition.y, 26, foldoutButtonPosition.height);
                    if (GUI.Button(menuButtonPosition, menuIconContent, menuButtonStyle))
                    {
                        onMenuButtonClick.Invoke(headerPosition);
                    }
                }

                if (GUI.Button(foldoutButtonPosition, GUIContent.none, Uniform.ButtonStyle))
                {
                    isExpanded = !isExpanded;
                }

                Event current = Event.current;
                if (current.type == EventType.DragUpdated && headerPosition.Contains(current.mousePosition))
                {
                    isExpanded = true;
                }

                if (current.type == EventType.Repaint)
                {
                    headerPosition.xMin += 4;
                    Uniform.BoldFoldout.Draw(headerPosition,
                        new GUIContent(GetName()),
                        false,
                        false,
                        isExpanded,
                        false);
                    headerPosition.xMin -= 4;
                }

                if (isExpanded && position.height > 0)
                {
                    EditorGUI.indentLevel++;
                    if (style == "Group")
                    {
                        Rect contentBackgroundPosition = new Rect(headerPosition.x, headerPosition.yMax - 1, headerPosition.width, position.height + 1);
                        GUI.Box(contentBackgroundPosition, GUIContent.none, Uniform.ContentBackground);
                    }

                    Rect childrenPosition = new Rect(position.x, headerPosition.yMax + 2, position.width - 4, position.height);
                    if (onChildrenGUI != null)
                        onChildrenGUI.Invoke(childrenPosition);
                    else
                        base.OnGUI(childrenPosition);
                    EditorGUI.indentLevel--;
                }
            }

            Repaint.Invoke();
        }

        /// <summary>
        /// visual entity height.
        /// </summary>
        public override float GetHeight()
        {
            float height = 0;
            if (isExpanded)
            {
                height += (getChildrenHeight?.Invoke() ?? base.GetHeight()) + EditorGUIUtility.standardVerticalSpacing;
                if (style == "Group")
                {
                    height += 3;
                }
            }

            if (style == "Header" || style == "Group")
            {
                height += 22;
            }
            else
            {
                height += EditorGUIUtility.singleLineHeight;
            }

            return height;
        }

        #region [Callback Functions]

        public Action<Rect> onMenuButtonClick;

        public Action<Rect> onChildrenGUI;

        public Func<float> getChildrenHeight;

        public GUIStyle menuButtonStyle { get; set; }

        public GUIContent menuIconContent { get; set; }

        #endregion

        #region [Getter / Setter]

        public string GetStyle() { return style; }

        public void SetStyle(string value) { style = value; }

        public bool IsExpanded() { return isExpanded; }

        public void IsExpanded(bool value) { isExpanded = value; }

        #endregion
    }
}