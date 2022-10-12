using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    public class GroupContainer : EntityListContainer
    {
        /// <summary>
        /// Box group constructor.
        /// </summary>
        /// <param name="title">Foldout title content.</param>
        /// <param name="children">Foldout children elements.</param>
        public GroupContainer(string name, List<VisualEntity> children)
            : base(name, children)
        {
        }

        /// <summary>
        /// Called for rendering and handling layout element.
        /// </summary>
        /// <param name="position">Rectangle position.</param>
        public override void OnGUI(Rect position)
        {
            using (new ContainerScope(ref position))
            {
                const float HEADER_HEIGHT = 24;
                position.height = Mathf.Max(0, position.height - HEADER_HEIGHT);
                if (position.height >= 0)
                {
                    Rect headerPosition = EditorGUI.IndentedRect(position);
                    headerPosition.height = HEADER_HEIGHT;
                    GUI.Box(headerPosition, GUIContent.none, Uniform.Header);
                    GUI.Label(headerPosition, GetName(), Uniform.HeaderLabel);

                    Rect contentPosition = new Rect(headerPosition.x, headerPosition.yMax - 1, headerPosition.width, position.height);
                    GUI.Box(contentPosition, GUIContent.none, Uniform.ContentBackground);

                    EditorGUI.indentLevel++;
                    Rect childrenPosition = new Rect(position.x,
                        headerPosition.yMax + EditorGUIUtility.standardVerticalSpacing,
                        position.width - 2,
                        contentPosition.height);
                    base.OnGUI(childrenPosition);
                    EditorGUI.indentLevel--;
                }
            }
        }

        /// <summary>
        /// Container elements height.
        /// </summary>
        public override float GetHeight()
        {
            const float HEADER_HEIGHT = 30;
            return base.GetHeight() + HEADER_HEIGHT;
        }

        #region [Getter / Setter]

        public string GetTitle() { return GetName(); }

        public void SetGroupName(string value) { SetName(value); }

        #endregion
    }
}