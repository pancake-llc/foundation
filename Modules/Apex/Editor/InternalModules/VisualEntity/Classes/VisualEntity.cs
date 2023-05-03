using Pancake.Apex;
using System;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.Events;

namespace Pancake.ApexEditor
{
    public abstract class VisualEntity : IVisualEntity, IEntityHeight, IEntityVisibility, IEntityName, IEntityOrder, IComparable<VisualEntity>
    {
        private string name;
        private int order;
        private Rect position;
        private AnimFloat animHeight;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of visual entity.</param>
        public VisualEntity(string name) { this.name = name; }

        #region [IVisualEntity Implementation]

        /// <summary>
        /// Called for rendering and handling visual entity.
        /// </summary>
        /// <param name="position">Rectangle position.</param>
        public abstract void OnGUI(Rect position);

        #endregion

        #region [IElementHeight Implementation]

        /// <summary>
        /// Visual entity height.
        /// </summary>
        public abstract float GetHeight();

        #endregion

        #region [IEntityVisibility Implementation]

        /// <summary>
        /// Visual entity visibility state.
        /// </summary>
        public virtual bool IsVisible() { return true; }

        #endregion

        #region [IEntityName Implementation]

        public string GetName() { return name; }

        #endregion

        #region [IEntityOrder Implementation]

        public int GetOrder() { return order; }

        #endregion

        #region [IComparable<VisualEntity> Implementation]

        public virtual int CompareTo(VisualEntity other)
        {
            int thisOrder = order;
            int otherOrder = other.GetOrder();

            if (this is SerializedMember member)
            {
                OrderAttribute thisOrderAttribute = member.GetAttribute<OrderAttribute>();
                if (thisOrderAttribute != null)
                {
                    thisOrder = thisOrderAttribute.order;
                }
            }

            if (other is SerializedMember otherMember)
            {
                OrderAttribute otherOrderAttribute = otherMember.GetAttribute<OrderAttribute>();
                if (otherOrderAttribute != null)
                {
                    otherOrder = otherOrderAttribute.order;
                }
            }

            if (thisOrder > otherOrder)
                return 1;
            else if (thisOrder < otherOrder)
                return -1;
            else
                return 0;
        }

        #endregion

        /// <summary>
        /// Draw visual entity with auto layout.
        /// </summary>
        public void DrawLayout()
        {
            if (IsVisible())
            {
                float targetHeight = GetHeight();

                if (ApexGUIUtility.Animate)
                {
                    SafeCreateAnimFloat(targetHeight);
                    if (animHeight.target != targetHeight)
                    {
                        animHeight.target = targetHeight;
                    }

                    targetHeight = animHeight.value;
                }

                Rect controlRect = ApexGUILayout.GetControlRect(targetHeight);
                if (Event.current.type == EventType.Repaint)
                {
                    position = controlRect;
                }

                OnGUI(position);
            }
            else if (ApexGUIUtility.Animate)
            {
                SafeCreateAnimFloat(0);
            }
        }

        /// <summary>
        /// Create animated height value safety.
        /// <br><i>Will be created only if value is null.</i></br>
        /// </summary>
        /// <param name="target"></param>
        private void SafeCreateAnimFloat(in float target)
        {
            if (animHeight == null)
            {
                animHeight = new AnimFloat(target, Repaint) {speed = 5.5f};
            }
        }

        /// <summary>
        /// Repaint editor window which currently has mouse focus.
        /// </summary>
        private static void RepaintWindow()
        {
            if (EditorWindow.mouseOverWindow != null)
            {
                EditorWindow.mouseOverWindow.Repaint();
            }
        }

        #region [Event Callback Functions]

        public UnityAction Repaint = RepaintWindow;

        #endregion

        #region [Getter / Setter]

        public void SetName(string value) { name = value; }

        public void SetOrder(int value) { order = value; }

        #endregion
    }
}