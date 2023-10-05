using System;
using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    public sealed class HandlesSpace : VisualEntity
    {
        private Action<Vector2> onGUI;
        private Func<float> getHeight;
        private bool showTitle;
        private bool showBackground;

        // Stored style properties.
        private GUIStyle titleStyle;

        public HandlesSpace(string name, Action<Vector2> onGUI, Func<float> getHeight, bool showTitle = false, bool showBackground = false)
            : base(name)
        {
            this.onGUI = onGUI;
            this.getHeight = getHeight;
            this.showTitle = showTitle;
            this.showBackground = showBackground;
        }

        #region [VisualEntity Implementation]

        /// <summary>
        /// Called for rendering and handling visual entity.
        /// </summary>
        /// <param name="position">Rectangle position.</param>
        public override void OnGUI(Rect position)
        {
            float totalHeight = position.height;

            if (showTitle)
            {
                if (titleStyle == null)
                {
                    titleStyle = new GUIStyle(EditorStyles.boldLabel);
                    titleStyle.fontSize = 12;
                    titleStyle.alignment = TextAnchor.MiddleLeft;
                }

                position.height = EditorGUIUtility.singleLineHeight;
                GUI.Label(position, GetName(), titleStyle);
                position.y = position.yMax + EditorGUIUtility.standardVerticalSpacing;
                totalHeight -= position.height + EditorGUIUtility.standardVerticalSpacing;
            }


            if (showBackground)
            {
                position.height = totalHeight;
                GUI.Box(position, GUIContent.none, ApexStyles.BoxEntryBkg);
            }

            GUI.BeginClip(position);
            onGUI.Invoke(position.size);
            GUI.EndClip();
        }

        /// <summary>
        /// Total height required to drawing float field.
        /// </summary>
        public override float GetHeight() { return getHeight.Invoke(); }

        #endregion

        #region [Getter / Setter]

        public bool GetShowTitle() { return showTitle; }

        public void SetShowTitle(bool value) { showTitle = value; }

        public bool GetShowBackground() { return showBackground; }

        public void SetShowBackground(bool value) { showBackground = value; }

        public GUIStyle GetTitleStyle() { return titleStyle; }

        public void SetTitleStyle(GUIStyle value) { titleStyle = value; }

        #endregion
    }
}