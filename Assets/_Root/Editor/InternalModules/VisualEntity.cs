using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.Events;

namespace Pancake.Editor
{
    public abstract class VisualEntity : IVisualEntity, IEntityHeight, IEntityVisibility, IEntityOrder
    {
        private int order;
        private AnimFloat animHeight;

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

        #region [IEntityOrder Implementation]

        public int GetOrder() { return order; }

        #endregion

        /// <summary>
        /// Create animated height value safety.
        /// <br><i>Will be created only if value is null.</i></br>
        /// </summary>
        /// <param name="target"></param>
        private void CreateAnimFloatSafety(in float target)
        {
            if (animHeight == null)
            {
                animHeight = new AnimFloat(target, Repaint) {speed = 5.5f};
            }
        }

        /// <summary>
        /// Draw visual entity with auto layout.
        /// </summary>
        public void DrawLayout()
        {
            if (IsVisible())
            {
                float targetHeight = GetHeight();

                if (EditorSettings.Current.Animate())
                {
                    CreateAnimFloatSafety(targetHeight);
                    if (animHeight.target != targetHeight)
                    {
                        animHeight.target = targetHeight;
                    }

                    targetHeight = animHeight.value;
                }

                Rect position = GUILayoutUtility.GetRect(0, targetHeight);
                OnGUI(position);
                GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
            }
            else if (EditorSettings.Current.Animate())
            {
                CreateAnimFloatSafety(0);
            }
        }

        /// <summary>
        /// Repaint EditorWindow which currently has keyboard focus.
        /// </summary>
        private static void RepaintFocusedWindow()
        {
            if (EditorWindow.focusedWindow != null)
            {
                EditorWindow.focusedWindow.Repaint();
            }
        }

        #region [Event Callback Functions]

        public UnityAction Repaint = RepaintFocusedWindow;

        #endregion

        #region [Getter / Setter]

        internal void SetOrder(int value) { order = value; }

        #endregion
    }
}