using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    public sealed class HorizontalContainer : EntityListContainer
    {
        /// <summary>
        /// Horizontal group constructor.
        /// </summary>
        /// <param name="children"></param>
        public HorizontalContainer(string name, List<VisualEntity> children)
            : base(name, children)
        {
        }

        /// <summary>
        /// Called for rendering and handling layout element.
        /// </summary>
        /// <param name="position">Rectangle position.</param>
        public override void OnGUI(Rect position)
        {
            int count = 0;
            for (int i = 0; i < GetChildrenCount(); i++)
            {
                if (GetChild(i).IsVisible())
                {
                    count++;
                }
            }

            for (int i = 0; i < count; i++)
            {
                VisualEntity entity = GetChild(i);
                if (entity.IsVisible())
                {
                    Rect childPosition = new Rect(position.position.x + (i * position.width / count) - (EditorGUI.indentLevel * 15),
                        position.position.y,
                        (position.width / count) + (EditorGUI.indentLevel * 15),
                        entity.GetHeight());

                    entity.OnGUI(childPosition);
                }
            }
        }

        /// <summary>
        /// Horizontal button group height.
        /// </summary>
        public override float GetHeight()
        {
            float maxHeight = 0;
            for (int i = 0; i < GetChildrenCount(); i++)
            {
                float height = GetChild(i).GetHeight();
                if (maxHeight < height)
                {
                    maxHeight = height;
                }
            }

            return maxHeight > 0 ? maxHeight : 17.0f;
        }

        /// <summary>
        /// Horizontal group visibility state.
        /// </summary>
        public override bool IsVisible()
        {
            for (int i = 0; i < GetChildrenCount(); i++)
            {
                if (GetChild(i).IsVisible())
                {
                    return true;
                }
            }

            return false;
        }

        #region [Static Methods]

        /// <summary>
        /// Split specified rectangle position.
        /// </summary>
        /// <param name="totalPosition">Original position.</param>
        /// <param name="count">Split count.</param>
        public static Rect[] SplitRectangle(Rect totalPosition, int count)
        {
            Rect[] rects = new Rect[count];
            for (int i = 0; i < count; i++)
            {
                rects[i] = new Rect(totalPosition.position.x + (i * totalPosition.width / count),
                    totalPosition.position.y,
                    (totalPosition.width / count),
                    totalPosition.height);
            }

            return rects;
        }

        /// <summary>
        /// Split specified rectangle position.
        /// </summary>
        /// <param name="totalPosition">Original position.</param>
        /// <param name="count">Split count.</param>
        public static Rect[] SplitIndentRectangle(Rect totalPosition, int count)
        {
            Rect[] rects = new Rect[count];
            for (int i = 0; i < count; i++)
            {
                rects[i] = new Rect(totalPosition.position.x + (i * totalPosition.width / count) - (EditorGUI.indentLevel * 15),
                    totalPosition.position.y,
                    (totalPosition.width / count) + (EditorGUI.indentLevel * 15),
                    totalPosition.height);
            }

            return rects;
        }

        #endregion
    }
}