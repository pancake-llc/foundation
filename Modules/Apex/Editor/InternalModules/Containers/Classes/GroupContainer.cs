using System.Collections.Generic;
using UnityEngine;

namespace Pancake.ApexEditor
{
    public class GroupContainer : ListContainer
    {
        private static GUIStyle LabelStyle;

        static GroupContainer()
        {
            LabelStyle = new GUIStyle(ApexStyles.LabelBold);
            LabelStyle.alignment = TextAnchor.MiddleCenter;
        }

        private float headerHeight;

        /// <summary>
        /// Group container constructor.
        /// </summary>
        /// <param name="name">Group container name.</param>
        /// <param name="entities">Group container entities.</param>
        public GroupContainer(string name, List<VisualEntity> entities)
            : base(name, entities)
        {
            headerHeight = 22;
        }

        #region [VisualEntity Implementation]

        /// <summary>
        /// Called for rendering and handling group container.
        /// </summary>
        /// <param name="position">Rectangle position.</param>
        public sealed override void OnGUI(Rect position)
        {
            float contentHeight = position.height - headerHeight;

            position.height = headerHeight;
            GUI.Box(position, GUIContent.none, ApexStyles.BoxHeader);
            GUI.Label(position, GetName(), LabelStyle);

            position.y = position.yMax - 1;
            position.height = contentHeight;
            GUI.Box(position, GUIContent.none, ApexStyles.BoxEntryBkg);
            position.y += ApexGUIUtility.BoxBounds;

            using (new BoxScope(ref position))
            {
                OnContentGUI(position);
            }
        }

        /// <summary>
        /// Total height required to drawing group container.
        /// </summary>
        public sealed override float GetHeight() { return headerHeight + (ApexGUIUtility.BoxBounds * 2) + GetContentHeight(); }

        #endregion

        /// <summary>
        /// Called for rendering and handling group container content.
        /// </summary>
        /// <param name="position">Rectangle position.</param>
        protected virtual void OnContentGUI(Rect position) { base.OnGUI(position); }

        /// <summary>
        /// Total height required to drawing group container content.
        /// </summary>
        protected virtual float GetContentHeight() { return base.GetHeight(); }

        #region [Getter / Setter]

        public float GetHeaderHeight() { return headerHeight; }

        public void SetHeaderHeight(float value) { headerHeight = value; }

        #endregion
    }
}