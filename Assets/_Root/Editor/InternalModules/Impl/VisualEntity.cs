using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine.Events;

namespace Pancake.Editor
{
    public abstract class VisualEntity : IVisualEntity, IEntityHeight, IEntityVisibility
    {
        private AnimFloat animHeight;

        #region [Abstract Methods]

        /// <summary>
        /// Called for rendering and handling visual entity.
        /// </summary>
        /// <param name="position">Rectangle position.</param>
        public abstract void OnGUI(Rect position);

        #endregion

        #region [IElementHeight Abstract Implementation]

        /// <summary>
        /// Visual entity height.
        /// </summary>
        public abstract float GetHeight();

        #endregion

        /// <summary>
        /// Draw visual entity with auto layout.
        /// </summary>
        public void DrawLayout()
        {
            if (IsVisible())
            {
                float targetHeight = GetHeight();

                if (EditorSettings.Current.AttributeAnimate())
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
            else if (EditorSettings.Current.AttributeAnimate())
            {
                CreateAnimFloatSafety(0);
            }
        }

        /// <summary>
        /// visual entity visibility state.
        /// </summary>
        public virtual bool IsVisible() { return true; }

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

        #region [Static Methods]

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

        #endregion

        #region [Event Callback Functions]

        public UnityAction Repaint = RepaintFocusedWindow;

        #endregion
    }
}