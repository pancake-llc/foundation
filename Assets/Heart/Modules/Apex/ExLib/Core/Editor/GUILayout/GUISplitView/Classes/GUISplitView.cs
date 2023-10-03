using UnityEditor;
using UnityEngine;

namespace Pancake.ExLibEditor
{
    public sealed class GUISplitView : ISplitView
    {
        private static class Colors
        {
            public static Color Separator
            {
                get
                {
                    if (EditorGUIUtility.isProSkin)
                        return new Color(0.1f, 0.1f, 0.1f, 1.0f);
                    else
                        return new Color(0.55f, 0.55f, 0.55f, 1.0f);
                }
            }
        }

        public enum Direction
        {
            Horizontal,
            Vertical
        }

        private Direction splitDirection;
        private float offset;
        private float minPosition;
        private float maxPosition;
        private float splitNormalizedPosition;
        private bool resize;
        private Vector2 scrollPosSideOne;
        private Vector2 scrollPosSideTwo;
        private Rect currentRect;
        private Rect resizeHandleRect;

        /// <summary>
        /// AuroraGUISplitView constructor.
        /// </summary>
        /// <param name="splitDirection">Split view direction.</param>
        public GUISplitView(Direction splitDirection)
        {
            this.splitDirection = splitDirection;
            offset = 0.0f;
            splitNormalizedPosition = 0.25f;
            minPosition = 0.125f;
            maxPosition = 0.5f;
        }

        /// <summary>
        /// AuroraGUISplitView constructor.
        /// </summary>
        /// <param name="splitDirection">Split view direction.</param>
        /// <param name="minPosition">Min possible split view position, ranged: [0...1].</param>
        /// <param name="maxPosition">Max possible split view position, ranged: [0...1].</param>
        public GUISplitView(Direction splitDirection, float minPosition, float maxPosition)
            : this(splitDirection)
        {
            splitNormalizedPosition = minPosition;
            this.minPosition = minPosition;
            this.maxPosition = maxPosition;
        }

        /// <summary>
        /// AuroraGUISplitView constructor.
        /// </summary>
        /// <param name="splitDirection">Split view direction.</param>
        /// <param name="minPosition">Min possible split view position, ranged: [0...1].</param>
        /// <param name="maxPosition">Max possible split view position, ranged: [0...1].</param>
        public GUISplitView(Direction splitDirection, float splitPosition, float minPosition, float maxPosition)
            : this(splitDirection, minPosition, maxPosition)
        {
            splitNormalizedPosition = Mathf.Clamp(splitPosition, minPosition, maxPosition);
        }

        public void BeginSplitView()
        {
            Rect beginSplitViewPosition = Rect.zero;

            if (splitDirection == Direction.Horizontal)
            {
                beginSplitViewPosition = EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            }
            else
            {
                beginSplitViewPosition = EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
            }

            if (beginSplitViewPosition.width > 0.0f)
            {
                currentRect = beginSplitViewPosition;
            }

            if (splitDirection == Direction.Horizontal)
            {
                scrollPosSideOne = GUILayout.BeginScrollView(scrollPosSideOne, GUILayout.Width(currentRect.width * splitNormalizedPosition));
            }
            else
            {
                scrollPosSideOne = GUILayout.BeginScrollView(scrollPosSideOne, GUILayout.Height(currentRect.height * splitNormalizedPosition));
            }
        }

        public void Split()
        {
            GUILayout.EndScrollView();
            ResizeSplitSeparator();
            if (splitDirection == Direction.Horizontal)
            {
                scrollPosSideTwo = GUILayout.BeginScrollView(scrollPosSideTwo);
            }
            else
            {
                scrollPosSideTwo = GUILayout.BeginScrollView(scrollPosSideTwo);
            }
        }

        public void EndSplitView()
        {
            GUILayout.EndScrollView();
            if (splitDirection == Direction.Horizontal)
            {
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.EndVertical();
            }
        }

        private void ResizeSplitSeparator()
        {
            if (splitDirection == Direction.Horizontal)
            {
                resizeHandleRect = new Rect((currentRect.width * splitNormalizedPosition) + offset, currentRect.y - 0.5f, 1f, currentRect.height + 0.5f);
            }
            else
            {
                resizeHandleRect = new Rect(currentRect.x, (currentRect.height * splitNormalizedPosition) + offset, currentRect.width, 1f);
            }

            EditorGUI.DrawRect(resizeHandleRect, Colors.Separator);

            Rect cursorHandlePosition = Rect.zero;
            if (splitDirection == Direction.Horizontal)
            {
                cursorHandlePosition = new Rect(resizeHandleRect.x - 5, resizeHandleRect.y, 10, resizeHandleRect.height);
                EditorGUIUtility.AddCursorRect(cursorHandlePosition, MouseCursor.ResizeHorizontal);
            }
            else
            {
                cursorHandlePosition = new Rect(resizeHandleRect.x, resizeHandleRect.y - 5, resizeHandleRect.width, 10);
                EditorGUIUtility.AddCursorRect(cursorHandlePosition, MouseCursor.ResizeVertical);
            }

            if (Event.current.type == EventType.MouseDown && cursorHandlePosition.Contains(Event.current.mousePosition))
            {
                resize = true;
            }

            if (resize)
            {
                if (splitDirection == Direction.Horizontal)
                {
                    splitNormalizedPosition = (Event.current.mousePosition.x - offset) / currentRect.width;
                }
                else
                {
                    splitNormalizedPosition = (Event.current.mousePosition.y - offset) / currentRect.height;
                }

                splitNormalizedPosition = Mathf.Clamp(splitNormalizedPosition, minPosition, maxPosition);
            }

            if (Event.current.type == EventType.MouseUp)
            {
                resize = false;
            }
        }

        #region [Getter / Setter]

        public Direction GetSplitDirection() { return splitDirection; }

        public void SetSplitDirection(Direction value) { splitDirection = value; }

        public float GetOffset() { return offset; }

        public void SetOffset(float value) { offset = value; }

        public float GetMinPosition() { return minPosition; }

        public void SetMinPosition(float value) { minPosition = value; }

        public float GetMaxPosition() { return maxPosition; }

        public void SetMaxPosition(float value) { maxPosition = value; }

        public bool IsResize() { return resize; }

        public Rect GetResizeHandlePosition() { return resizeHandleRect; }

        #endregion
    }
}